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

    [Header("전투 테스트")]
    [SerializeField] private bool autoPlaceTestShipForEnemyBoard;

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

    private ShipVisualController shipVisualController;
    private BoardRenderer boardRenderer;

    private string lastSunkAroundPositionsText = "";
    private string lastSunkShipId = "";

    [Header("함선 세팅")]
    private ShipData[] ships;
    private int selectedShipID = -1;
    private int selectedShipSize = 0;

    [SerializeField] private Button readyButton;

    [Header("배 선택 UI")]
    [SerializeField] private ShipDragItem[] shipDragItems;

    private ShipDirection currentDirection = ShipDirection.Horizontal;

    [SerializeField] private bool isShipSpacingRuleEnabled = true;

    private bool lastBattleState;

    private void Start()
    {
        Debug.Log("[BoardView] Start 실행");

        InitShips();

        CreateBoard();
        InitBoardState();
        ApplyLandTiles();

        InitShipVisualOverlay();
        InitBoardRenderer();

        if (autoPlaceTestShipForEnemyBoard)
        {
            PlaceTestShipsForEnemyBoard();
        }

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

    private readonly Vector2Int[] singleLandPositions =
    {
        new Vector2Int(1, 3),
        new Vector2Int(4, 2),
        new Vector2Int(5, 8),
        new Vector2Int(7, 2),
        new Vector2Int(8, 5),
    };

    private readonly Vector2Int[] islandShapeA =
    {
        new Vector2Int(0, 0),
        new Vector2Int(1, 0),
        new Vector2Int(2, 0),

        new Vector2Int(1, 1),

        new Vector2Int(1, 2),
        new Vector2Int(2, 2),
    };

    private readonly Vector2Int[] islandShapeB =
    {
        new Vector2Int(0, 0),
        new Vector2Int(1, 0),

        new Vector2Int(0, 1),
    };

    private void CreateBoard()
    {
        cellTemplate.gameObject.SetActive(false);

        for (int y = 0; y < BoardSize; y++)
        {
            for (int x = 0; x < BoardSize; x++)
            {
                BoardCell cell = Instantiate(cellTemplate, transform);

                cell.gameObject.SetActive(true);

                cell.Init(
                    x,
                    y,
                    OnClickCell,
                    OnRightClickCell,
                    OnPointerEnterCell,
                    OnPointerExitCell,
                    OnDropCell
                );

                cells[x, y] = cell;
            }
        }
    }

    private void InitBoardState()
    {
        for (int y = 0; y < BoardSize; y++)
        {
            for (int x = 0; x < BoardSize; x++)
            {
                boardStates[x, y] = CellState.Empty;
                shipIDByCell[x, y] = -1;
            }
        }
    }

    #region 육지 배치

    private void ApplyLandTiles()
    {
        ApplySingleLandTiles();
        ApplyLandShape(new Vector2Int(3, 3), islandShapeA);
        ApplyLandShape(new Vector2Int(7, 5), islandShapeB);
    }

    private void ApplySingleLandTiles()
    {
        for (int i = 0; i < singleLandPositions.Length; i++)
        {
            Vector2Int position = singleLandPositions[i];

            if (IsInsideBoard(position.x, position.y))
            {
                boardStates[position.x, position.y] = CellState.Land;
            }
        }
    }

    private void ApplyLandShape(Vector2Int startPosition, Vector2Int[] shape)
    {
        for (int i = 0; i < shape.Length; i++)
        {
            Vector2Int position = startPosition + shape[i];

            if (IsInsideBoard(position.x, position.y))
            {
                boardStates[position.x, position.y] = CellState.Land;
            }
        }
    }

    #endregion

    #region 함선 선택 / 회전

    public void SelectShip(int shipID)
    {
        if (boardRole != BoardRole.MyBoard)
        {
            return;
        }

        if (IsPlacementLocked())
        {
            Debug.Log("[Placement] Ready 이후 배 선택 불가");
            return;
        }

        Debug.Log($"[BoardView] SelectShip 호출됨: {shipID}");

        if (ships == null)
        {
            Debug.LogWarning("[BoardView] ships 초기화 필요");
            return;
        }

        if (shipID < 0 || shipID >= ships.Length)
        {
            Debug.LogError($"[BoardView] 잘못된 shipID입니다: {shipID}");
            return;
        }

        ShipData ship = ships[shipID];

        if (ship.isPlaced)
        {
            Debug.LogWarning($"[BoardView] 이미 배치한 배입니다. ID={ship.shipID}, Size={ship.size}");
            RemovePlacedShip(ship);
        }

        selectedShipID = ship.shipID;
        selectedShipSize = ship.size;
        currentDirection = ShipDirection.Horizontal;

        Debug.Log($"[BoardView] Selected Ship: ID={selectedShipID}, Size={selectedShipSize}");
        Debug.Log($"[BoardView] Direction: {currentDirection}");
    }

    public void RotateShip()
    {
        if (boardRole != BoardRole.MyBoard)
        {
            return;
        }

        Debug.Log($"[RotateShip] 회전 전 방향: {currentDirection}");

        if (currentDirection == ShipDirection.Horizontal)
        {
            currentDirection = ShipDirection.Vertical;
        }
        else
        {
            currentDirection = ShipDirection.Horizontal;
        }

        Debug.Log($"[RotateShip] 회전 후 방향: {currentDirection}");
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

    #region 로컬 전투 테스트 전용 배 자동 배치

    private void PlaceTestShipsForEnemyBoard()
    {
        if (boardRole != BoardRole.EnemyBoard)
        {
            return;
        }

        bool success = true;

        success &= TryPlaceTestShip(
            0,
            new List<Vector2Int>
            {
                new Vector2Int(0, 0),
                new Vector2Int(1, 0)
            }
        );

        success &= TryPlaceTestShip(
            1,
            new List<Vector2Int>
            {
                new Vector2Int(10, 0),
                new Vector2Int(10, 1),
                new Vector2Int(10, 2)
            }
        );

        success &= TryPlaceTestShip(
            2,
            new List<Vector2Int>
            {
                new Vector2Int(0, 6),
                new Vector2Int(1, 6),
                new Vector2Int(2, 6)
            }
        );

        success &= TryPlaceTestShip(
            3,
            new List<Vector2Int>
            {
                new Vector2Int(5, 10),
                new Vector2Int(6, 10),
                new Vector2Int(7, 10),
                new Vector2Int(8, 10)
            }
        );

        success &= TryPlaceTestShip(
            4,
            new List<Vector2Int>
            {
                new Vector2Int(10, 5),
                new Vector2Int(10, 6),
                new Vector2Int(10, 7),
                new Vector2Int(10, 8),
                new Vector2Int(10, 9)
            }
        );

        if (success)
        {
            Debug.Log("[Test] EnemyBoard 테스트 배 5척 자동 배치 완료");
        }
        else
        {
            Debug.LogWarning("[Test] EnemyBoard 테스트 배 자동 배치 중 일부 실패");
        }
    }

    private bool TryPlaceTestShip(int shipID, List<Vector2Int> positions)
    {
        if (shipID < 0 || shipID >= ships.Length)
        {
            Debug.LogWarning($"[Test] 잘못된 ShipID={shipID}");
            return false;
        }

        if (!CanPlaceShip(positions, false))
        {
            Debug.LogWarning($"[Test] 테스트 배 배치 실패: ShipID={shipID}");
            return false;
        }

        PlaceShip(shipID, positions);

        Debug.Log($"[Test] 테스트 배 배치 완료: ShipID={shipID}");

        return true;
    }

    #endregion

    #region 전투 판정

    public AttackResult ReceiveAttack(int x, int y)
    {
        lastSunkShipId = "";
        lastSunkAroundPositionsText = "";

        if (!CanAttackCell(x, y))
        {
            return AttackResult.Invalid;
        }

        CellState state = boardStates[x, y];

        if (state == CellState.Ship)
        {
            int shipID = shipIDByCell[x, y];

            boardStates[x, y] = CellState.Hit;
            RefreshCell(x, y);

            Debug.Log($"[Battle] Hit X={x}, Y={y}, ShipID={shipID}");

            if (IsShipSunk(shipID))
            {
                lastSunkShipId = ConvertShipIdToStatusId(shipID);

                MarkMissAroundSunkShip(shipID);

                if (IsAllShipsSunk())
                {
                    Debug.Log($"[Battle] 모든 함선 침몰, LastSunkShip={lastSunkShipId}");
                    return AttackResult.GameOver;
                }

                Debug.Log($"[Battle] Sunk ShipID={shipID}, LastSunkShip={lastSunkShipId}");
                return AttackResult.Sunk;
            }

            return AttackResult.Hit;
        }

        if (state == CellState.Empty || state == CellState.Blocked)
        {
            boardStates[x, y] = CellState.Miss;
            RefreshCell(x, y);

            Debug.Log($"[Battle] Miss X={x}, Y={y}");
            return AttackResult.Miss;
        }

        return AttackResult.Invalid;
    }

    private bool CanAttackCell(int x, int y)
    {
        if (!IsInsideBoard(x, y))
        {
            return false;
        }

        CellState state = boardStates[x, y];

        if (state == CellState.Hit || state == CellState.Miss)
        {
            return false;
        }

        if (state == CellState.Land)
        {
            return false;
        }

        return true;
    }

    private bool IsShipSunk(int shipID)
    {
        if (shipID < 0 || shipID >= ships.Length)
        {
            return false;
        }

        ShipData ship = ships[shipID];

        for (int i = 0; i < ship.positions.Count; i++)
        {
            Vector2Int position = ship.positions[i];

            if (!IsInsideBoard(position.x, position.y))
            {
                continue;
            }

            if (boardStates[position.x, position.y] != CellState.Hit)
            {
                return false;
            }
        }

        return true;
    }

    private void MarkMissAroundSunkShip(int shipID)
    {
        lastSunkAroundPositionsText = "";

        if (shipID < 0 || shipID >= ships.Length)
        {
            return;
        }

        ShipData ship = ships[shipID];

        HashSet<Vector2Int> aroundMissPositions = new HashSet<Vector2Int>();

        for (int i = 0; i < ship.positions.Count; i++)
        {
            Vector2Int shipPosition = ship.positions[i];

            for (int y = -1; y <= 1; y++)
            {
                for (int x = -1; x <= 1; x++)
                {
                    if (x == 0 && y == 0)
                    {
                        continue;
                    }

                    int checkX = shipPosition.x + x;
                    int checkY = shipPosition.y + y;

                    if (!IsInsideBoard(checkX, checkY))
                    {
                        continue;
                    }

                    CellState aroundState = boardStates[checkX, checkY];

                    if (aroundState == CellState.Hit || aroundState == CellState.Miss)
                    {
                        continue;
                    }

                    if (aroundState == CellState.Ship)
                    {
                        continue;
                    }

                    if (aroundState == CellState.Land)
                    {
                        continue;
                    }

                    if (aroundState == CellState.Empty || aroundState == CellState.Blocked)
                    {
                        boardStates[checkX, checkY] = CellState.Miss;
                        RefreshCell(checkX, checkY);

                        aroundMissPositions.Add(new Vector2Int(checkX, checkY));
                    }
                }
            }
        }

        lastSunkAroundPositionsText = ConvertPositionsToPacketText(aroundMissPositions);
    }

    private string ConvertPositionsToPacketText(HashSet<Vector2Int> positions)
    {
        if (positions == null || positions.Count == 0)
        {
            return "";
        }

        List<string> positionTexts = new List<string>();

        foreach (Vector2Int position in positions)
        {
            positionTexts.Add($"{position.x},{position.y}");
        }

        return string.Join(";", positionTexts);
    }

    private bool IsAllShipsSunk()
    {
        for (int i = 0; i < ships.Length; i++)
        {
            if (!ships[i].isPlaced)
            {
                return false;
            }

            if (!IsShipSunk(ships[i].shipID))
            {
                return false;
            }
        }

        return true;
    }

    public void ApplyAttackResult(int x, int y, string resultText, string aroundPositionsText)
    {
        if (!IsInsideBoard(x, y))
        {
            Debug.LogWarning($"[Result] 결과 적용 실패: 보드 밖 좌표 X={x}, Y={y}");
            return;
        }

        switch (resultText)
        {
            case "HIT":
                ApplyHitResult(x, y);
                break;

            case "MISS":
                ApplyMissResult(x, y);
                break;

            case "SUNK":
                ApplyHitResult(x, y);
                ApplyAroundMissPositions(aroundPositionsText);
                break;

            case "GAME_OVER":
                ApplyHitResult(x, y);
                ApplyAroundMissPositions(aroundPositionsText);
                break;

            default:
                Debug.LogWarning($"[Result] 알 수 없는 결과 타입: {resultText}");
                break;
        }
    }

    private void ApplyHitResult(int x, int y)
    {
        boardStates[x, y] = CellState.Hit;
        RefreshCell(x, y);

        Debug.Log($"[EnemyBoard] Hit 표시 X={x}, Y={y}");
    }

    private void ApplyMissResult(int x, int y)
    {
        boardStates[x, y] = CellState.Miss;
        RefreshCell(x, y);

        Debug.Log($"[EnemyBoard] Miss 표시 X={x}, Y={y}");
    }

    private void ApplyAroundMissPositions(string aroundPositionsText)
    {
        if (string.IsNullOrEmpty(aroundPositionsText))
        {
            return;
        }

        string[] positions = aroundPositionsText.Split(';');

        for (int i = 0; i < positions.Length; i++)
        {
            string positionText = positions[i];

            if (string.IsNullOrEmpty(positionText))
            {
                continue;
            }

            string[] xy = positionText.Split(',');

            if (xy.Length < 2)
            {
                continue;
            }

            if (!int.TryParse(xy[0], out int x) || !int.TryParse(xy[1], out int y))
            {
                continue;
            }

            if (!IsInsideBoard(x, y))
            {
                continue;
            }

            CellState state = boardStates[x, y];

            if (state == CellState.Hit || state == CellState.Miss || state == CellState.Land)
            {
                continue;
            }

            boardStates[x, y] = CellState.Miss;
            RefreshCell(x, y);

            Debug.Log($"[EnemyBoard] 침몰 주변 Miss 표시 X={x}, Y={y}");
        }
    }

    public string GetLastSunkAroundPositionsText()
    {
        return lastSunkAroundPositionsText;
    }

    public string GetLastSunkShipId()
    {
        return lastSunkShipId;
    }

    public bool CanRequestAttack(int x, int y)
    {
        return CanAttackCell(x, y);
    }

    #endregion

    #region 배치 처리

    public bool TryPlaceSelectedShipAt(BoardCell cell)
    {
        if (boardRole != BoardRole.MyBoard)
        {
            return false;
        }

        if (IsPlacementLocked())
        {
            Debug.Log("[Placement] Ready 이후 배치 불가");
            return false;
        }

        if (cell == null)
        {
            return false;
        }

        if (selectedShipID == -1)
        {
            Debug.LogWarning("[Placement] 함선을 먼저 선택해야 함");
            return false;
        }

        Vector2Int startPosition = GetClampedStartPosition(
            cell.X,
            cell.Y,
            selectedShipSize,
            currentDirection
        );

        List<Vector2Int> positions = GetShipPositions(
            startPosition.x,
            startPosition.y,
            selectedShipSize,
            currentDirection
        );

        if (!CanPlaceShip(positions))
        {
            return false;
        }

        ClearShipPreview();
        PlaceShip(selectedShipID, positions);

        return true;
    }

    private void OnClickCell(BoardCell cell)
    {
        if (IsBattle())
        {
            if (boardRole == BoardRole.EnemyBoard)
            {
                GameManager.Instance.TryAttackEnemyBoard(cell.X, cell.Y);
            }
            else
            {
                Debug.Log("[Battle] 내 보드는 공격 대상이 아님");
            }

            return;
        }

        if (boardRole != BoardRole.MyBoard)
        {
            return;
        }

        if (IsPlacementLocked())
        {
            Debug.Log("[Placement] Ready 이후 배치 불가");
            return;
        }

        CellState state = boardStates[cell.X, cell.Y];

        Debug.Log($"[BoardView] Clicked Cell: X={cell.X}, Y={cell.Y}, State={cell.State}");

        if (selectedShipID == -1 && state == CellState.Ship)
        {
            SelectPlacedShipFromBoard(cell.X, cell.Y);
            return;
        }

        TryPlaceSelectedShipAt(cell);
    }

    private void OnRightClickCell(BoardCell cell)
    {
        if (IsBattle())
        {
            return;
        }

        if (boardRole != BoardRole.MyBoard)
        {
            return;
        }

        if (selectedShipID == -1)
        {
            return;
        }

        if (IsPlacementLocked())
        {
            Debug.Log("[Placement] Ready 이후 배치 불가");
            return;
        }

        RotateShip();

        OnPointerEnterCell(cell);
    }

    private List<Vector2Int> GetShipPositions(int startX, int startY, int size, ShipDirection direction)
    {
        List<Vector2Int> positions = new List<Vector2Int>();

        for (int i = 0; i < size; i++)
        {
            int x = startX;
            int y = startY;

            if (direction == ShipDirection.Horizontal)
            {
                x += i;
            }
            else
            {
                y += i;
            }

            positions.Add(new Vector2Int(x, y));
        }

        return positions;
    }

    private Vector2Int GetClampedStartPosition(int startX, int startY, int size, ShipDirection direction)
    {
        if (direction == ShipDirection.Horizontal)
        {
            startX = Mathf.Clamp(startX, 0, BoardSize - size);
        }
        else
        {
            startY = Mathf.Clamp(startY, 0, BoardSize - size);
        }

        return new Vector2Int(startX, startY);
    }

    private bool CanPlaceShip(List<Vector2Int> positions, bool showLog = true)
    {
        for (int i = 0; i < positions.Count; i++)
        {
            Vector2Int position = positions[i];

            if (!IsInsideBoard(position.x, position.y))
            {
                if (showLog)
                {
                    Debug.LogWarning($"[BoardView] 배치 실패: 보드 밖 좌표 X={position.x}, Y={position.y}");
                }

                return false;
            }

            CellState state = boardStates[position.x, position.y];

            if (state == CellState.Land)
            {
                if (showLog)
                {
                    Debug.LogWarning($"[BoardView] 배치 실패: 육지 칸 X={position.x}, Y={position.y}");
                }

                return false;
            }

            if (state == CellState.Ship)
            {
                if (showLog)
                {
                    Debug.LogWarning($"[BoardView] 배치 실패: 이미 배가 있는 칸 X={position.x}, Y={position.y}");
                }

                return false;
            }

            if (state == CellState.Blocked)
            {
                if (showLog)
                {
                    Debug.LogWarning($"[BoardView] 배치 실패: 배치 금지 칸 X={position.x}, Y={position.y}");
                }

                return false;
            }
        }

        return true;
    }

    private void PlaceShip(int shipID, List<Vector2Int> positions)
    {
        if (shipID < 0 || shipID >= ships.Length)
        {
            Debug.LogError($"[BoardView] PlaceShip 실패: 잘못된 ShipID={shipID}");
            return;
        }

        ShipData ship = ships[shipID];

        for (int i = 0; i < positions.Count; i++)
        {
            Vector2Int position = positions[i];

            boardStates[position.x, position.y] = CellState.Ship;
            shipIDByCell[position.x, position.y] = shipID;
        }

        ship.positions.Clear();
        ship.positions.AddRange(positions);
        ship.isPlaced = true;

        RebuildBlockedCells();
        RefreshCells();
        CreateShipVisual(ship);

        selectedShipID = -1;
        selectedShipSize = 0;
        currentDirection = ShipDirection.Horizontal;

        UpdateReadyButton();

        Debug.Log($"[BoardView] 배치 완료: ShipID={shipID}, Size={ship.size}");
    }

    private void RemovePlacedShip(ShipData ship)
    {
        RemoveShipVisual(ship.shipID);

        for (int i = 0; i < ship.positions.Count; i++)
        {
            Vector2Int position = ship.positions[i];

            if (!IsInsideBoard(position.x, position.y))
            {
                continue;
            }

            if (boardStates[position.x, position.y] == CellState.Ship)
            {
                boardStates[position.x, position.y] = CellState.Empty;
                shipIDByCell[position.x, position.y] = -1;
            }
        }

        ship.positions.Clear();
        ship.isPlaced = false;

        RebuildBlockedCells();
        RefreshCells();

        UpdateReadyButton();

        Debug.Log($"[BoardView] 기존 배 위치 제거: ID={ship.shipID}, Size={ship.size}");
    }

    private void SelectPlacedShipFromBoard(int x, int y)
    {
        if (boardRole != BoardRole.MyBoard)
        {
            return;
        }

        if (IsPlacementLocked())
        {
            Debug.Log("[Placement] Ready 이후 배치 불가");
            return;
        }

        int shipID = shipIDByCell[x, y];

        if (shipID < 0 || shipID >= ships.Length)
        {
            Debug.LogWarning("[BoardView] 해당 칸의 shipID를 찾을 수 없음");
            return;
        }

        ShipData ship = ships[shipID];

        RemovePlacedShip(ship);

        selectedShipID = ship.shipID;
        selectedShipSize = ship.size;
        currentDirection = ShipDirection.Horizontal;

        Debug.Log($"[BoardView] 배 재배치 선택: ID={selectedShipID}, Size={selectedShipSize}");
        Debug.Log($"[BoardView] Direction: {currentDirection}");
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
        lastSunkShipId = "";
        lastSunkAroundPositionsText = "";

        ClearShipPreview();
        ClearAllShipVisuals();

        for (int y = 0; y < BoardSize; y++)
        {
            for (int x = 0; x < BoardSize; x++)
            {
                if (boardStates[x, y] == CellState.Ship || boardStates[x, y] == CellState.Blocked)
                {
                    boardStates[x, y] = CellState.Empty;
                }

                shipIDByCell[x, y] = -1;
            }
        }

        for (int i = 0; i < ships.Length; i++)
        {
            ships[i].positions.Clear();
            ships[i].isPlaced = false;
        }

        selectedShipID = -1;
        selectedShipSize = 0;
        currentDirection = ShipDirection.Horizontal;

        RefreshCells();
        UpdateReadyButton();
        ResetShipDragItems();

        Debug.Log("[Placement] 배치 전체 리셋 완료");
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

    #region 프리뷰 / 드래그

    private void OnPointerEnterCell(BoardCell cell)
    {
        if (IsBattle())
        {
            return;
        }

        if (boardRole != BoardRole.MyBoard)
        {
            return;
        }

        if (selectedShipID == -1)
        {
            return;
        }

        ClearShipPreview();

        Vector2Int startPosition = GetClampedStartPosition(
            cell.X,
            cell.Y,
            selectedShipSize,
            currentDirection
        );

        List<Vector2Int> positions = GetShipPositions(
            startPosition.x,
            startPosition.y,
            selectedShipSize,
            currentDirection
        );

        bool canPlace = CanPlaceShip(positions, false);

        ShowShipPreview(positions, canPlace);
    }

    private void OnPointerExitCell(BoardCell cell)
    {
        if (IsBattle())
        {
            return;
        }

        if (boardRole != BoardRole.MyBoard)
        {
            return;
        }

        ClearShipPreview();
    }

    private void OnDropCell(BoardCell cell, PointerEventData eventData)
    {
        if (IsBattle())
        {
            return;
        }

        if (boardRole != BoardRole.MyBoard)
        {
            return;
        }

        if (IsPlacementLocked())
        {
            Debug.Log("[Placement] Ready 이후 배치 불가");
            return;
        }

        if (eventData.pointerDrag == null)
        {
            return;
        }

        ShipDragItem dragItem = eventData.pointerDrag.GetComponent<ShipDragItem>();

        if (dragItem == null)
        {
            return;
        }

        bool success = TryPlaceSelectedShipAt(cell);

        if (success)
        {
            dragItem.MarkDroppedSuccessfully();
        }
    }

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

    #region 배 주변 Blocked 처리

    private void MarkBlockedAroundShip(List<Vector2Int> shipPositions)
    {
        for (int i = 0; i < shipPositions.Count; i++)
        {
            Vector2Int shipPosition = shipPositions[i];

            for (int y = -1; y <= 1; y++)
            {
                for (int x = -1; x <= 1; x++)
                {
                    if (x == 0 && y == 0)
                    {
                        continue;
                    }

                    int checkX = shipPosition.x + x;
                    int checkY = shipPosition.y + y;

                    if (!IsInsideBoard(checkX, checkY))
                    {
                        continue;
                    }

                    if (boardStates[checkX, checkY] == CellState.Empty)
                    {
                        boardStates[checkX, checkY] = CellState.Blocked;
                    }
                }
            }
        }
    }

    private void ClearBlockedCells()
    {
        for (int y = 0; y < BoardSize; y++)
        {
            for (int x = 0; x < BoardSize; x++)
            {
                if (boardStates[x, y] == CellState.Blocked)
                {
                    boardStates[x, y] = CellState.Empty;
                }
            }
        }
    }

    private void RebuildBlockedCells()
    {
        ClearBlockedCells();

        for (int i = 0; i < ships.Length; i++)
        {
            if (!ships[i].isPlaced)
            {
                continue;
            }

            MarkBlockedAroundShip(ships[i].positions);
        }
    }

    #endregion

    #region 상태 확인 / 유틸

    public bool IsAllShipsPlaced()
    {
        for (int i = 0; i < ships.Length; i++)
        {
            if (!ships[i].isPlaced)
            {
                return false;
            }
        }

        return true;
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

    private bool IsInsideBoard(int x, int y)
    {
        return x >= 0 && x < BoardSize && y >= 0 && y < BoardSize;
    }

    private string ConvertShipIdToStatusId(int shipID)
    {
        switch (shipID)
        {
            case 0:
                return "Ship2";

            case 1:
                return "Ship3A";

            case 2:
                return "Ship3B";

            case 3:
                return "Ship4";

            case 4:
                return "Ship5";

            default:
                return "";
        }
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