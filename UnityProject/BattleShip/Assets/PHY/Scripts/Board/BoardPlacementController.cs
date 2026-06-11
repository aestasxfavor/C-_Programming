using System;
using System.Collections.Generic;
using UnityEngine;

// 함선 선택, 회전, 배치 가능 여부 검사, 8방향 인접 금지 처리를 담당하는 배치 컨트롤러
public class BoardPlacementController
{
    #region 필드 / 외부 콜백

    private readonly int boardSize;
    private readonly BoardRole boardRole;
    private readonly CellState[,] boardStates;
    private readonly int[,] shipIdByCell;
    private readonly ShipData[] ships;

    private readonly Func<bool> checkPlacementLocked;
    private readonly Action refreshCells;
    private readonly Action updateReadyButton;
    private readonly Action resetShipDragItems;
    private readonly Action clearShipPreview;
    private readonly Action<int> removeShipVisual;
    private readonly Action<ShipData> showShipVisual;
    private readonly Action clearAllShipVisuals;

    // TODO: 배치 방향과 인접 규칙은 추후 BattleShipRuleConfigSO로 분리 가능
    private int selectedShipId = -1;
    private int selectedShipSize = 0;
    private ShipDirection currentDirection = ShipDirection.Horizontal;

    public bool HasSelectedShip
    {
        get { return selectedShipId != -1; }
    }

    #endregion

    #region 생성자

    public BoardPlacementController(
        int _boardSize,
        BoardRole _boardRole,
        CellState[,] _boardStates,
        int[,] _shipIdByCell,
        ShipData[] _ships,
        Func<bool> _isPlacementLocked,
        Action _refreshCells,
        Action _updateReadyButton,
        Action _resetShipDragItems,
        Action _clearShipPreview,
        Action<int> _removeShipVisual,
        Action<ShipData> _showShipVisual,
        Action _clearAllShipVisuals
    )
    {
        boardSize = _boardSize;
        boardRole = _boardRole;
        boardStates = _boardStates;
        shipIdByCell = _shipIdByCell;
        ships = _ships;

        checkPlacementLocked = _isPlacementLocked;
        refreshCells = _refreshCells;
        updateReadyButton = _updateReadyButton;
        resetShipDragItems = _resetShipDragItems;
        clearShipPreview = _clearShipPreview;
        removeShipVisual = _removeShipVisual;
        showShipVisual = _showShipVisual;
        clearAllShipVisuals = _clearAllShipVisuals;
    }

    #endregion

    #region 선택 / 회전

    public void SelectShip(int shipId)
    {
        if (boardRole != BoardRole.MyBoard)
        {
            return;
        }

        if (IsLocked())
        {
            Debug.Log("[Placement] Ready 이후 배 선택 불가");
            return;
        }

        Debug.Log($"[BoardView] SelectShip 호출됨: {shipId}");

        if (ships == null)
        {
            Debug.LogWarning("[BoardView] ships 초기화 필요");
            return;
        }

        if (shipId < 0 || shipId >= ships.Length)
        {
            Debug.LogError($"[BoardView] 잘못된 shipID입니다: {shipId}");
            return;
        }

        ShipData ship = ships[shipId];

        if (ship.isPlaced)
        {
            Debug.LogWarning($"[BoardView] 이미 배치한 배입니다. ID={ship.shipId}, Size={ship.size}");
            RemovePlacedShip(ship);
        }

        selectedShipId = ship.shipId;
        selectedShipSize = ship.size;
        currentDirection = ShipDirection.Horizontal;

        Debug.Log($"[BoardView] Selected Ship: ID={selectedShipId}, Size={selectedShipSize}");
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

    public void SelectPlacedShipFromBoard(int x, int y)
    {
        if (boardRole != BoardRole.MyBoard)
        {
            return;
        }

        if (IsLocked())
        {
            Debug.Log("[Placement] Ready 이후 배치 불가");
            return;
        }

        int shipId = shipIdByCell[x, y];

        if (shipId < 0 || shipId >= ships.Length)
        {
            Debug.LogWarning("[BoardView] 해당 칸의 shipID를 찾을 수 없음");
            return;
        }

        ShipData ship = ships[shipId];

        RemovePlacedShip(ship);

        selectedShipId = ship.shipId;
        selectedShipSize = ship.size;
        currentDirection = ShipDirection.Horizontal;

        Debug.Log($"[BoardView] 배 재배치 선택: ID={selectedShipId}, Size={selectedShipSize}");
        Debug.Log($"[BoardView] Direction: {currentDirection}");
    }

    #endregion

    #region 배치 처리

    public bool TryPlaceSelectedShipAt(BoardCell cell)
    {
        if (boardRole != BoardRole.MyBoard)
        {
            return false;
        }

        if (IsLocked())
        {
            Debug.Log("[Placement] Ready 이후 배치 불가");
            return false;
        }

        if (cell == null)
        {
            return false;
        }

        if (selectedShipId == -1)
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

        clearShipPreview?.Invoke();
        PlaceShip(selectedShipId, positions);

        return true;
    }

    private void PlaceShip(int shipId, List<Vector2Int> positions)
    {
        if (shipId < 0 || shipId >= ships.Length)
        {
            Debug.LogError($"[BoardView] PlaceShip 실패: 잘못된 ShipID={shipId}");
            return;
        }

        ShipData ship = ships[shipId];

        for (int i = 0; i < positions.Count; i++)
        {
            Vector2Int position = positions[i];

            boardStates[position.x, position.y] = CellState.Ship;
            shipIdByCell[position.x, position.y] = shipId;
        }

        ship.positions.Clear();
        ship.positions.AddRange(positions);
        ship.isPlaced = true;

        RebuildBlockedCells();

        refreshCells?.Invoke();
        showShipVisual?.Invoke(ship);

        selectedShipId = -1;
        selectedShipSize = 0;
        currentDirection = ShipDirection.Horizontal;

        updateReadyButton?.Invoke();

        Debug.Log($"[BoardView] 배치 완료: ShipID={shipId}, Size={ship.size}");
    }

    private void RemovePlacedShip(ShipData ship)
    {
        removeShipVisual?.Invoke(ship.shipId);

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
                shipIdByCell[position.x, position.y] = -1;
            }
        }

