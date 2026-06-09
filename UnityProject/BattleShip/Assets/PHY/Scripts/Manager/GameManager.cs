using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    #region Inspector Fields

    [Header("게임 상태")]
    [SerializeField] private GameState gameState = GameState.None;

    [Header("보드 연결")]
    [SerializeField] private BoardView myBoardView;
    [SerializeField] private BoardView enemyBoardView;

    [Header("UI 컨트롤러")]
    [SerializeField] private BattleUIController battleUIController;

    [Header("매치 컨트롤러")]
    [SerializeField] private MatchController matchController;

    [Header("Ready 컨트롤러")]
    [SerializeField] private ReadyController readyController;

    [Header("전투 턴 상태")]
    [SerializeField] private bool isMyTurn;
    [SerializeField] private bool isWaitingResult;
    [SerializeField] private bool isGameOver;

    [Header("재시작 상태")]
    [SerializeField] private bool isMyReplayReady;
    [SerializeField] private bool isOpponentReplayReady;

    [Header("턴 시간 제한")]
    [SerializeField] private float turnTimeLimit = 15f;
    [SerializeField] private float turnTimer;
    [SerializeField] private bool isTurnTimerRunning;

    #endregion

    #region Properties

    public bool IsPlacementLocked => readyController != null && readyController.IsPlacementLocked;
    public bool IsBattle => gameState == GameState.Battle;
    public GameState CurrentState => gameState;

    private bool IsDisconnected => matchController != null && matchController.IsDisconnected;
    private bool IsRestarting => matchController != null && matchController.IsRestarting;
    private bool IsLeaving => matchController != null && matchController.IsLeaving;

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

        if (battleUIController == null)
        {
            battleUIController = GetComponent<BattleUIController>();
        }

        if (matchController == null)
        {
            matchController = GetComponent<MatchController>();
        }

        if (readyController == null)
        {
            readyController = GetComponent<ReadyController>();
        }
    }

    private void Start()
    {
        gameState = GameState.Placement;

        if (matchController != null)
        {
            matchController.Setup(UpdateStatusText);
            matchController.ResetState();
        }

        if (readyController != null)
        {
            readyController.Setup(
                IsAllShipsPlaced,
                IsTcpConnected,
                TrySendPacket,
                SetWaitingReadyState,
                TryStartBattle,
                UpdateStatusText
            );

            readyController.ResetReadyState();
        }

        ShowPlacementUI();
        HideGameOverUI();
        HideDisconnectUI();
        ResetShipStatusUI();
        UpdateTurnTimerUI();
        UpdateStatusText();

        if (matchController != null)
        {
            matchController.ClearReplayAfterSceneLoad(gameState);
        }

        Debug.Log("[GameManager] Placement 단계 시작");
    }

    private void Update()
    {
        UpdateTurnTimer();
        TrySendWaitingReady();

        if (matchController != null)
        {
            matchController.CheckReplayReconnect();
        }
    }

    #endregion

    #region Ready Flow

    public void OnClickReadyButton()
    {
        if (IsDisconnected)
        {
            Debug.Log("[Network] 연결 끊김 상태라 Ready 불가");
            return;
        }

        if (IsRestarting || IsLeaving)
        {
            Debug.Log("[Ready] 씬 전환 중이라 Ready 불가");
            return;
        }

        if (readyController == null)
        {
            Debug.LogError("[Ready] ReadyController 연결 필요");
            return;
        }

        readyController.ClickReady();
    }

    private void ReceiveOpponentReady()
    {
        if (readyController == null)
        {
            return;
        }

        readyController.ReceiveOpponentReady();
    }

    private void TrySendWaitingReady()
    {
        if (readyController == null)
        {
            return;
        }

        bool canSendReady =
            !IsDisconnected &&
            !IsRestarting &&
            !IsLeaving;

        readyController.TrySendWaitingReady(canSendReady);
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

    private bool IsTcpConnected()
    {
        return TCPManager.Instance != null && TCPManager.Instance.IsConnected;
    }

    private void SetWaitingReadyState()
    {
        gameState = GameState.WaitingReady;
    }

    private void TryStartBattle()
    {
        if (IsDisconnected)
        {
            return;
        }

        if (IsRestarting || IsLeaving)
        {
            return;
        }

        if (readyController == null)
        {
            return;
        }

        if (!readyController.IsMyReady)
        {
            Debug.Log("[Ready] 내 Ready 대기 중");
            return;
        }

        if (!readyController.IsOpponentReady)
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

        if (matchController != null)
        {
            matchController.ClearBattleLock();
        }

        isMyTurn = TCPManager.Instance != null && TCPManager.Instance.IsHost;

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

    #region Packet Receive Flow

    public void OnReceivePacket(string packet)
    {
        if (string.IsNullOrEmpty(packet))
        {
            return;
        }

        if (IsDisconnected)
        {
            Debug.Log($"[Packet] 연결 끊김 상태라 패킷 무시: {packet}");
            return;
        }

        Debug.Log($"[Packet Received] {packet}");

        string[] split = packet.Split('|');

        switch (split[0])
        {
            case PacketProtocol.READY:
                ReceiveOpponentReady();
                break;

            case PacketProtocol.ATTACK:
                ReceiveAttackPacket(split);
                break;

            case PacketProtocol.RESULT:
                ReceiveResultPacket(split);
                break;

            case PacketProtocol.GAME_OVER:
                ReceiveGameOverPacket();
                break;

            case PacketProtocol.TURN_TIMEOUT:
                ReceiveTurnTimeoutPacket();
                break;

            case PacketProtocol.REPLAY_READY:
                ReceiveReplayReadyPacket();
                break;

            case PacketProtocol.REPLAY_START:
                ReceiveReplayStartPacket();
                break;

            case PacketProtocol.LEAVE:
                ReceiveLeavePacket();
                break;

            default:
                Debug.LogWarning($"[Packet] 알 수 없는 패킷: {packet}");
                break;
        }
    }

    private void ReceiveAttackPacket(string[] split)
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

        if (IsRestarting || IsLeaving)
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

    private void ReceiveResultPacket(string[] split)
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

    private void ReceiveGameOverPacket()
    {
        if (isGameOver)
        {
            return;
        }

        SetGameOver(true);
    }

    private void ReceiveTurnTimeoutPacket()
    {
        if (gameState != GameState.Battle)
        {
            return;
        }

        if (isGameOver)
        {
            return;
        }

        if (IsRestarting || IsLeaving)
        {
            return;
        }

        isMyTurn = true;
        isWaitingResult = false;

        Debug.Log("[TurnTimer] 상대 시간 초과, 내 턴 시작");

        UpdateStatusText();
        StartTurnTimer();
    }

    private void ReceiveReplayReadyPacket()
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

        TryRestartGame();
    }

    private void ReceiveReplayStartPacket()
    {
        if (IsRestarting)
        {
            return;
        }

        Debug.Log("[Replay] 재시작 시작 패킷 수신");

        if (matchController != null)
        {
            matchController.StartReplay();
        }

        StopTurnTimer();

        if (matchController != null)
        {
            matchController.RestartGameScene();
        }
    }

    private void ReceiveLeavePacket()
    {
        if (matchController == null)
        {
            return;
        }

        if (!matchController.ReceiveLeave())
        {
            return;
        }

        isMyTurn = false;
        isWaitingResult = false;
        isGameOver = true;
        isMyReplayReady = false;
        isOpponentReplayReady = false;

        if (readyController != null)
        {
            readyController.LockPlacement();
            readyController.ClearWaitingReady();
        }

        UpdateStatusText();
        StopTurnTimer();

        HideGameOverUI();
        ShowDisconnectUI();

        matchController.GoTitleAfterLeave();
    }

    #endregion

    #region Network Disconnect Flow

    public void OnNetworkDisconnected()
    {
        if (matchController == null)
        {
            return;
        }

        if (!matchController.Disconnect(false))
        {
            return;
        }

        isMyTurn = false;
        isWaitingResult = false;
        isGameOver = true;
        isMyReplayReady = false;
        isOpponentReplayReady = false;

        if (readyController != null)
        {
            readyController.LockPlacement();
            readyController.ClearWaitingReady();
        }

        UpdateStatusText();
        StopTurnTimer();

        HideGameOverUI();
        ShowDisconnectUI();

        matchController.GoTitleAfterDisconnect();
    }

    private void ShowDisconnectUI()
    {
        if (battleUIController != null)
        {
            battleUIController.ShowDisconnectPanel();
        }

        UpdateStatusText();
    }

    private void HideDisconnectUI()
    {
        if (battleUIController != null)
        {
            battleUIController.HideDisconnectPanel();
        }
    }

    #endregion

    #region Attack Flow

    public void TryAttackEnemyBoard(int x, int y)
    {
        if (IsDisconnected)
        {
            Debug.Log("[Network] 연결 끊김 상태라 공격 불가");
            return;
        }

        if (IsRestarting || IsLeaving)
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
        if (IsDisconnected)
        {
            return;
        }

        if (IsRestarting || IsLeaving)
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
        if (IsDisconnected)
        {
            Debug.Log("[Network] 연결 끊김 상태라 Replay 불가");
            return;
        }

        if (IsLeaving)
        {
            Debug.Log("[Replay] 타이틀 복귀 중이라 Replay 불가");
            return;
        }

        if (IsRestarting)
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

        TryRestartGame();
    }

    public void OnClickExitButton()
    {
        if (matchController == null)
        {
            return;
        }

        if (!matchController.TryLeave())
        {
            return;
        }

        isMyTurn = false;
        isWaitingResult = false;
        isGameOver = true;

        if (readyController != null)
        {
            readyController.LockPlacement();
            readyController.ClearWaitingReady();
        }

        UpdateStatusText();
        StopTurnTimer();
        HideGameOverUI();

        if (TCPManager.Instance != null && TCPManager.Instance.IsConnected)
        {
            TrySendPacket(PacketProtocol.LEAVE);
            matchController.GoTitleAfterSendLeave();
            return;
        }

        matchController.GoTitleNow();
    }

    private void TryRestartGame()
    {
        if (IsDisconnected)
        {
            return;
        }

        if (IsLeaving)
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

        if (IsRestarting)
        {
            return;
        }

        Debug.Log("[Replay] 양쪽 재시작 Ready 완료, BattleScene 재시작 준비");

        if (matchController != null)
        {
            matchController.StartReplay();
        }

        TrySendPacket(PacketProtocol.REPLAY_START);

        StopTurnTimer();

        if (matchController != null)
        {
            matchController.RestartGameScene();
        }
    }

    #endregion

    #region UI Flow

    private void ShowPlacementUI()
    {
        if (battleUIController != null)
        {
            battleUIController.ShowPlacementUI();
        }

        UpdateStatusText();
    }

    private void ShowBattleUI()
    {
        if (battleUIController != null)
        {
            battleUIController.ShowBattleUI();
        }

        Debug.Log("[UI] 함선 상태바 표시");

        UpdateStatusText();
    }

    private void HideGameOverUI()
    {
        if (battleUIController != null)
        {
            battleUIController.HideGameOverUI();
        }
    }

    private void ShowGameOverUI(bool isWin)
    {
        if (battleUIController != null)
        {
            battleUIController.ShowGameOverUI(isWin);
        }

        UpdateStatusText();
    }

    private void UpdateStatusText()
    {
        if (battleUIController == null)
        {
            return;
        }

        if (IsDisconnected)
        {
            battleUIController.SetStatusText("연결 끊김");
            return;
        }

        if (IsLeaving)
        {
            battleUIController.SetStatusText("매치 종료 중");
            return;
        }

        if (IsRestarting)
        {
            battleUIController.SetStatusText("다시 시작 준비 중");
            return;
        }

        if (gameState == GameState.Placement)
        {
            battleUIController.SetStatusText("함선 배치 중");
            return;
        }

        if (gameState == GameState.WaitingReady)
        {
            battleUIController.SetStatusText("상대 준비 대기 중");
            return;
        }

        if (gameState == GameState.GameOver)
        {
            battleUIController.SetStatusText("게임 종료");
            return;
        }

        if (gameState == GameState.Battle)
        {
            if (isWaitingResult)
            {
                battleUIController.SetStatusText("공격 결과 대기 중");
                return;
            }

            battleUIController.SetStatusText(isMyTurn ? "내 차례" : "상대 차례");
            return;
        }

        battleUIController.SetStatusText("");
    }

    private void ResetShipStatusUI()
    {
        if (battleUIController != null)
        {
            battleUIController.ResetShipStatus();
        }
    }

    private void MarkMyShipSunk(string shipId)
    {
        if (battleUIController != null)
        {
            battleUIController.MarkMyShipSunk(shipId);
        }
    }

    private void MarkEnemyShipSunk(string shipId)
    {
        if (battleUIController != null)
        {
            battleUIController.MarkEnemyShipSunk(shipId);
        }
    }

    #endregion

    #region Turn Timer Flow

    private void StartTurnTimer()
    {
        if (IsDisconnected)
        {
            return;
        }

        if (IsRestarting || IsLeaving)
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
        if (IsDisconnected)
        {
            StopTurnTimer();
            return;
        }

        if (IsRestarting || IsLeaving)
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
        if (battleUIController == null)
        {
            return;
        }

        if (IsDisconnected)
        {
            battleUIController.ClearTurnTimeText();
            return;
        }

        if (IsRestarting)
        {
            battleUIController.ClearTurnTimeText();
            return;
        }

        if (IsLeaving)
        {
            battleUIController.ClearTurnTimeText();
            return;
        }

        if (gameState != GameState.Battle || isGameOver)
        {
            battleUIController.ClearTurnTimeText();
            return;
        }

        if (!isMyTurn)
        {
            battleUIController.ClearTurnTimeText();
            return;
        }

        if (isWaitingResult)
        {
            battleUIController.ClearTurnTimeText();
            return;
        }

        if (!isTurnTimerRunning)
        {
            battleUIController.ClearTurnTimeText();
            return;
        }

        int displayTime = Mathf.CeilToInt(turnTimer);
        battleUIController.SetTurnTimeText(displayTime);
    }

    private void OnTurnTimeout()
    {
        if (IsDisconnected)
        {
            return;
        }

        if (IsRestarting || IsLeaving)
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
        if (IsDisconnected)
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
}