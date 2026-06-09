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

    [Header("Replay 컨트롤러")]
    [SerializeField] private ReplayController replayController;

    [Header("나가기 컨트롤러")]
    [SerializeField] private LeaveController leaveController;

    [Header("Ready 컨트롤러")]
    [SerializeField] private ReadyController readyController;

    [Header("전투 컨트롤러")]
    [SerializeField] private BattleController battleController;

    [Header("네트워크 패킷 핸들러")]
    [SerializeField] private BattleNetworkHandler battleNetworkHandler;

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

        FindControllers();
    }

    private void Start()
    {
        gameState = GameState.Placement;

        SetupControllers();

        battleUIController?.ShowPlacementUI();
        battleUIController?.HideGameOverUI();
        battleUIController?.HideDisconnectPanel();
        battleUIController?.ResetShipStatus();

        UpdateStatusText();

        matchController?.ClearReplayAfterSceneLoad(gameState);

        Debug.Log("[GameManager] Placement 단계 시작");
    }

    private void Update()
    {
        TrySendWaitingReady();

        matchController?.CheckReplayReconnect();
        battleController?.UpdateBattle();
    }

    private void FindControllers()
    {
        if (battleUIController == null)
        {
            battleUIController = GetComponent<BattleUIController>();
        }

        if (matchController == null)
        {
            matchController = GetComponent<MatchController>();
        }

        if (replayController == null)
        {
            replayController = GetComponent<ReplayController>();
        }

        if (leaveController == null)
        {
            leaveController = GetComponent<LeaveController>();
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

    private void SetupControllers()
    {
        if (matchController != null)
        {
            matchController.Setup(UpdateStatusText);
            matchController.ResetState();
        }

        if (replayController != null)
        {
            replayController.Setup(
                matchController,
                () => gameState,
                SendPacket,
                StopBattle
            );

            replayController.ResetState();
        }

        if (leaveController != null)
        {
            leaveController.Setup(
                matchController,
                SendPacket,
                StopBattle,
                LockPlacement,
                UpdateStatusText,
                HideGameOverUI,
                ShowDisconnectUI
            );
        }

        if (readyController != null)
        {
            readyController.Setup(
                IsAllShipsPlaced,
                IsTcpConnected,
                SendPacket,
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
                SendPacket,
                UpdateStatusText
            );

            battleController.ResetBattle();
        }

        if (battleNetworkHandler != null)
        {
            battleNetworkHandler.Setup(
                () => IsDisconnected,
                () => readyController?.ReceiveOpponentReady(),
                split => battleController?.ReceiveAttackPacket(split),
                split => battleController?.ReceiveResultPacket(split),
                () => battleController?.ReceiveGameOverPacket(),
                () => battleController?.ReceiveTurnTimeoutPacket(),
                () => replayController?.ReceiveReplayReady(),
                () => replayController?.ReceiveReplayStart(),
                () => leaveController?.ReceiveLeave()
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
        if (IsDisconnected || IsRestarting || IsLeaving)
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

        matchController?.ClearBattleLock();
        replayController?.ResetState();

        bool startWithMyTurn = TCPManager.Instance != null && TCPManager.Instance.IsHost;

        battleUIController?.ShowBattleUI();

        Debug.Log("[Battle] 양쪽 Ready 완료, 전투 단계 진입");

        battleController?.StartBattle(startWithMyTurn);
    }

    #endregion

    #region Packet Flow

    public void OnReceivePacket(string packet)
    {
        if (battleNetworkHandler == null)
        {
            Debug.LogError("[Packet] BattleNetworkHandler 연결 필요");
            return;
        }

        battleNetworkHandler.ReceivePacket(packet);
    }

    private bool SendPacket(string packet)
    {
        if (battleNetworkHandler == null)
        {
            Debug.LogError("[Packet] BattleNetworkHandler 연결 필요");
            return false;
        }

        return battleNetworkHandler.SendPacket(packet);
    }

    #endregion

    #region Button Flow

    public void TryAttackEnemyBoard(int x, int y)
    {
        battleController?.TryAttackEnemyBoard(x, y);
    }

    public void OnClickReplayButton()
    {
        replayController?.ClickReplay();
    }

    public void OnClickExitButton()
    {
        leaveController?.ClickExit();
    }

    public void OnNetworkDisconnected()
    {
        leaveController?.Disconnect();
    }

    #endregion

    #region State Flow

    private void SetGameState(GameState newState)
    {
        gameState = newState;
        UpdateStatusText();
    }

    private void StopBattle()
    {
        battleController?.StopBattle();
    }

    private void LockPlacement()
    {
        readyController?.LockPlacement();
        readyController?.ClearWaitingReady();
    }

    #endregion

    #region UI Flow

    private void UpdateStatusText()
    {
        if (battleUIController == null)
        {
            return;
        }

        battleUIController.UpdateGameStatus(
            gameState,
            IsDisconnected,
            IsLeaving,
            IsRestarting,
            IsWaitingResult,
            IsMyTurn
        );
    }

    private void HideGameOverUI()
    {
        battleUIController?.HideGameOverUI();
    }

    private void ShowDisconnectUI()
    {
        battleUIController?.ShowDisconnectPanel();
        UpdateStatusText();
    }

    #endregion
}