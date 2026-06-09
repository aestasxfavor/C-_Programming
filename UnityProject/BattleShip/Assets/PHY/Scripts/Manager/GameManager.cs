using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    private static bool isReplaySceneReloading;

    #region Inspector Fields

    [Header("게임 상태")]
    [SerializeField] private GameState gameState = GameState.None;

    [Header("Ready 상태")]
    [SerializeField] private bool isMyReady;
    [SerializeField] private bool isOpponentReady;
    [SerializeField] private bool isPlacementLocked;

    [Header("보드 연결")]
    [SerializeField] private BoardView myBoardView;
    [SerializeField] private BoardView enemyBoardView;

    [Header("UI 전환")]
    [SerializeField] private GameObject shipCanvas;
    [SerializeField] private GameObject enemyBoardPanel;

    [Header("로컬 테스트")]
    [SerializeField] private bool useLocalBattleTest;

    [Header("전투 턴 상태")]
    [SerializeField] private bool isMyTurn;
    [SerializeField] private bool isWaitingResult;
    [SerializeField] private bool isGameOver;

    [Header("재시작 상태")]
    [SerializeField] private bool isMyReplayReady;
    [SerializeField] private bool isOpponentReplayReady;
    [SerializeField] private bool isRestartingByReplay;
    [SerializeField] private bool isClearingReplayReloadFlag;

    [Header("나가기 상태")]
    [SerializeField] private bool isReturningToTitleByLeave;

    [Header("게임 종료 UI")]
    [SerializeField] private GameObject winCanvas;
    [SerializeField] private GameObject loseCanvas;

    [Header("연결 끊김 UI")]
    [SerializeField] private GameObject disconnectPanel;
    [SerializeField] private bool isNetworkDisconnected;

    [Header("턴 시간 제한")]
    [SerializeField] private float turnTimeLimit = 15f;
    [SerializeField] private float turnTimer;
    [SerializeField] private bool isTurnTimerRunning;

    [Header("턴 시간 UI")]
    [SerializeField] private TextMeshProUGUI turnTimerText;

    [Header("상태 UI")]
    [SerializeField] private TextMeshProUGUI statusText;

    [Header("함선 상태 UI")]
    [SerializeField] private GameObject shipStatusHeader;
    [SerializeField] private Image[] myShipIcons;
    [SerializeField] private Image[] enemyShipIcons;
    [SerializeField] private Color normalShipColor = Color.white;
    [SerializeField] private Color sunkShipColor = new Color(0.35f, 0.35f, 0.35f, 1f);

    [Header("씬 이름")]
    [SerializeField] private string titleSceneName = "Title";
    [SerializeField] private string battleSceneName = "Game";

    [SerializeField] private bool isPendingReadySend;

    #endregion

    #region Properties

    public bool IsPlacementLocked => isPlacementLocked;
    public bool IsBattle => gameState == GameState.Battle;
    public GameState CurrentState => gameState;

    #endregion

    #region Unity Event Methods

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        gameState = GameState.Placement;

        isNetworkDisconnected = false;
        isRestartingByReplay = isReplaySceneReloading;
        isReturningToTitleByLeave = false;
        isClearingReplayReloadFlag = false;

        ShowPlacementUI();
        HideGameOverUI();
        HideDisconnectUI();
        ResetShipStatusUI();
        UpdateTurnTimerUI();
        UpdateStatusText();

        if (isRestartingByReplay)
        {
            Debug.Log("[Replay] Replay 씬 재시작 상태 유지 중");
            StartCoroutine(ClearReplayStateAfterSceneReload());
        }

        Debug.Log("[GameManager] Placement 단계 시작");
    }

    private void Update()
    {
        UpdateTurnTimer();
        TrySendPendingReady();
        TryClearReplaySceneReloadFlagWhenReconnected();
    }

    #endregion

    #region Ready Flow

    public void OnClickReadyButton()
    {
        if (isNetworkDisconnected)
        {
            Debug.Log("[Network] 연결 끊김 상태라 Ready 불가");
            return;
        }

        if (isRestartingByReplay || isReturningToTitleByLeave)
        {
            Debug.Log("[Ready] 씬 전환 중이라 Ready 불가");
            return;
        }

        if (isMyReady)
        {
            Debug.Log("[Ready] 이미 Ready 상태");
            return;
        }

        if (!IsAllShipsPlaced())
        {
            Debug.LogWarning("[Ready] 배 5척을 모두 배치해야 Ready 가능");
            return;
        }

        isMyReady = true;
        isPlacementLocked = true;

        Debug.Log("[Ready] 내 Ready 완료");
        Debug.Log("[Placement] 배치 수정 잠금");

        if (useLocalBattleTest)
        {
            Debug.Log("[Ready] 로컬 전투 테스트 모드라 바로 Battle 진입");

            isOpponentReady = true;
            TryStartBattle();

            return;
        }

        gameState = GameState.WaitingReady;
        UpdateStatusText();

        if (TCPManager.Instance != null && TCPManager.Instance.IsConnected)
        {
            TrySendPacket(PacketProtocol.READY);
            isPendingReadySend = false;

            Debug.Log("[Ready] READY 패킷 즉시 전송");
        }
        else
        {
            isPendingReadySend = true;

            Debug.Log("[Ready] TCP 연결 전이라 READY 패킷 전송 예약");
        }

        TryStartBattle();
    }

    private void OnOpponentReady()
    {
        if (isOpponentReady)
        {
            return;
        }

        isOpponentReady = true;
        UpdateStatusText();

        Debug.Log("[Ready] 상대 Ready 수신");

        TryStartBattle();
    }

    private void TrySendPendingReady()
    {
        if (isNetworkDisconnected)
        {
            return;
        }

        if (isRestartingByReplay || isReturningToTitleByLeave)
        {
            return;
        }

        if (!isPendingReadySend)
        {
            return;
        }

        if (useLocalBattleTest)
        {
            return;
        }

        if (!isMyReady)
        {
            return;
        }

        if (TCPManager.Instance == null || !TCPManager.Instance.IsConnected)
        {
            return;
        }

        TrySendPacket(PacketProtocol.READY);
        isPendingReadySend = false;

        Debug.Log("[Ready] 예약된 READY 패킷 전송 완료");

        TryStartBattle();
    }

    private bool IsAllShipsPlaced()
    {
        if (myBoardView == null)
        {
            Debug.LogError("[Ready] myBoardView 연결 필요");
            return false;
        }

        return myBoardView.IsAllShipsPlaced();
    }

    private void TryStartBattle()
    {
        if (isNetworkDisconnected)
        {
            return;
        }

        if (isRestartingByReplay || isReturningToTitleByLeave)
        {
            return;
        }

        if (!isMyReady)
        {
            Debug.Log("[Ready] 내 Ready 대기 중");
            return;
        }

        if (!isOpponentReady)
        {
            Debug.Log("[Ready] 상대 Ready 대기 중");
            return;
        }

        if (gameState == GameState.Battle)
        {
            return;
        }

        gameState = GameState.Battle;

        isGameOver = false;
        isWaitingResult = false;
        isMyReplayReady = false;
        isOpponentReplayReady = false;
        isRestartingByReplay = false;
        isReturningToTitleByLeave = false;

        if (useLocalBattleTest)
        {
            isMyTurn = true;
        }
        else
        {
            isMyTurn = TCPManager.Instance != null && TCPManager.Instance.IsHost;
        }

        ShowBattleUI();

        Debug.Log("[Battle] 양쪽 Ready 완료, 전투 단계 진입");

        if (isMyTurn)
        {
            Debug.Log("[Turn] 내 턴 시작");
            StartTurnTimer();
        }
        else
        {
            Debug.Log("[Turn] 상대 턴 대기");
            UpdateStatusText();
            StopTurnTimer();
        }
    }

    #endregion

    #region Replay Reload Guard Flow

    private void MarkReplaySceneReloading()
    {
        isReplaySceneReloading = true;
        isRestartingByReplay = true;
        isClearingReplayReloadFlag = false;

        UpdateStatusText();

        Debug.Log("[Replay] Replay 씬 재시작 플래그 설정");
    }

    private IEnumerator ClearReplayStateAfterSceneReload()
    {
        yield return new WaitForSecondsRealtime(0.7f);

        if (gameState != GameState.Placement)
        {
            yield break;
        }

        isReplaySceneReloading = false;
        isRestartingByReplay = false;
        isClearingReplayReloadFlag = false;

        UpdateStatusText();

        Debug.Log("[Replay] 씬 재시작 후 Placement 상태로 전환 완료");
    }

    private void TryClearReplaySceneReloadFlagWhenReconnected()
    {
        if (!isReplaySceneReloading)
        {
            return;
        }

        if (isClearingReplayReloadFlag)
        {
            return;
        }

        if (TCPManager.Instance == null)
        {
            return;
        }

        if (!TCPManager.Instance.IsConnected)
        {
            return;
        }

        StartCoroutine(ClearReplaySceneReloadFlagAfterDelay());
    }

    private IEnumerator ClearReplaySceneReloadFlagAfterDelay()
    {
        isClearingReplayReloadFlag = true;

        yield return new WaitForSecondsRealtime(0.7f);

        if (TCPManager.Instance != null && TCPManager.Instance.IsConnected)
        {
            isReplaySceneReloading = false;
            isRestartingByReplay = false;
            isClearingReplayReloadFlag = false;

            UpdateStatusText();

            Debug.Log("[Replay] TCP 재연결 확인, Replay 씬 재시작 플래그 해제");
            yield break;
        }

        isClearingReplayReloadFlag = false;
    }

    #endregion

    #region Packet Receive Flow

    public void OnReceivePacket(string packet)
    {
        if (string.IsNullOrEmpty(packet))
        {
            return;
        }

        if (isNetworkDisconnected)
        {
            Debug.Log($"[Packet] 연결 끊김 상태라 패킷 무시: {packet}");
            return;
        }

        Debug.Log($"[Packet Received] {packet}");

        string[] split = packet.Split('|');

        switch (split[0])
        {
            case PacketProtocol.READY:
                OnOpponentReady();
                break;

            case PacketProtocol.ATTACK:
                OnReceiveAttackPacket(split);
                break;

            case PacketProtocol.RESULT:
                OnReceiveResultPacket(split);
                break;

            case PacketProtocol.GAME_OVER:
                OnReceiveGameOverPacket();
                break;

            case PacketProtocol.TURN_TIMEOUT:
                OnReceiveTurnTimeoutPacket();
                break;

            case PacketProtocol.REPLAY_READY:
                OnReceiveReplayReadyPacket();
                break;

            case PacketProtocol.REPLAY_START:
                OnReceiveReplayStartPacket();
                break;

            case PacketProtocol.LEAVE:
                OnReceiveLeavePacket();
                break;

            default:
                Debug.LogWarning($"[Packet] 알 수 없는 패킷: {packet}");
                break;
        }
    }

    private void OnReceiveAttackPacket(string[] split)
    {
        if (split.Length < 3)
        {
            Debug.LogWarning("[Attack] ATTACK 패킷 형식 오류");
            return;
        }

        if (gameState != GameState.Battle)
        {
            Debug.Log("[Attack] Battle 단계가 아니라 ATTACK 무시");
            return;
        }

        if (isGameOver)
        {
            Debug.Log("[Attack] 이미 게임 종료 상태라 ATTACK 무시");
            return;
        }

        if (isRestartingByReplay || isReturningToTitleByLeave)
        {
            Debug.Log("[Attack] 씬 전환 중이라 ATTACK 무시");
            return;
        }

        if (myBoardView == null)
        {
            Debug.LogError("[Attack] myBoardView 연결 필요");
            return;
        }

        if (!int.TryParse(split[1], out int x) || !int.TryParse(split[2], out int y))
        {
            Debug.LogWarning("[Attack] 좌표 파싱 실패");
            return;
        }

        Debug.Log($"[Attack] 공격 패킷 수신 X={x}, Y={y}");

        AttackResult result = myBoardView.ReceiveAttack(x, y);
        string resultText = ConvertAttackResultToPacketText(result);

        string sunkShipId = "-";
        string aroundPositionsText = "-";

        if (result == AttackResult.Sunk || result == AttackResult.GameOver)
        {
            sunkShipId = myBoardView.GetLastSunkShipId();
            aroundPositionsText = myBoardView.GetLastSunkAroundPositionsText();

            if (string.IsNullOrEmpty(sunkShipId))
            {
                sunkShipId = "-";
            }

            if (string.IsNullOrEmpty(aroundPositionsText))
            {
                aroundPositionsText = "-";
            }

            MarkMyShipSunk(sunkShipId);
        }

        string resultPacket = $"{PacketProtocol.RESULT}|{x}|{y}|{resultText}|{sunkShipId}|{aroundPositionsText}";

        TrySendPacket(resultPacket);

        Debug.Log($"[Result] 결과 패킷 전송 {resultText}, X={x}, Y={y}, Ship={sunkShipId}");

        if (result == AttackResult.Invalid)
        {
            Debug.Log("[Attack] 유효하지 않은 공격이라 턴 변경 없음");
            return;
        }

        if (result == AttackResult.GameOver)
        {
            SetGameOver(false);
            return;
        }

        isWaitingResult = false;

        if (result == AttackResult.Miss)
        {
            isMyTurn = true;

            Debug.Log("[Turn] 상대 공격이 빗나감, 내 턴 시작");

            UpdateStatusText();
            StartTurnTimer();
        }
        else if (result == AttackResult.Hit || result == AttackResult.Sunk)
        {
            isMyTurn = false;

            Debug.Log("[Turn] 상대 공격이 명중함, 상대 턴 유지");

            UpdateStatusText();
            StopTurnTimer();
        }
    }

    private void OnReceiveResultPacket(string[] split)
    {
        if (split.Length < 6)
        {
            Debug.LogWarning("[Result] RESULT 패킷 형식 오류");
            return;
        }

        if (!int.TryParse(split[1], out int x) || !int.TryParse(split[2], out int y))
        {
            Debug.LogWarning("[Result] 좌표 파싱 실패");
            return;
        }

        string resultText = split[3];
        string sunkShipId = split[4];
        string aroundPositionsText = split[5];

        if (sunkShipId == "-")
        {
            sunkShipId = "";
        }

        if (aroundPositionsText == "-")
        {
            aroundPositionsText = "";
        }

        Debug.Log($"[Result] 결과 수신 {resultText}, X={x}, Y={y}, Ship={sunkShipId}");

        isWaitingResult = false;

        if (resultText == "INVALID")
        {
            isMyTurn = true;

            Debug.Log("[Result] INVALID 수신, 내 턴 유지");

            UpdateStatusText();
            StartTurnTimer();
            return;
        }

        if (enemyBoardView == null)
        {
            Debug.LogError("[Result] enemyBoardView 연결 필요");
            return;
        }

        enemyBoardView.ApplyAttackResult(x, y, resultText, aroundPositionsText);

        if (!string.IsNullOrEmpty(sunkShipId))
        {
            MarkEnemyShipSunk(sunkShipId);
        }

        if (resultText == "GAME_OVER")
        {
            SetGameOver(true);
            return;
        }

        if (resultText == "MISS")
        {
            isMyTurn = false;

            Debug.Log("[Turn] 공격 실패, 상대 턴 대기");

            UpdateStatusText();
            StopTurnTimer();
        }
        else if (resultText == "HIT" || resultText == "SUNK")
        {
            isMyTurn = true;

            Debug.Log("[Turn] 공격 성공, 내 턴 유지");

            UpdateStatusText();
            StartTurnTimer();
        }
    }

    private void OnReceiveGameOverPacket()
    {
        if (isGameOver)
        {
            return;
        }

        SetGameOver(true);
    }

    private void OnReceiveTurnTimeoutPacket()
    {
        if (gameState != GameState.Battle)
        {
            return;
        }

        if (isGameOver)
        {
            return;
        }

        if (isRestartingByReplay || isReturningToTitleByLeave)
        {
            return;
        }

        isMyTurn = true;
        isWaitingResult = false;

        Debug.Log("[TurnTimer] 상대 시간 초과, 내 턴 시작");

        UpdateStatusText();
        StartTurnTimer();
    }

    private void OnReceiveReplayReadyPacket()
    {
        if (gameState != GameState.GameOver)
        {
            Debug.Log("[Replay] 게임 종료 상태가 아니라 REPLAY_READY 무시");
            return;
        }

        if (isOpponentReplayReady)
        {
            return;
        }

        isOpponentReplayReady = true;

        Debug.Log("[Replay] 상대 재시작 Ready 수신");

        TryRestartBattleScene();
    }

    private void OnReceiveReplayStartPacket()
    {
        if (isRestartingByReplay)
        {
            return;
        }

        Debug.Log("[Replay] 재시작 시작 패킷 수신");

        MarkReplaySceneReloading();

        StopTurnTimer();

        StartCoroutine(RestartBattleSceneAfterDelay());
    }

    private void OnReceiveLeavePacket()
    {
        if (isReturningToTitleByLeave)
        {
            return;
        }

        Debug.Log("[Network] 상대가 매치에서 나감");

        isReturningToTitleByLeave = true;
        isNetworkDisconnected = true;
        isMyTurn = false;
        isWaitingResult = false;
        isGameOver = true;
        isPlacementLocked = true;
        isPendingReadySend = false;
        isMyReplayReady = false;
        isOpponentReplayReady = false;

        UpdateStatusText();
        StopTurnTimer();

        HideGameOverUI();
        ShowDisconnectUI();

        StartCoroutine(ReturnToTitleAfterLeave());
    }

    #endregion

    #region Network Disconnect Flow

    public void OnNetworkDisconnected()
    {
        if (isReplaySceneReloading || isRestartingByReplay)
        {
            Debug.Log("[Network] Replay 씬 재시작 중이라 연결 끊김 알림 무시");
            return;
        }

        if (isReturningToTitleByLeave)
        {
            Debug.Log("[Network] Leave 처리 중이라 연결 끊김 알림 무시");
            return;
        }

        if (isNetworkDisconnected)
        {
            return;
        }

        if (useLocalBattleTest)
        {
            return;
        }

        isNetworkDisconnected = true;

        Debug.Log("[Network] 상대와의 연결이 끊김");

        isMyTurn = false;
        isWaitingResult = false;
        isGameOver = true;
        isPlacementLocked = true;
        isPendingReadySend = false;
        isMyReplayReady = false;
        isOpponentReplayReady = false;

        UpdateStatusText();
        StopTurnTimer();

        HideGameOverUI();
        ShowDisconnectUI();

        StartCoroutine(ReturnToTitleAfterDisconnect());
    }

    private IEnumerator ReturnToTitleAfterDisconnect()
    {
        yield return new WaitForSecondsRealtime(2f);

        SceneManager.LoadScene(titleSceneName);
    }

    private IEnumerator ReturnToTitleAfterLeave()
    {
        yield return new WaitForSecondsRealtime(2f);

        SceneManager.LoadScene(titleSceneName);
    }

    private IEnumerator ReturnToTitleAfterSendLeave()
    {
        yield return new WaitForSecondsRealtime(0.25f);

        SceneManager.LoadScene(titleSceneName);
    }

    private void ShowDisconnectUI()
    {
        if (disconnectPanel != null)
        {
            disconnectPanel.SetActive(true);
        }

        UpdateStatusText();
    }

    private void HideDisconnectUI()
    {
        if (disconnectPanel != null)
        {
            disconnectPanel.SetActive(false);
        }
    }

    #endregion

    #region Attack Flow

    public void TryAttackEnemyBoard(int x, int y)
    {
        if (isNetworkDisconnected)
        {
            Debug.Log("[Network] 연결 끊김 상태라 공격 불가");
            return;
        }

        if (isRestartingByReplay || isReturningToTitleByLeave)
        {
            Debug.Log("[Battle] 씬 전환 중이라 공격 불가");
            return;
        }

        if (gameState != GameState.Battle)
        {
            Debug.Log("[Battle] 전투 단계가 아니라 공격 불가");
            return;
        }

        if (isGameOver)
        {
            Debug.Log("[Battle] 이미 게임 종료 상태");
            return;
        }

        if (enemyBoardView == null)
        {
            Debug.LogError("[Battle] enemyBoardView 연결 필요");
            return;
        }

        if (useLocalBattleTest)
        {
            if (!enemyBoardView.CanRequestAttack(x, y))
            {
                Debug.Log($"[Battle] 공격 불가 칸 X={x}, Y={y}");
                return;
            }

            AttackResult localResult = enemyBoardView.ReceiveAttack(x, y);

            Debug.Log($"[Battle] 로컬 공격 결과: {localResult}, X={x}, Y={y}");

            if (localResult == AttackResult.Sunk || localResult == AttackResult.GameOver)
            {
                string localSunkShipId = enemyBoardView.GetLastSunkShipId();
                MarkEnemyShipSunk(localSunkShipId);
            }

            if (localResult == AttackResult.GameOver)
            {
                SetGameOver(true);
            }

            return;
        }

        if (!isMyTurn)
        {
            Debug.Log("[Turn] 내 턴이 아니라 공격 불가");
            return;
        }

        if (isWaitingResult)
        {
            Debug.Log("[Turn] 이전 공격 결과 대기 중");
            return;
        }

        if (!enemyBoardView.CanRequestAttack(x, y))
        {
            Debug.Log($"[Battle] 공격 불가 칸 X={x}, Y={y}");
            return;
        }

        string packet = $"{PacketProtocol.ATTACK}|{x}|{y}";

        if (!TrySendPacket(packet))
        {
            return;
        }

        isWaitingResult = true;

        UpdateStatusText();
        StopTurnTimer();

        Debug.Log($"[Attack] 공격 패킷 전송 X={x}, Y={y}");
    }

    #endregion

    #region Game Over Flow

    private void SetGameOver(bool isWin)
    {
        if (isNetworkDisconnected)
        {
            return;
        }

        if (isRestartingByReplay || isReturningToTitleByLeave)
        {
            return;
        }

        isGameOver = true;
        isMyTurn = false;
        isWaitingResult = false;

        StopTurnTimer();

        gameState = GameState.GameOver;

        ShowGameOverUI(isWin);
        UpdateStatusText();

        if (isWin)
        {
            Debug.Log("[GameOver] 승리");
        }
        else
        {
            Debug.Log("[GameOver] 패배");
        }
    }

    public void OnClickReplayButton()
    {
        if (isNetworkDisconnected)
        {
            Debug.Log("[Network] 연결 끊김 상태라 Replay 불가");
            return;
        }

        if (isReturningToTitleByLeave)
        {
            Debug.Log("[Replay] 타이틀 복귀 중이라 Replay 불가");
            return;
        }

        if (isRestartingByReplay)
        {
            Debug.Log("[Replay] 이미 재시작 진행 중");
            return;
        }

        if (gameState != GameState.GameOver)
        {
            Debug.Log("[Replay] 게임 종료 상태가 아니라 재시작 불가");
            return;
        }

        if (isMyReplayReady)
        {
            Debug.Log("[Replay] 이미 재시작 Ready 상태");
            return;
        }

        isMyReplayReady = true;

        Debug.Log("[Replay] 내 재시작 Ready");

        TrySendPacket(PacketProtocol.REPLAY_READY);

        TryRestartBattleScene();
    }

    public void OnClickExitButton()
    {
        if (isNetworkDisconnected)
        {
            Debug.Log("[Network] 연결 끊김 상태라 Exit 무시");
            return;
        }

        if (isReturningToTitleByLeave)
        {
            return;
        }

        Debug.Log("[UI] Exit 버튼 클릭");

        isReturningToTitleByLeave = true;
        isMyTurn = false;
        isWaitingResult = false;
        isGameOver = true;
        isPlacementLocked = true;

        UpdateStatusText();
        StopTurnTimer();
        HideGameOverUI();

        if (TCPManager.Instance != null && TCPManager.Instance.IsConnected)
        {
            TrySendPacket(PacketProtocol.LEAVE);
            StartCoroutine(ReturnToTitleAfterSendLeave());
            return;
        }

        SceneManager.LoadScene(titleSceneName);
    }

    private void TryRestartBattleScene()
    {
        if (isNetworkDisconnected)
        {
            return;
        }

        if (isReturningToTitleByLeave)
        {
            return;
        }

        if (!isMyReplayReady)
        {
            Debug.Log("[Replay] 내 재시작 Ready 대기");
            return;
        }

        if (!isOpponentReplayReady)
        {
            Debug.Log("[Replay] 상대 재시작 Ready 대기");
            return;
        }

        if (isRestartingByReplay)
        {
            return;
        }

        Debug.Log("[Replay] 양쪽 재시작 Ready 완료, BattleScene 재시작 준비");

        MarkReplaySceneReloading();

        TrySendPacket(PacketProtocol.REPLAY_START);

        StopTurnTimer();

        StartCoroutine(RestartBattleSceneAfterDelay());
    }

    private IEnumerator RestartBattleSceneAfterDelay()
    {
        yield return new WaitForSecondsRealtime(0.5f);

        SceneManager.LoadScene(battleSceneName);
    }

    #endregion

    #region UI Flow

    private void ShowPlacementUI()
    {
        if (shipCanvas != null)
        {
            shipCanvas.SetActive(true);
        }

        if (enemyBoardPanel != null)
        {
            enemyBoardPanel.SetActive(false);
        }

        SetShipStatusChildrenVisible(false);

        UpdateStatusText();
    }

    private void ShowBattleUI()
    {
        if (shipCanvas != null)
        {
            shipCanvas.SetActive(false);
        }

        if (enemyBoardPanel != null)
        {
            enemyBoardPanel.SetActive(true);
        }

        SetShipStatusChildrenVisible(true);

        Debug.Log("[UI] 함선 상태바 자식 표시");

        UpdateStatusText();
    }

    private void SetShipStatusChildrenVisible(bool isVisible)
    {
        if (shipStatusHeader == null)
        {
            return;
        }

        shipStatusHeader.SetActive(true);

        for (int i = 0; i < shipStatusHeader.transform.childCount; i++)
        {
            Transform child = shipStatusHeader.transform.GetChild(i);
            SetActiveRecursive(child, isVisible);
        }
    }

    private void SetActiveRecursive(Transform target, bool isActive)
    {
        if (target == null)
        {
            return;
        }

        target.gameObject.SetActive(isActive);

        for (int i = 0; i < target.childCount; i++)
        {
            SetActiveRecursive(target.GetChild(i), isActive);
        }
    }

    private void HideGameOverUI()
    {
        if (winCanvas != null)
        {
            winCanvas.SetActive(false);
        }

        if (loseCanvas != null)
        {
            loseCanvas.SetActive(false);
        }
    }

    private void ShowGameOverUI(bool isWin)
    {
        if (winCanvas != null)
        {
            winCanvas.SetActive(isWin);
        }

        if (loseCanvas != null)
        {
            loseCanvas.SetActive(!isWin);
        }

        UpdateStatusText();
    }

    private void UpdateStatusText()
    {
        if (statusText == null)
        {
            return;
        }

        if (isNetworkDisconnected)
        {
            statusText.text = "연결 끊김";
            return;
        }

        if (isReturningToTitleByLeave)
        {
            statusText.text = "매치 종료 중";
            return;
        }

        if (isRestartingByReplay)
        {
            statusText.text = "다시 시작 준비 중";
            return;
        }

        if (gameState == GameState.Placement)
        {
            statusText.text = "함선 배치 중";
            return;
        }

        if (gameState == GameState.WaitingReady)
        {
            statusText.text = "상대 준비 대기 중";
            return;
        }

        if (gameState == GameState.GameOver)
        {
            statusText.text = "게임 종료";
            return;
        }

        if (gameState == GameState.Battle)
        {
            if (isWaitingResult)
            {
                statusText.text = "공격 결과 대기 중";
                return;
            }

            statusText.text = isMyTurn ? "내 차례" : "상대 차례";
            return;
        }

        statusText.text = "";
    }

    private void ResetShipStatusUI()
    {
        if (myShipIcons != null)
        {
            for (int i = 0; i < myShipIcons.Length; i++)
            {
                if (myShipIcons[i] != null)
                {
                    myShipIcons[i].color = normalShipColor;
                }
            }
        }

        if (enemyShipIcons != null)
        {
            for (int i = 0; i < enemyShipIcons.Length; i++)
            {
                if (enemyShipIcons[i] != null)
                {
                    enemyShipIcons[i].color = normalShipColor;
                }
            }
        }
    }

    private int GetShipIconIndex(string shipId)
    {
        switch (shipId)
        {
            case "Ship2":
                return 0;

            case "Ship3A":
                return 1;

            case "Ship3B":
                return 2;

            case "Ship4":
                return 3;

            case "Ship5":
                return 4;

            default:
                return -1;
        }
    }

    private void MarkMyShipSunk(string shipId)
    {
        int index = GetShipIconIndex(shipId);

        if (index < 0 || myShipIcons == null || index >= myShipIcons.Length)
        {
            return;
        }

        if (myShipIcons[index] != null)
        {
            myShipIcons[index].color = sunkShipColor;
        }
    }

    private void MarkEnemyShipSunk(string shipId)
    {
        int index = GetShipIconIndex(shipId);

        if (index < 0 || enemyShipIcons == null || index >= enemyShipIcons.Length)
        {
            return;
        }

        if (enemyShipIcons[index] != null)
        {
            enemyShipIcons[index].color = sunkShipColor;
        }
    }

    #endregion

    #region Turn Timer Flow

    private void StartTurnTimer()
    {
        if (isNetworkDisconnected)
        {
            return;
        }

        if (isRestartingByReplay || isReturningToTitleByLeave)
        {
            return;
        }

        if (useLocalBattleTest)
        {
            return;
        }

        if (gameState != GameState.Battle)
        {
            return;
        }

        if (isGameOver)
        {
            return;
        }

        if (!isMyTurn)
        {
            return;
        }

        turnTimer = turnTimeLimit;
        isTurnTimerRunning = true;

        UpdateStatusText();
        UpdateTurnTimerUI();

        Debug.Log($"[TurnTimer] 턴 타이머 시작: {turnTimeLimit}초");
    }

    private void StopTurnTimer()
    {
        isTurnTimerRunning = false;
        UpdateTurnTimerUI();
    }

    private void UpdateTurnTimer()
    {
        if (isNetworkDisconnected)
        {
            StopTurnTimer();
            return;
        }

        if (isRestartingByReplay || isReturningToTitleByLeave)
        {
            StopTurnTimer();
            return;
        }

        if (!isTurnTimerRunning)
        {
            UpdateTurnTimerUI();
            return;
        }

        if (gameState != GameState.Battle)
        {
            StopTurnTimer();
            return;
        }

        if (isGameOver)
        {
            StopTurnTimer();
            return;
        }

        if (!isMyTurn)
        {
            StopTurnTimer();
            return;
        }

        if (isWaitingResult)
        {
            UpdateTurnTimerUI();
            return;
        }

        turnTimer -= Time.deltaTime;
        UpdateTurnTimerUI();

        if (turnTimer <= 0f)
        {
            OnTurnTimeout();
        }
    }

    private void UpdateTurnTimerUI()
    {
        if (turnTimerText == null)
        {
            return;
        }

        if (isNetworkDisconnected)
        {
            turnTimerText.text = "";
            return;
        }

        if (isRestartingByReplay)
        {
            turnTimerText.text = "";
            return;
        }

        if (isReturningToTitleByLeave)
        {
            turnTimerText.text = "";
            return;
        }

        if (gameState != GameState.Battle || isGameOver)
        {
            turnTimerText.text = "";
            return;
        }

        if (!isMyTurn)
        {
            turnTimerText.text = "";
            return;
        }

        if (isWaitingResult)
        {
            turnTimerText.text = "";
            return;
        }

        if (!isTurnTimerRunning)
        {
            turnTimerText.text = "";
            return;
        }

        int displayTime = Mathf.CeilToInt(turnTimer);
        turnTimerText.text = $"남은 시간: {displayTime}초";
    }

    private void OnTurnTimeout()
    {
        if (isNetworkDisconnected)
        {
            return;
        }

        if (isRestartingByReplay || isReturningToTitleByLeave)
        {
            return;
        }

        if (!isMyTurn)
        {
            return;
        }

        if (isWaitingResult)
        {
            return;
        }

        Debug.Log("[TurnTimer] 제한 시간 초과, 턴 넘김");

        isMyTurn = false;
        isWaitingResult = false;

        UpdateStatusText();
        StopTurnTimer();

        TrySendPacket(PacketProtocol.TURN_TIMEOUT);

        Debug.Log("[Turn] 시간 초과로 상대 턴 대기");
    }

    #endregion

    #region Network Send Flow

    private bool TrySendPacket(string packet)
    {
        if (useLocalBattleTest)
        {
            Debug.Log($"[Packet Send] 로컬 테스트 모드라 패킷 전송 생략: {packet}");
            return false;
        }

        if (isNetworkDisconnected)
        {
            Debug.LogWarning($"[Packet Send] 연결 끊김 상태라 패킷 전송 생략: {packet}");
            return false;
        }

        if (TCPManager.Instance == null)
        {
            Debug.LogWarning($"[Packet Send] TCPManager.Instance 없음: {packet}");
            return false;
        }

        if (!TCPManager.Instance.IsConnected)
        {
            Debug.LogWarning($"[Packet Send] TCP 연결 안 됨: {packet}");
            return false;
        }

        TCPManager.Instance.Send(packet);
        return true;
    }

    #endregion

    #region Utility

    private string ConvertAttackResultToPacketText(AttackResult result)
    {
        switch (result)
        {
            case AttackResult.Hit:
                return "HIT";

            case AttackResult.Miss:
                return "MISS";

            case AttackResult.Sunk:
                return "SUNK";

            case AttackResult.GameOver:
                return "GAME_OVER";

            default:
                return "INVALID";
        }
    }

    #endregion

    #region Debug

    [ContextMenu("Debug Start Battle")]
    private void DebugStartBattle()
    {
        isReplaySceneReloading = false;

        isMyReady = true;
        isOpponentReady = true;
        isPlacementLocked = true;
        gameState = GameState.Battle;

        isGameOver = false;
        isWaitingResult = false;
        isMyTurn = true;
        isMyReplayReady = false;
        isOpponentReplayReady = false;
        isNetworkDisconnected = false;
        isRestartingByReplay = false;
        isReturningToTitleByLeave = false;
        isClearingReplayReloadFlag = false;

        HideGameOverUI();
        HideDisconnectUI();
        ResetShipStatusUI();
        ShowBattleUI();

        Debug.Log("[Debug] 강제 Battle 단계 진입");

        StartTurnTimer();
    }

    #endregion
}