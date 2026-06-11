using System;
using System.Collections.Generic;
using UnityEngine;

// 보드 CellState에 맞춰 타일 Sprite와 X/O 표시, 배치 프리뷰 표시를 담당하는 렌더러
public class BoardRenderer
{
    private readonly int boardSize;
    private readonly BoardRole boardRole;
    private readonly BoardCell[,] cells;

    // TODO: 보드 타일 Sprite 묶음은 추후 BoardVisualConfigSO로 분리 가능
    private readonly Sprite waterSprite;
    private readonly Sprite landSprite;
    private readonly Sprite shipSprite;
    private readonly Sprite blockedSprite;
    private readonly Sprite hitSprite;
    private readonly Sprite missSprite;

    private readonly Sprite validPreviewSprite;
    private readonly Sprite invalidPreviewSprite;

    private readonly bool hideBlockedCellsOnBattle;
    private readonly bool hideCellShipSpriteWhenUsingOverlay;

    private readonly Func<bool> getBattleState;
    private readonly Func<bool> isShipVisualOverlayEnabled;

    private readonly List<Vector2Int> previewCells = new List<Vector2Int>();

    public BoardRenderer(
        int _boardSize,
        BoardRole _boardRole,
        BoardCell[,] _cells,
        Sprite _waterSprite,
        Sprite _landSprite,
        Sprite _shipSprite,
        Sprite _blockedSprite,
        Sprite _hitSprite,
        Sprite _missSprite,
        Sprite _validPreviewSprite,
        Sprite _invalidPreviewSprite,
        bool _hideBlockedCellsOnBattle,
        bool _hideCellShipSpriteWhenUsingOverlay,
        Func<bool> _getBattleState,
        Func<bool> _isShipVisualOverlayEnabled
    )
    {
        boardSize = _boardSize;
        boardRole = _boardRole;
        cells = _cells;

        waterSprite = _waterSprite;
        landSprite = _landSprite;
        shipSprite = _shipSprite;
        blockedSprite = _blockedSprite;
        hitSprite = _hitSprite;
        missSprite = _missSprite;

        validPreviewSprite = _validPreviewSprite;
        invalidPreviewSprite = _invalidPreviewSprite;

        hideBlockedCellsOnBattle = _hideBlockedCellsOnBattle;
        hideCellShipSpriteWhenUsingOverlay = _hideCellShipSpriteWhenUsingOverlay;

        getBattleState = _getBattleState;
        isShipVisualOverlayEnabled = _isShipVisualOverlayEnabled;
    }

    public void RefreshAllCells(CellState[,] boardStates)
    {
        if (boardStates == null)
        {
            return;
        }

        for (int y = 0; y < boardSize; y++)
        {
            for (int x = 0; x < boardSize; x++)
            {
                RefreshCell(x, y, boardStates);
            }
        }
    }

    public void RefreshCell(int x, int y, CellState[,] boardStates)
    {
        if (!IsInsideBoard(x, y))
        {
            return;
        }

        if (cells == null || cells[x, y] == null)
        {
            return;
        }

        if (boardStates == null)
        {
            return;
        }

        CellState state = boardStates[x, y];

        cells[x, y].SetState(state);
        cells[x, y].SetSprite(GetSpriteForCell(state));
    }

    // 배치 가능 여부에 따라 프리뷰 Sprite 표시
    public void ShowShipPreview(List<Vector2Int> shipPositions, bool canPlace)
    {
        if (shipPositions == null)
        {
            return;
        }

        previewCells.Clear();

        Sprite previewSprite = canPlace ? validPreviewSprite : invalidPreviewSprite;

        if (previewSprite == null)
        {
            previewSprite = canPlace ? shipSprite : blockedSprite;
        }

        for (int i = 0; i < shipPositions.Count; i++)
        {
            Vector2Int position = shipPositions[i];

            if (!IsInsideBoard(position.x, position.y))
            {
                continue;
            }

            if (cells == null || cells[position.x, position.y] == null)
            {
                continue;
            }

            previewCells.Add(position);
            cells[position.x, position.y].SetSprite(previewSprite);
        }
    }

    public void ClearShipPreview(CellState[,] boardStates)
    {
        for (int i = 0; i < previewCells.Count; i++)
        {
            Vector2Int position = previewCells[i];

            if (!IsInsideBoard(position.x, position.y))
            {
                continue;
            }

            RefreshCell(position.x, position.y, boardStates);
        }

        previewCells.Clear();
    }

    // CellState에 맞는 타일 Sprite 선택
    private Sprite GetSpriteForCell(CellState state)
    {
        if (boardRole == BoardRole.EnemyBoard)
        {
            if (state == CellState.Ship || state == CellState.Blocked)
            {
                return waterSprite;
            }
        }

        switch (state)
        {
            case CellState.Empty:
                return waterSprite;

            case CellState.Land:
                return landSprite;

            case CellState.Ship:
                if (ShouldHideShipCell())
                {
                    return waterSprite;
                }

                return shipSprite;

            case CellState.Blocked:
                if (ShouldHideBlockedCell())
                {
                    return waterSprite;
                }

                return blockedSprite;

            case CellState.Hit:
                return hitSprite;

            case CellState.Miss:
                return missSprite;

            case CellState.SunkShip:
                if (shipSprite != null)
                {
                    return shipSprite;
                }

                return hitSprite;

            default:
                return waterSprite;
        }
    }

    private bool ShouldHideBlockedCell()
    {
        if (!hideBlockedCellsOnBattle)
        {
            return false;
        }

        if (boardRole != BoardRole.MyBoard)
        {
            return false;
        }

        if (getBattleState == null)
        {
            return false;
        }

        return getBattleState.Invoke();
    }

    private bool ShouldHideShipCell()
    {
        if (!hideCellShipSpriteWhenUsingOverlay)
        {
            return false;
        }

        if (isShipVisualOverlayEnabled == null)
        {
            return false;
        }

        return isShipVisualOverlayEnabled.Invoke();
    }

    private bool IsInsideBoard(int x, int y)
    {
        return x >= 0 && x < boardSize && y >= 0 && y < boardSize;
    }
}