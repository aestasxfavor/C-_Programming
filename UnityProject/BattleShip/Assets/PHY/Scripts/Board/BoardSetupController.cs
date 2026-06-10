using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class BoardSetupController
{
    private readonly int boardSize;
    private readonly BoardCell cellTemplate;
    private readonly Transform boardRoot;

    private readonly BoardCell[,] cells;
    private readonly CellState[,] boardStates;
    private readonly int[,] shipIDByCell;

    private readonly Action<BoardCell> onClickCell;
    private readonly Action<BoardCell> onRightClickCell;
    private readonly Action<BoardCell> onPointerEnterCell;
    private readonly Action<BoardCell> onPointerExitCell;
    private readonly Action<BoardCell, PointerEventData> onDropCell;

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

    public BoardSetupController(
        int boardSize,
        BoardCell cellTemplate,
        Transform boardRoot,
        BoardCell[,] cells,
        CellState[,] boardStates,
        int[,] shipIDByCell,
        Action<BoardCell> onClickCell,
        Action<BoardCell> onRightClickCell,
        Action<BoardCell> onPointerEnterCell,
        Action<BoardCell> onPointerExitCell,
        Action<BoardCell, PointerEventData> onDropCell
    )
    {
        this.boardSize = boardSize;
        this.cellTemplate = cellTemplate;
        this.boardRoot = boardRoot;

        this.cells = cells;
        this.boardStates = boardStates;
        this.shipIDByCell = shipIDByCell;

        this.onClickCell = onClickCell;
        this.onRightClickCell = onRightClickCell;
        this.onPointerEnterCell = onPointerEnterCell;
        this.onPointerExitCell = onPointerExitCell;
        this.onDropCell = onDropCell;
    }

    public void SetupBoard()
    {
        CreateCells();
        ResetBoardState();
        PlaceLandTiles();
    }

    private void CreateCells()
    {
        if (cellTemplate == null)
        {
            Debug.LogError("[BoardSetup] CellTemplate 연결 필요");
            return;
        }

        cellTemplate.gameObject.SetActive(false);

        for (int y = 0; y < boardSize; y++)
        {
            for (int x = 0; x < boardSize; x++)
            {
                BoardCell cell = UnityEngine.Object.Instantiate(cellTemplate, boardRoot);

                cell.gameObject.SetActive(true);

                cell.Init(
                    x,
                    y,
                    onClickCell,
                    onRightClickCell,
                    onPointerEnterCell,
                    onPointerExitCell,
                    onDropCell
                );

                cells[x, y] = cell;
            }
        }
    }

    private void ResetBoardState()
    {
        for (int y = 0; y < boardSize; y++)
        {
            for (int x = 0; x < boardSize; x++)
            {
                boardStates[x, y] = CellState.Empty;
                shipIDByCell[x, y] = -1;
            }
        }
    }

    private void PlaceLandTiles()
    {
        PlaceSingleLandTiles();
        PlaceLandShape(new Vector2Int(3, 3), islandShapeA);
        PlaceLandShape(new Vector2Int(7, 5), islandShapeB);
    }

    private void PlaceSingleLandTiles()
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

    private void PlaceLandShape(Vector2Int startPosition, Vector2Int[] shape)
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

    private bool IsInsideBoard(int x, int y)
    {
        return x >= 0 && x < boardSize && y >= 0 && y < boardSize;
    }
}