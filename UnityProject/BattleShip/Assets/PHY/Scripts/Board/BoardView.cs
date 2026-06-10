using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum BoardRole
{
    MyBoard,
    EnemyBoard
}

public class BoardView : MonoBehaviour
{
    [SerializeField] private BoardCell cellTemplate;

    private const int BoardSize = 11;

    private BoardCell[,] cells = new BoardCell[BoardSize, BoardSize];
    private CellState[,] boardStates = new CellState[BoardSize, BoardSize];
    private int[,] shipIDByCell = new int[BoardSize, BoardSize];

    [Header("보드 역할")]
    [SerializeField] private BoardRole boardRole = BoardRole.MyBoard;

    [Header("타일 스프라이트")]
    [SerializeField] private Sprite waterSprite;
    [SerializeField] private Sprite landSprite;
    [SerializeField] private Sprite shipSprite;
    [SerializeField] private Sprite blockedSprite;
    [SerializeField] private Sprite hitSprite;
    [SerializeField] private Sprite missSprite;

    [Header("프리뷰 스프라이트")]
    [SerializeField] private Sprite previewShipSprite;
    [SerializeField] private Sprite invalidPreviewSprite;
    [SerializeField] private Sprite spacingPreviewSprite;

    [Header("배 이미지 오버레이")]
    [SerializeField] private bool useShipVisualOverlay;
    [SerializeField] private RectTransform shipVisualRoot;
    [SerializeField] private Image shipVisualTemplate;
    [SerializeField] private Sprite shipSize2VisualSprite;
    [SerializeField] private Sprite shipSize3VisualSprite;
    [SerializeField] private Sprite shipSize4VisualSprite;
    [SerializeField] private Sprite shipSize5VisualSprite;
    [SerializeField] private Vector2 shipVisualPadding = Vector2.zero;
    [SerializeField] private bool hideCellShipSpriteWhenUsingOverlay = true;

    [Header("전투 표시 옵션")]
    [SerializeField] private bool hideBlockedCellsOnBattle = true;

    private BoardSetupController setupController;
    private ShipVisualController shipVisualController;
    private BoardRenderer boardRenderer;
    private BoardPlacementController placementController;
    private BattleAttackController attackController;
    private BoardInputController inputController;

    [Header("함선 세팅")]
    private ShipData[] ships;

    [SerializeField] private Button readyButton;

    [Header("배 선택 UI")]
    [SerializeField] private ShipDragItem[] shipDragItems;

    [SerializeField] private bool isShipSpacingRuleEnabled = true;

    private bool lastBattleState;

    private void Start()
    {
        Debug.Log("[BoardView] Start 실행");

        InitShips();

        InitBoardSetup();
        setupController.SetupBoard();

        InitShipVisualOverlay();
        InitBoardRenderer();
        InitPlacementController();
        InitAttackController();
        InitInputController();

        RefreshCells();

        UpdateReadyButton();
    }

    private void Update()
    {
        UpdateBattleVisualState();
    }

    private void UpdateBattleVisualState()
    {
        bool currentBattleState = IsBattle();

        if (currentBattleState == lastBattleState)
        {
            return;
        }

        lastBattleState = currentBattleState;

        RefreshCells();
    }

    private void InitShips()
    {
        ships = new ShipData[]
        {
            new ShipData(0, 2),
            new ShipData(1, 3),
            new ShipData(2, 3),
            new ShipData(3, 4),
            new ShipData(4, 5),
        };
    }

    #region 보드 준비

    private void InitBoardSetup()
    {
        setupController = new BoardSetupController(
            BoardSize,
            cellTemplate,
            transform,
            cells,
            boardStates,
            shipIDByCell,
            OnClickCell,
            OnRightClickCell,
            OnPointerEnterCell,
            OnPointerExitCell,
            OnDropCell
        );
    }

    #endregion

    #region 입력 처리

