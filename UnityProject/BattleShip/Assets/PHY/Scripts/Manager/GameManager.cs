using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    #region Inspector Fields

    [Header("게임 상태")]
    [SerializeField] private GameState gameState = GameState.None;

    [Header("보드 연결")]
    [SerializeField] private BoardView myBoardView;

    [Header("UI 컨트롤러")]
    [SerializeField] private BattleUIController battleUIController;

    [Header("매치 컨트롤러")]
    [SerializeField] private MatchController matchController;

    [Header("Ready 컨트롤러")]
    [SerializeField] private ReadyController readyController;

    [Header("전투 컨트롤러")]
    [SerializeField] private BattleController battleController;

    [Header("네트워크 패킷 핸들러")]
    [SerializeField] private BattleNetworkHandler battleNetworkHandler;

    [Header("재시작 상태")]
    [SerializeField] private bool isMyReplayReady;
    [SerializeField] private bool isOpponentReplayReady;

    #endregion

    #region Properties

    public bool IsPlacementLocked => readyController != null && readyController.IsPlacementLocked;
    public bool IsBattle => gameState == GameState.Battle;
    public GameState CurrentState => gameState;

    private bool IsDisconnected => matchController != null && matchController.IsDisconnected;
    private bool IsRestarting => matchController != null && matchController.IsRestarting;
    private bool IsLeaving => matchController != null && matchController.IsLeaving;

    private bool IsMyTurn => battleController != null && battleController.IsMyTurn;
    private bool IsWaitingResult => battleController != null && battleController.IsWaitingResult;
    private bool IsGameOver => battleController != null && battleController.IsGameOver;

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

        if (battleController == null)
        {
            battleController = GetComponent<BattleController>();
        }

        if (battleNetworkHandler == null)
        {
            battleNetworkHandler = GetComponent<BattleNetworkHandler>();
        }
    }

    private void Start()
    {
        gameState = GameState.Placement;

        SetupControllers();

        ShowPlacementUI();
        HideGameOverUI();
        HideDisconnectUI();
        ResetShipStatusUI();
        UpdateStatusText();

        if (matchController != null)
        {
            matchController.ClearReplayAfterSceneLoad(gameState);
        }

        Debug.Log("[GameManager] Placement 단계 시작");
    }

    private void Update()
    {
        TrySendWaitingReady();

        if (matchController != null)
        {
            matchController.CheckReplayReconnect();
        }

        if (battleController != null)
        {
            battleController.UpdateBattle();
        }
    }

    private void SetupControllers()
    {
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

        if (battleController != null)
        {
            battleController.Setup(
                () => IsDisconnected,
                () => IsRestarting,
                () => IsLeaving,
                () => gameState,
                SetGameState,
                TrySendPacket,
                UpdateStatusText
            );

            battleController.ResetBattle();
        }

        if (battleNetworkHandler != null)
        {
            battleNetworkHandler.Setup(
                () => IsDisconnected,
                ReceiveOpponentReady,
                ReceiveAttackPacket,
                ReceiveResultPacket,
                ReceiveGameOverPacket,
                ReceiveTurnTimeoutPacket,
                ReceiveReplayReadyPacket,
                ReceiveReplayStartPacket,
                ReceiveLeavePacket
            );
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

        isMyReplayReady = false;
        isOpponentReplayReady = false;

        if (matchController != null)
        {
            matchController.ClearBattleLock();
        }

        bool startWithMyTurn = TCPManager.Instance != null && TCPManager.Instance.IsHost;

        ShowBattleUI();

        Debug.Log("[Battle] 양쪽 Ready 완료, 전투 단계 진입");

        if (battleController != null)
        {
            battleController.StartBattle(startWithMyTurn);
        }
    }

    #endregion

    #region Packet Receive Flow

    public void OnReceivePacket(string packet)
    {
        if (battleNetworkHandler == null)
        {
            Debug.LogError("[Packet] BattleNetworkHandler 연결 필요");
            return;
        }

        battleNetworkHandler.ReceivePacket(packet);
    }

    private void ReceiveAttackPacket(string[] split)
    {
        if (battleController == null)
        {
            Debug.LogError("[Battle] BattleController 연결 필요");
            return;
        }

        battleController.ReceiveAttackPacket(split);
    }

    private void ReceiveResultPacket(string[] split)
    {
        if (battleController == null)
        {
            Debug.LogError("[Battle] BattleController 연결 필요");
            return;
        }

        battleController.ReceiveResultPacket(split);
    }

    private void ReceiveGameOverPacket()
    {
        if (battleController == null)
        {
            Debug.LogError("[Battle] BattleController 연결 필요");
            return;
        }

        battleController.ReceiveGameOverPacket();
    }

    private void ReceiveTurnTimeoutPacket()
    {
        if (battleController == null)
        {
            Debug.LogError("[Battle] BattleController 연결 필요");
            return;
        }

        battleController.ReceiveTurnTimeoutPacket();
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

        StopBattle();

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

        StopBattle();

        isMyReplayReady = false;
        isOpponentReplayReady = false;

        if (readyController != null)
        {
            readyController.LockPlacement();
            readyController.ClearWaitingReady();
        }

        UpdateStatusText();

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

        StopBattle();

        isMyReplayReady = false;
        isOpponentReplayReady = false;

        if (readyController != null)
        {
            readyController.LockPlacement();
            readyController.ClearWaitingReady();
        }

        UpdateStatusText();

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
        if (battleController == null)
        {
            Debug.LogError("[Battle] BattleController 연결 필요");
            return;
        }

        battleController.TryAttackEnemyBoard(x, y);
    }

    #endregion

    #region Replay / Exit Flow

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

        StopBattle();

        if (readyController != null)
        {
            readyController.LockPlacement();
            readyController.ClearWaitingReady();
        }

        UpdateStatusText();
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

        StopBattle();

        if (matchController != null)
        {
            matchController.RestartGameScene();
        }
    }

    #endregion

    #region Battle State Flow

    private void StopBattle()
    {
        if (battleController != null)
        {
            battleController.StopBattle();
        }
    }

    private void SetGameState(GameState newState)
    {
        gameState = newState;
        UpdateStatusText();
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
            if (IsWaitingResult)
            {
                battleUIController.SetStatusText("공격 결과 대기 중");
                return;
            }

            battleUIController.SetStatusText(IsMyTurn ? "내 차례" : "상대 차례");
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
}