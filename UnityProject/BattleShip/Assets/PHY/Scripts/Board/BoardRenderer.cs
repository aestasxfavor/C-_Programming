using System;
using System.Collections.Generic;
using UnityEngine;

public class BoardRenderer
{
    private readonly int boardSize;
    private readonly BoardRole boardRole;
    private readonly BoardCell[,] cells;

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
        int boardSize,
        BoardRole boardRole,
        BoardCell[,] cells,
        Sprite waterSprite,
        Sprite landSprite,
        Sprite shipSprite,
        Sprite blockedSprite,
        Sprite hitSprite,
        Sprite missSprite,
        Sprite validPreviewSprite,
        Sprite invalidPreviewSprite,
        bool hideBlockedCellsOnBattle,
        bool hideCellShipSpriteWhenUsingOverlay,
        Func<bool> getBattleState,
        Func<bool> isShipVisualOverlayEnabled
    )
    {
        this.boardSize = boardSize;
        this.boardRole = boardRole;
        this.cells = cells;

        this.waterSprite = waterSprite;
        this.landSprite = landSprite;
        this.shipSprite = shipSprite;
        this.blockedSprite = blockedSprite;
        this.hitSprite = hitSprite;
        this.missSprite = missSprite;

        this.validPreviewSprite = validPreviewSprite;
        this.invalidPreviewSprite = invalidPreviewSprite;

        this.hideBlockedCellsOnBattle = hideBlockedCellsOnBattle;
        this.hideCellShipSpriteWhenUsingOverlay = hideCellShipSpriteWhenUsingOverlay;

        this.getBattleState = getBattleState;
        this.isShipVisualOverlayEnabled = isShipVisualOverlayEnabled;
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