    private void InitInputController()
    {
        inputController = new BoardInputController(
            boardRole,
            boardStates,
            placementController,
            IsBattle,
            IsPlacementLocked,
            RequestEnemyAttack,
            ShowShipPreview,
            ClearShipPreview
        );
    }

    private void OnClickCell(BoardCell cell)
    {
        if (inputController == null)
        {
            return;
        }

        inputController.OnClickCell(cell);
    }

    private void OnRightClickCell(BoardCell cell)
    {
        if (inputController == null)
        {
            return;
        }

        inputController.OnRightClickCell(cell);
    }

    private void OnPointerEnterCell(BoardCell cell)
    {
        if (inputController == null)
        {
            return;
        }

        inputController.OnPointerEnterCell(cell);
    }

    private void OnPointerExitCell(BoardCell cell)
    {
        if (inputController == null)
        {
            return;
        }

        inputController.OnPointerExitCell(cell);
    }

    private void OnDropCell(BoardCell cell, PointerEventData eventData)
    {
        if (inputController == null)
        {
            return;
        }

        inputController.OnDropCell(cell, eventData);
    }

    private void RequestEnemyAttack(int x, int y)
    {
        if (GameManager.Instance == null)
        {
            return;
        }

        GameManager.Instance.TryAttackEnemyBoard(x, y);
    }

    #endregion

    #region 함선 선택 / 회전

    public void SelectShip(int shipID)
    {
        if (placementController == null)
        {
            return;
        }

        placementController.SelectShip(shipID);
    }

    public void RotateShip()
    {
        if (placementController == null)
        {
            return;
        }

        placementController.RotateShip();
    }

    #endregion

    #region 셀 표시

    private void InitBoardRenderer()
    {
        boardRenderer = new BoardRenderer(
            BoardSize,
            boardRole,
            cells,
            waterSprite,
            landSprite,
            shipSprite,
            blockedSprite,
            hitSprite,
            missSprite,
            previewShipSprite,
            invalidPreviewSprite,
            hideBlockedCellsOnBattle,
            hideCellShipSpriteWhenUsingOverlay,
            IsBattle,
            IsShipVisualOverlayEnabled
        );
    }

    private void RefreshCells()
    {
        if (boardRenderer == null)
        {
            return;
        }

        boardRenderer.RefreshAllCells(boardStates);
    }

    private void RefreshCell(int x, int y)
    {
        if (boardRenderer == null)
        {
            return;
        }

        boardRenderer.RefreshCell(x, y, boardStates);
    }

    private bool IsBattle()
    {
        return GameManager.Instance != null && GameManager.Instance.IsBattle;
    }

    #endregion

    #region 배 이미지 오버레이

    private void InitShipVisualOverlay()
    {
        shipVisualController = new ShipVisualController(
            useShipVisualOverlay,
            boardRole,
            BoardSize,
            cells,
            shipVisualRoot,
            shipVisualTemplate,
            shipSize2VisualSprite,
            shipSize3VisualSprite,
            shipSize4VisualSprite,
            shipSize5VisualSprite,
            shipVisualPadding
        );

        shipVisualController.InitVisualRoot();
    }

    private bool IsShipVisualOverlayEnabled()
    {
        return shipVisualController != null && shipVisualController.CanShowShipVisual;
    }

    private void CreateShipVisual(ShipData ship)
    {
        if (shipVisualController == null)
        {
            return;
        }

        shipVisualController.ShowShip(ship);
    }

    private void RemoveShipVisual(int shipID)
    {
        if (shipVisualController == null)
        {
            return;
        }

        shipVisualController.RemoveShip(shipID);
    }

    private void ClearAllShipVisuals()
    {
        if (shipVisualController == null)
        {
            return;
        }

        shipVisualController.ClearAllShips();
    }

    #endregion

    #region 배치 컨트롤러

