using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// 플레이어 보드와 상대 보드의 생성, 입력 연결, 표시 갱신, 배치 / 공격 컨트롤러 연결을 담당하는 보드 UI 컨트롤러
public class BoardView : MonoBehaviour
{
    #region 인스펙터 필드

    [SerializeField] private BoardCell cellTemplate;

    // TODO: 보드 크기는 추후 BattleShipRuleConfigSO로 분리 가능
    private const int BoardSize = 11;

    [Header("보드 역할")]
    [SerializeField] private BoardRole boardRole = BoardRole.MyBoard;

    // TODO: Water, Land, Hit, Miss, Sunk 표시 Sprite는 추후 BoardVisualConfigSO로 분리 가능
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

    // TODO: 함선 크기별 표시 Sprite는 추후 ShipDefinitionSO 또는 BoardVisualConfigSO로 분리 가능
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

    [Header("함선 세팅")]
    private ShipData[] ships;

    [SerializeField] private Button readyButton;

    [Header("배 선택 UI")]
    [SerializeField] private ShipDragItem[] shipDragItems;

    #endregion

    #region 런타임 상태 / 컨트롤러

    private BoardCell[,] cells = new BoardCell[BoardSize, BoardSize];
    private CellState[,] boardStates = new CellState[BoardSize, BoardSize];
    private int[,] shipIdByCell = new int[BoardSize, BoardSize];

    private BoardSetupController setupController;
    private ShipVisualController shipVisualController;
    private BoardRenderer boardRenderer;
    private BoardPlacementController placementController;
    private BattleAttackController attackController;
    private BoardInputController inputController;

    private bool lastBattleState;

    #endregion

    #region Unity 생명주기

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

    #endregion

    #region 초기화

    // 함선 크기 2, 3, 3, 4, 5 초기화
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

    private void InitBoardSetup()
    {
        setupController = new BoardSetupController(
            BoardSize,
            cellTemplate,
            transform,
            cells,
            boardStates,
            shipIdByCell,
            OnClickCell,
            OnRightClickCell,
            OnPointerEnterCell,
            OnPointerExitCell,
            OnDropCell
        );
    }

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

    private void InitPlacementController()
    {
        placementController = new BoardPlacementController(
            BoardSize,
            boardRole,
            boardStates,
            shipIdByCell,
            ships,
            IsPlacementLocked,
            RefreshCells,
            UpdateReadyButton,
            ResetShipDragItems,
            ClearShipPreview,
            RemoveShipVisual,
            ShowShipVisual,
            ClearAllShipVisuals
        );
    }

    private void InitAttackController()
    {
        attackController = new BattleAttackController(
            BoardSize,
            boardStates,
            shipIdByCell,
            ships,
            RefreshCell,
            ShowSunkShipVisual
        );
    }

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

    #endregion

    #region 입력 처리

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

    public void SelectShip(int shipId)
    {
        if (placementController == null)
        {
            return;
        }

        placementController.SelectShip(shipId);
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

    #region 보드 표시

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

    private void ShowShipVisual(ShipData ship)
    {
        if (shipVisualController == null)
        {
            return;
        }

        shipVisualController.ShowShip(ship);
    }

    private void RemoveShipVisual(int shipId)
    {
        if (shipVisualController == null)
        {
            return;
        }

        shipVisualController.RemoveShip(shipId);
    }

    private void ClearAllShipVisuals()
    {
        if (shipVisualController == null)
        {
            return;
        }

        shipVisualController.ClearAllShips();
    }

    // 침몰한 상대 함선의 위치 정보를 받아 보드 위에 함선 이미지 표시
    private void ShowSunkShipVisual(List<Vector2Int> positions, string shipStatusId)
    {
        if (boardRole != BoardRole.EnemyBoard)
        {
            return;
        }

        if (positions == null || positions.Count == 0)
        {
            return;
        }

        int shipId = GetShipIdFromStatusId(shipStatusId);

        if (shipId < 0)
        {
            return;
        }

        ShipData visualShip = new ShipData(shipId, positions.Count);
        visualShip.isPlaced = true;

        for (int i = 0; i < positions.Count; i++)
        {
            visualShip.positions.Add(positions[i]);
        }

        ShowShipVisual(visualShip);
    }

    private int GetShipIdFromStatusId(string shipStatusId)
    {
        switch (shipStatusId)
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

    #endregion

    #region 공격 판정 연결

    public AttackResult ReceiveAttack(int x, int y)
    {
        if (attackController == null)
        {
            return AttackResult.Invalid;
        }

        return attackController.ReceiveAttack(x, y);
    }

    public void ApplyAttackResult(
        int x,
        int y,
        string resultText,
        string sunkShipId,
        string aroundPositionsText,
        string sunkShipPositionsText
    )
    {
        if (attackController == null)
        {
            return;
        }

        attackController.ApplyAttackResult(x, y, resultText, sunkShipId, aroundPositionsText, sunkShipPositionsText);
    }

    public string GetLastSunkAroundPositionsText()
    {
        if (attackController == null)
        {
            return "";
        }

        return attackController.GetLastSunkAroundPositionsText();
    }

    public string GetLastSunkShipPositionsText()
    {
        if (attackController == null)
        {
            return "";
        }

        return attackController.GetLastSunkShipPositionsText();
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

    #region 상대 보드 좌표 반전 보류

    // 필요하면 UI 표시 전용으로만 적용
    // 네트워크 패킷 좌표는 원본 좌표 유지

    #endregion
}