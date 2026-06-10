using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BoardInputController
{
    private readonly BoardRole boardRole;
    private readonly CellState[,] boardStates;

    private readonly BoardPlacementController placementController;

    private readonly Func<bool> checkBattleState;
    private readonly Func<bool> checkPlacementLocked;

    private readonly Action<int, int> requestEnemyAttack;
    private readonly Action<List<Vector2Int>, bool> showShipPreview;
    private readonly Action clearShipPreview;

    public BoardInputController(
        BoardRole boardRole,
        CellState[,] boardStates,
        BoardPlacementController placementController,
        Func<bool> isBattle,
        Func<bool> isPlacementLocked,
        Action<int, int> requestEnemyAttack,
        Action<List<Vector2Int>, bool> showShipPreview,
        Action clearShipPreview
    )
    {
        this.boardRole = boardRole;
        this.boardStates = boardStates;
        this.placementController = placementController;

        this.checkBattleState = isBattle;
        this.checkPlacementLocked = isPlacementLocked;

        this.requestEnemyAttack = requestEnemyAttack;
        this.showShipPreview = showShipPreview;
        this.clearShipPreview = clearShipPreview;
    }

    public bool TryPlaceSelectedShipAt(BoardCell cell)
    {
        if (placementController == null)
        {
            return false;
        }

        return placementController.TryPlaceSelectedShipAt(cell);
    }

    public void OnClickCell(BoardCell cell)
    {
        if (cell == null)
        {
            return;
        }

        if (IsBattle())
        {
            HandleBattleClick(cell);
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

        if (placementController != null && !placementController.HasSelectedShip && state == CellState.Ship)
        {
            placementController.SelectPlacedShipFromBoard(cell.X, cell.Y);
            return;
        }

        TryPlaceSelectedShipAt(cell);
    }

    public void OnRightClickCell(BoardCell cell)
    {
        if (cell == null)
        {
            return;
        }

        if (IsBattle())
        {
            return;
        }

        if (boardRole != BoardRole.MyBoard)
        {
            return;
        }

        if (placementController == null || !placementController.HasSelectedShip)
        {
            return;
        }

        if (IsPlacementLocked())
        {
            Debug.Log("[Placement] Ready 이후 배치 불가");
            return;
        }

        placementController.RotateShip();

        OnPointerEnterCell(cell);
    }

    public void OnPointerEnterCell(BoardCell cell)
    {
        if (cell == null)
        {
            return;
        }

        if (IsBattle())
        {
            return;
        }

        if (boardRole != BoardRole.MyBoard)
        {
            return;
        }

        if (placementController == null || !placementController.HasSelectedShip)
        {
            return;
        }

        clearShipPreview?.Invoke();

        List<Vector2Int> positions = placementController.GetPreviewShipPositions(cell, out bool canPlace);

        showShipPreview?.Invoke(positions, canPlace);
    }

    public void OnPointerExitCell(BoardCell cell)
    {
        if (IsBattle())
        {
            return;
        }

        if (boardRole != BoardRole.MyBoard)
        {
            return;
        }

        clearShipPreview?.Invoke();
    }

    public void OnDropCell(BoardCell cell, PointerEventData eventData)
    {
        if (cell == null)
        {
            return;
        }

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

        if (eventData == null || eventData.pointerDrag == null)
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

    private void HandleBattleClick(BoardCell cell)
    {
        if (boardRole == BoardRole.EnemyBoard)
        {
            requestEnemyAttack?.Invoke(cell.X, cell.Y);
            return;
        }

        Debug.Log("[Battle] 내 보드는 공격 대상이 아님");
    }

    private bool IsBattle()
    {
        return checkBattleState != null && checkBattleState.Invoke();
    }

    private bool IsPlacementLocked()
    {
        return checkPlacementLocked != null && checkPlacementLocked.Invoke();
    }
}