    private void InitPlacementController()
    {
        placementController = new BoardPlacementController(
            BoardSize,
            boardRole,
            boardStates,
            shipIDByCell,
            ships,
            IsPlacementLocked,
            RefreshCells,
            UpdateReadyButton,
            ResetShipDragItems,
            ClearShipPreview,
            RemoveShipVisual,
            CreateShipVisual,
            ClearAllShipVisuals
        );
    }

    #endregion

    #region 공격 컨트롤러

    private void InitAttackController()
    {
        attackController = new BattleAttackController(
            BoardSize,
            boardStates,
            shipIDByCell,
            ships,
            RefreshCell
        );
    }

    public AttackResult ReceiveAttack(int x, int y)
    {
        if (attackController == null)
        {
            return AttackResult.Invalid;
        }

        return attackController.ReceiveAttack(x, y);
    }

    public void ApplyAttackResult(int x, int y, string resultText, string aroundPositionsText)
    {
        if (attackController == null)
        {
            return;
        }

        attackController.ApplyAttackResult(x, y, resultText, aroundPositionsText);
    }

    public string GetLastSunkAroundPositionsText()
    {
        if (attackController == null)
        {
            return "";
        }

        return attackController.GetLastSunkAroundPositionsText();
    }

    public string GetLastSunkShipId()
    {
        if (attackController == null)
        {
            return "";
        }

        return attackController.GetLastSunkShipId();
    }

    public bool CanRequestAttack(int x, int y)
    {
        if (attackController == null)
        {
            return false;
        }

        return attackController.CanRequestAttack(x, y);
    }

    #endregion

    #region 배치 처리

    public bool TryPlaceSelectedShipAt(BoardCell cell)
    {
        if (inputController == null)
        {
            return false;
        }

        return inputController.TryPlaceSelectedShipAt(cell);
    }

    #endregion

    #region 함선 배치 리셋

    public void OnClickResetPlacementButton()
    {
        if (boardRole != BoardRole.MyBoard)
        {
            return;
        }

        if (IsPlacementLocked())
        {
            Debug.Log("[Placement] Ready 이후 배치 리셋 불가");
            return;
        }

        ResetPlacement();
    }

    private void ResetPlacement()
    {
        if (attackController != null)
        {
            attackController.ClearLastSunkResult();
        }

        if (placementController == null)
        {
            return;
        }

        placementController.ResetPlacement();
    }

    private void ResetShipDragItems()
    {
        if (shipDragItems == null)
        {
            return;
        }

        for (int i = 0; i < shipDragItems.Length; i++)
        {
            if (shipDragItems[i] == null)
            {
                continue;
            }

            shipDragItems[i].ResetDragItem();
        }
    }

    #endregion

    #region 프리뷰

    private void ShowShipPreview(List<Vector2Int> positions, bool canPlace)
    {
        if (boardRenderer == null)
        {
            return;
        }

        boardRenderer.ShowShipPreview(positions, canPlace);
    }

    private void ClearShipPreview()
    {
        if (boardRenderer == null)
        {
            return;
        }

        boardRenderer.ClearShipPreview(boardStates);
    }

    #endregion

    #region 상태 확인 / 유틸

    public bool IsAllShipsPlaced()
    {
        if (placementController == null)
        {
            return false;
        }

        return placementController.IsAllShipsPlaced();
    }

    private void UpdateReadyButton()
    {
        if (readyButton == null)
        {
            return;
        }

        readyButton.interactable = IsAllShipsPlaced();
    }

    private bool IsPlacementLocked()
    {
        return GameManager.Instance != null && GameManager.Instance.IsPlacementLocked;
    }

    #endregion

    #region 상대 보드 좌표 반전 예정 구역

    // 기본 전투 시스템 안정화 이후 작업
    // TCP 패킷에는 원본 좌표 사용
    // 화면 표시만 반전 좌표 사용

    //private Vector2Int ConvertToDisplayPosition(Vector2Int originalPosition)
    //{
    //    return originalPosition;
    //}

    //private Vector2Int ConvertToOriginalPosition(Vector2Int displayPosition)
    //{
    //    return originalPosition;
    //}

    #endregion
}