using System;
using System.Collections.Generic;
using UnityEngine;

public class BoardPlacementController
{
    private readonly int boardSize;
    private readonly BoardRole boardRole;
    private readonly CellState[,] boardStates;
    private readonly int[,] shipIDByCell;
    private readonly ShipData[] ships;

    private readonly Func<bool> isPlacementLocked;
    private readonly Action refreshCells;
    private readonly Action updateReadyButton;
    private readonly Action resetShipDragItems;
    private readonly Action clearShipPreview;
    private readonly Action<int> removeShipVisual;
    private readonly Action<ShipData> showShipVisual;
    private readonly Action clearAllShipVisuals;

    private int selectedShipID = -1;
    private int selectedShipSize = 0;
    private ShipDirection currentDirection = ShipDirection.Horizontal;

    public bool HasSelectedShip
    {
        get { return selectedShipID != -1; }
    }

    public BoardPlacementController(
        int boardSize,
        BoardRole boardRole,
        CellState[,] boardStates,
        int[,] shipIDByCell,
        ShipData[] ships,
        Func<bool> isPlacementLocked,
        Action refreshCells,
        Action updateReadyButton,
        Action resetShipDragItems,
        Action clearShipPreview,
        Action<int> removeShipVisual,
        Action<ShipData> showShipVisual,
        Action clearAllShipVisuals
    )
    {
        this.boardSize = boardSize;
        this.boardRole = boardRole;
        this.boardStates = boardStates;
        this.shipIDByCell = shipIDByCell;
        this.ships = ships;

        this.isPlacementLocked = isPlacementLocked;
        this.refreshCells = refreshCells;
        this.updateReadyButton = updateReadyButton;
        this.resetShipDragItems = resetShipDragItems;
        this.clearShipPreview = clearShipPreview;
        this.removeShipVisual = removeShipVisual;
        this.showShipVisual = showShipVisual;
        this.clearAllShipVisuals = clearAllShipVisuals;
    }

    public void SelectShip(int shipID)
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

        clearShipPreview?.Invoke();
        PlaceShip(selectedShipID, positions);

        return true;
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

    public List<Vector2Int> GetPreviewShipPositions(BoardCell cell, out bool canPlace)
    {
        canPlace = false;

        if (cell == null)
        {
            return new List<Vector2Int>();
        }

        if (selectedShipID == -1)
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

    public bool TryPlaceShipForTest(int shipID, List<Vector2Int> positions)
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

        refreshCells?.Invoke();
        showShipVisual?.Invoke(ship);

        selectedShipID = -1;
        selectedShipSize = 0;
        currentDirection = ShipDirection.Horizontal;

        updateReadyButton?.Invoke();

        Debug.Log($"[BoardView] 배치 완료: ShipID={shipID}, Size={ship.size}");
    }

    private void RemovePlacedShip(ShipData ship)
    {
        removeShipVisual?.Invoke(ship.shipID);

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

        refreshCells?.Invoke();
        updateReadyButton?.Invoke();

        Debug.Log($"[BoardView] 기존 배 위치 제거: ID={ship.shipID}, Size={ship.size}");
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

    private bool IsLocked()
    {
        return isPlacementLocked != null && isPlacementLocked.Invoke();
    }

    private bool IsInsideBoard(int x, int y)
    {
        return x >= 0 && x < boardSize && y >= 0 && y < boardSize;
    }
}