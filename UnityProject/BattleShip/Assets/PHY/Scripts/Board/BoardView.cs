using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BoardView : MonoBehaviour
{
    [SerializeField] private BoardCell cellTemplate;

    private const int BoardSize = 11;

    private BoardCell[,] cells = new BoardCell[BoardSize, BoardSize];
    private CellState[,] boardStates = new CellState[BoardSize, BoardSize];
    private int[,] shipIDByCell = new int[BoardSize, BoardSize];

    [Header("타일 스프라이트")]
    [SerializeField] private Sprite waterSprite;
    [SerializeField] private Sprite landSprite;
    [SerializeField] private Sprite shipSprite;
    [SerializeField] private Sprite blockedSprite;
    [SerializeField] private Sprite hitSprite;
    [SerializeField] private Sprite missSprite;

    [Header("프리뷰 스프라이트")]
    [SerializeField] private Sprite previewShipSprite;      // 배 미리보기
    [SerializeField] private Sprite invalidPreviewSprite;   // 배를 놓을 수 없는 위치
    [SerializeField] private Sprite spacingPreviewSprite;   // 배 놓을 위치 주변 8칸 미리 보여주기

    private List<Vector2Int> previewPositions = new List<Vector2Int>();

    [Header("함선 세팅")]
    private ShipData[] ships;
    private int selectedShipID = -1;
    private int selectedShipSize = 0;

    [SerializeField] private Button readyButton;

    private ShipDirection currentDirection = ShipDirection.Horizontal;

    [SerializeField] private bool isShipSpacingRuleEnabled = true;

    // Todo: 상대 보드 UI 표시 / 상대 보드 클릭 좌표 변환
    //[SerializeField] private bool isMirrorView;

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
        new Vector2Int(1,3),
        new Vector2Int(4,2),
        new Vector2Int(5,8),
        new Vector2Int(7,2),
        new Vector2Int(8,5),
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

    private void Start()
    {
        Debug.Log("[BoardView] Start 실행");
        InitShips();

        CreateBoard();
        InitBoardState();
        ApplyLandTiles();
        RefreshCells();

        UpdateReadyButton();
    }

    private void CreateBoard()
    {
        cellTemplate.gameObject.SetActive(false);

        for (int y = 0; y < BoardSize; y++)
        {
            for (int x = 0; x < BoardSize; x++)
            {
                BoardCell cell = Instantiate(cellTemplate, transform);      // 추후 Pooling작업 예정

                cell.gameObject.SetActive(true);

                cell.Init(x, y, OnClickCell, OnRightClickCell, OnPointerEnterCell, OnPointerExitCell, OnDropCell);

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

    #region 육지 배치 함수
    // Todo: 기본 시스템들이 안정화 된 이후 추가확장으로 매판 섬 랜덤배치 가능
    // 다만 추가 검사가 들어가야 안정적으로 랜덤 배치를 할 수 있음
    // 1. 보드 밖으로 섬이 나가는지
    // 2. 배치된 다른 육지랑 겹치는지
    // 3. 배치 가능한 공간을 너무 막지는 않는지
    private void ApplyLandTiles()
    {
        ApplySingleLandTiles();
        ApplyLandShape(new Vector2Int(3, 3), islandShapeA);
        ApplyLandShape(new Vector2Int(7, 5), islandShapeB);
    }

    // 단일 육지 타일 배치
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

    // 섬 모양 
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

    #region 함선 배치
    public void SelectShip(int shipID)
    {
        Debug.Log($"[BoardView] SelectShip 호출됨: {shipID}");

        if (ships == null)
        {
            Debug.Log("BoardView is null");
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

        Debug.Log($"Selected Ship: ID={selectedShipID}, Size={selectedShipSize}");
        Debug.Log($"Direction: {currentDirection}");

    }

    public void RotateShip()
    {
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
    private void RefreshCells()
    {
        for (int y = 0; y < BoardSize; y++)
        {
            for (int x = 0; x < BoardSize; x++)
            {
                RefreshCell(x, y);
            }
        }
    }

    private void RefreshCell(int x, int y)
    {
        CellState state = boardStates[x, y];

        cells[x, y].SetState(state);
        cells[x, y].SetSprite(GetSpriteByState(state));
    }

    private Sprite GetSpriteByState(CellState state)
    {
        switch (state)
        {
            case CellState.Empty:
                return waterSprite;

            case CellState.Land:
                return landSprite;

            case CellState.Ship:
                return shipSprite;

            case CellState.Blocked:
                return blockedSprite;

            case CellState.Hit:
                return hitSprite;

            case CellState.Miss:
                return missSprite;

            default:
                return waterSprite;
        }
    }

    private bool IsInsideBoard(int x, int y)
    {
        return x >= 0 && x < BoardSize && y >= 0 && y < BoardSize;
    }

    public bool TryPlaceSelectedShipAt(BoardCell cell)
    {
        if (cell == null)
        {
            return false;
        }

        if (selectedShipID == -1)
        {
            Debug.LogWarning("[경고] 함선을 먼저 선택해야 함");
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

        ClearPreview();
        PlaceShip(selectedShipID, positions);

        return true;
    }

    // 함선 클릭 후 배치
    private void OnClickCell(BoardCell cell)
    {
        CellState state = boardStates[cell.X, cell.Y];

        Debug.Log($"Clicked Cell: X={cell.X}, Y={cell.Y}, State={cell.State}");

        if (selectedShipID == -1 && state == CellState.Ship)
        {
            SelectPlacedShipFromBoard(cell.X, cell.Y);
            return;
        }

        TryPlaceSelectedShipAt(cell);
    }

    private void OnRightClickCell(BoardCell cell)
    {
        if (selectedShipID == -1)
        {
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

    // 시작 좌표 보정
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

    // 함선을 실제로 배치할 수 있는 칸인지 검사하는 함수
    private bool CanPlaceShip(List<Vector2Int> positions, bool showLog = true)
    {
        for (int i = 0; i < positions.Count; i++)
        {
            Vector2Int position = positions[i];

            if (!IsInsideBoard(position.x, position.y))
            {
                if(showLog)
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
                if(showLog)
                {
                Debug.LogWarning($"[BoardView] 배치 실패: 이미 배가 있는 칸 X={position.x}, Y={position.y}");

                }
                return false;
            }

            if (state == CellState.Blocked)
            {
                if(showLog)
                {
                Debug.LogWarning($"[BoardView] 배치 실패: 배치 금지 칸 X={position.x}, Y={position.y}");

                }
                return false;
            }

            //if (isShipSpacingRuleEnabled && IsAdjacentToOtherShip(position, positions))
            //{
            //    if(showLog)
            //    {
            //    Debug.LogWarning($"[BoardView] 배치 실패: 다른 배와 8방향 인접 X={position.x}, Y={position.y}");

            //    }
            //    return false;
            //}
        }

        return true;
    }

    private bool IsAdjacentToOtherShip(Vector2Int position, List<Vector2Int> currentShipPositions)
    {
        for (int y = -1; y <= 1; y++)
        {
            for (int x = -1; x <= 1; x++)
            {
                if (x == 0 && y == 0) continue;

                int checkX = position.x + x;
                int checkY = position.y + y;

                if (!IsInsideBoard(checkX, checkY))
                {
                    continue;
                }

                Vector2Int checkPosition = new Vector2Int(checkX, checkY);

                if (currentShipPositions.Contains(checkPosition))
                {
                    continue;
                }

                if (boardStates[checkX, checkY] == CellState.Ship) return true;

            }

        }
        return false;
    }

    private void PlaceShip(int shipID, List<Vector2Int> positions)
    {
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

        selectedShipID = -1;
        selectedShipSize = 0;
        currentDirection = ShipDirection.Horizontal;

        UpdateReadyButton();
        Debug.Log($"[BoardView] 배치 완료: ShipID={shipID}, Size={ship.size}");

    }

    private void RemovePlacedShip(ShipData ship)
    {
        for (int i = 0; i < ship.positions.Count; i++)
        {
            Vector2Int position = ship.positions[i];

            if (!IsInsideBoard(position.x, position.y)) continue;

            if (boardStates[position.x, position.y] == CellState.Ship)
            {
                boardStates[position.x, position.y] = CellState.Empty;
                shipIDByCell[position.x, position.y] = -1;
            }
        }

        ship.positions.Clear();
        ship.isPlaced= false;

        RebuildBlockedCells();
        RefreshCells();

        UpdateReadyButton();
        Debug.Log($"[BoardView] 기존 배 위치 제거: ID={ship.shipID}, Size={ship.size}");

    }

    // 주변 프리뷰 좌표 구하기
    private List<Vector2Int> GetAroundShipPositions(List<Vector2Int> shipPositions)
    {
        List<Vector2Int> aroundPositions = new List<Vector2Int>();

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

                    Vector2Int aroundPosition = new Vector2Int(checkX, checkY);

                    if (shipPositions.Contains(aroundPosition))
                    {
                        continue;
                    }

                    if (aroundPositions.Contains(aroundPosition))
                    {
                        continue;
                    }

                    if (boardStates[checkX, checkY] != CellState.Empty)
                    {
                        continue;
                    }

                    aroundPositions.Add(aroundPosition);
                }
            }
        }

        return aroundPositions;
    }

    private void SelectPlacedShipFromBoard(int x, int y)
    {
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
        Debug.Log($"Direction: {currentDirection}");
    }

    private void OnPointerEnterCell(BoardCell cell)
    {
        if (selectedShipID == -1)
        {
            return;
        }

        ClearPreview();

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

        ShowPreview(positions, canPlace);
    }

    private void OnPointerExitCell(BoardCell cell)
    {
        ClearPreview();
    }

    private void OnDropCell(BoardCell cell, PointerEventData eventData)
    {
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

    private void ShowPreview(List<Vector2Int> positions, bool canPlace)
    {
        previewPositions.Clear();

        Sprite previewSprite = canPlace ? previewShipSprite : invalidPreviewSprite;

        if (previewSprite == null)
        {
            previewSprite = canPlace ? shipSprite : blockedSprite;
        }

        for (int i = 0; i < positions.Count; i++)
        {
            Vector2Int position = positions[i];

            if (!IsInsideBoard(position.x, position.y))
            {
                continue;
            }

            previewPositions.Add(position);
            cells[position.x, position.y].SetSprite(previewSprite);
        }
    }

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

    private void ClearPreview()
    {
        for (int i = 0; i < previewPositions.Count; i++)
        {
            Vector2Int position = previewPositions[i];

            if (!IsInsideBoard(position.x, position.y))
            {
                continue;
            }

            RefreshCell(position.x, position.y);
        }

        previewPositions.Clear();
    }

    private bool IsAllShipsPlaced()
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

    public void OnClickReady()
    {
        if (!IsAllShipsPlaced())
        {
            Debug.LogWarning("[BoardView] 아직 모든 배가 배치되지 않았음.");
            return;
        }

        Debug.Log("[BoardView] Ready 완료");
    }

    #region 상대 보드 UI 표시 / 상대 보드 클릭 좌표 변환
    // Todo : 나중에 기본적인 시스템이 안정화 된 이후 작업예정
    //private Vector2Int ConvertToDisplayPosition(Vector2Int originalPosition)
    //{
    //    if (!isMirrorView)
    //    {
    //        return originalPosition;
    //    }

    //    int mirrorX = BoardSize - 1 - originalPosition.x;
    //    return new Vector2Int(mirrorX, originalPosition.y);
    //}

    //private Vector2Int ConvertToOriginalPosition(Vector2Int displayPosition)
    //{
    //    if (!isMirrorView)
    //    {
    //        return displayPosition;
    //    }

    //    int originalX = BoardSize - 1 - displayPosition.x;
    //    return new Vector2Int(originalX, displayPosition.y);
    //}
    #endregion
}
