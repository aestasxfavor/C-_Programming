using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

// 보드 클릭, 우클릭, 드래그 앤 드랍 입력을 배치 또는 전투 처리로 분기하는 입력 컨트롤러
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
        BoardRole _boardRole,
        CellState[,] _boardStates,
        BoardPlacementController _placementController,
        Func<bool> _isBattle,
        Func<bool> _isPlacementLocked,
        Action<int, int> _requestEnemyAttack,
        Action<List<Vector2Int>, bool> _showShipPreview,
        Action _clearShipPreview
    )
    {
        boardRole = _boardRole;
        boardStates = _boardStates;
        placementController = _placementController;

        checkBattleState = _isBattle;
        checkPlacementLocked = _isPlacementLocked;

        requestEnemyAttack = _requestEnemyAttack;
        showShipPreview = _showShipPreview;
        clearShipPreview = _clearShipPreview;
    }

    public bool TryPlaceSelectedShipAt(BoardCell cell)
    {
        if (placementController == null)
        {
            return false;
        }

        return placementController.TryPlaceSelectedShipAt(cell);
    }

    // 현재 게임 단계에 따라 배치 선택 또는 공격 요청으로 입력 분기
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

    // 전투 중 상대 보드 클릭 시 공격 요청
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