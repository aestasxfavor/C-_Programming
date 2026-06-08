using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

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

        ShowPlacementUI();
        HideGameOverUI();
        HideDisconnectUI();
        UpdateTurnTimerUI();

        Debug.Log("[GameManager] Placement 단계 시작");
    }

    private void Update()
    {
        UpdateTurnTimer();
        TrySendPendingReady();
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

        Debug.Log("[Ready] 상대 Ready 수신");

        TryStartBattle();
    }

    private void TrySendPendingReady()
    {
        if (isNetworkDisconnected)
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
            StopTurnTimer();
        }
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

        string aroundPositionsText = "";

        if (result == AttackResult.Sunk || result == AttackResult.GameOver)
        {
            aroundPositionsText = myBoardView.GetLastSunkAroundPositionsText();
        }

        string resultPacket = $"{PacketProtocol.RESULT}|{x}|{y}|{resultText}";

        if (!string.IsNullOrEmpty(aroundPositionsText))
        {
            resultPacket += $"|{aroundPositionsText}";
        }

        TrySendPacket(resultPacket);

        Debug.Log($"[Result] 결과 패킷 전송 {resultText}, X={x}, Y={y}");

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

            StartTurnTimer();
        }
        else if (result == AttackResult.Hit || result == AttackResult.Sunk)
        {
            isMyTurn = false;

            Debug.Log("[Turn] 상대 공격이 명중함, 상대 턴 유지");

            StopTurnTimer();
        }
    }

    private void OnReceiveResultPacket(string[] split)
    {
        if (split.Length < 4)
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
        string aroundPositionsText = split.Length >= 5 ? split[4] : "";

        Debug.Log($"[Result] 결과 수신 {resultText}, X={x}, Y={y}");

        isWaitingResult = false;

        if (resultText == "INVALID")
        {
            isMyTurn = true;

            Debug.Log("[Result] INVALID 수신, 내 턴 유지");

            StartTurnTimer();
            return;
        }

        if (enemyBoardView == null)
        {
            Debug.LogError("[Result] enemyBoardView 연결 필요");
            return;
        }

        enemyBoardView.ApplyAttackResult(x, y, resultText, aroundPositionsText);

        if (resultText == "GAME_OVER")
        {
            SetGameOver(true);
            return;
        }

        if (resultText == "MISS")
        {
            isMyTurn = false;

            Debug.Log("[Turn] 공격 실패, 상대 턴 대기");

            StopTurnTimer();
        }
        else if (resultText == "HIT" || resultText == "SUNK")
        {
            isMyTurn = true;

            Debug.Log("[Turn] 공격 성공, 내 턴 유지");

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

        isMyTurn = true;
        isWaitingResult = false;

        Debug.Log("[TurnTimer] 상대 시간 초과, 내 턴 시작");

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

    private void OnReceiveLeavePacket()
    {
        Debug.Log("[Network] 상대가 게임에서 나감, 타이틀로 이동");

        StopTurnTimer();

        SceneManager.LoadScene(titleSceneName);
    }

    #endregion

    #region Network Disconnect Flow

    public void OnNetworkDisconnected()
    {
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

    private void ShowDisconnectUI()
    {
        if (disconnectPanel != null)
        {
            disconnectPanel.SetActive(true);
        }
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

        isGameOver = true;
        isMyTurn = false;
        isWaitingResult = false;

        StopTurnTimer();

        gameState = GameState.GameOver;

        ShowGameOverUI(isWin);

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

        Debug.Log("[UI] Exit 버튼 클릭");

        TrySendPacket(PacketProtocol.LEAVE);

        StopTurnTimer();

        SceneManager.LoadScene(titleSceneName);
    }

    private void TryRestartBattleScene()
    {
        if (isNetworkDisconnected)
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

        Debug.Log("[Replay] 양쪽 재시작 Ready 완료, BattleScene 재시작");

        StopTurnTimer();

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
    }

    #endregion

    #region Turn Timer Flow

    private void StartTurnTimer()
    {
        if (isNetworkDisconnected)
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

        if (gameState != GameState.Battle || isGameOver)
        {
            turnTimerText.text = "";
            return;
        }

        if (!isMyTurn)
        {
            turnTimerText.text = "상대 턴 대기 중";
            return;
        }

        int displayTime = Mathf.CeilToInt(turnTimer);
        turnTimerText.text = $"남은 시간: {displayTime}";
    }

    private void OnTurnTimeout()
    {
        if (isNetworkDisconnected)
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

        HideGameOverUI();
        HideDisconnectUI();
        ShowBattleUI();

        Debug.Log("[Debug] 강제 Battle 단계 진입");

        StartTurnTimer();
    }

    #endregion
}