        ship.positions.Clear();
        ship.isPlaced = false;

        RebuildBlockedCells();

        refreshCells?.Invoke();
        updateReadyButton?.Invoke();

        Debug.Log($"[BoardView] 기존 배 위치 제거: ID={ship.shipId}, Size={ship.size}");
    }

    #endregion

    #region 프리뷰

    public List<Vector2Int> GetPreviewShipPositions(BoardCell cell, out bool canPlace)
    {
        canPlace = false;

        if (cell == null)
        {
            return new List<Vector2Int>();
        }

        if (selectedShipId == -1)
        {
            return new List<Vector2Int>();
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

        canPlace = CanPlaceShip(positions, false);

        return positions;
    }

    #endregion

    #region 리셋 / 상태 확인

    public void ResetPlacement()
    {
        clearShipPreview?.Invoke();
        clearAllShipVisuals?.Invoke();

        for (int y = 0; y < boardSize; y++)
        {
            for (int x = 0; x < boardSize; x++)
            {
                if (boardStates[x, y] == CellState.Ship || boardStates[x, y] == CellState.Blocked)
                {
                    boardStates[x, y] = CellState.Empty;
                }

                shipIdByCell[x, y] = -1;
            }
        }

        for (int i = 0; i < ships.Length; i++)
        {
            ships[i].positions.Clear();
            ships[i].isPlaced = false;
        }

        selectedShipId = -1;
        selectedShipSize = 0;
        currentDirection = ShipDirection.Horizontal;

        refreshCells?.Invoke();
        updateReadyButton?.Invoke();
        resetShipDragItems?.Invoke();

        Debug.Log("[Placement] 배치 전체 리셋 완료");
    }

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

    #endregion

    #region 위치 계산

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
            startX = Mathf.Clamp(startX, 0, boardSize - size);
        }
        else
        {
            startY = Mathf.Clamp(startY, 0, boardSize - size);
        }

        return new Vector2Int(startX, startY);
    }

    #endregion

    #region 배치 검사

    // 보드 범위, 육지, 다른 함선, 8방향 인접 금지 조건 검사
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

    #endregion

    #region 8방향 인접 금지 처리

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

    // 배치된 함선 주변 8방향을 배치 불가 영역으로 표시
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
        for (int y = 0; y < boardSize; y++)
        {
            for (int x = 0; x < boardSize; x++)
            {
                if (boardStates[x, y] == CellState.Blocked)
                {
                    boardStates[x, y] = CellState.Empty;
                }
            }
        }
    }

    #endregion

    #region 내부 유틸

    private bool IsLocked()
    {
        return checkPlacementLocked != null && checkPlacementLocked.Invoke();
    }

    private bool IsInsideBoard(int x, int y)
    {
        return x >= 0 && x < boardSize && y >= 0 && y < boardSize;
    }

    #endregion
}