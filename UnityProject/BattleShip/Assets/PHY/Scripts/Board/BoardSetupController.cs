using System;
using UnityEngine;
using UnityEngine.EventSystems;

// 보드 셀 생성, 초기 상태 리셋, 고정 육지 타일 배치를 담당하는 보드 초기화 컨트롤러
public class BoardSetupController
{
    private readonly int boardSize;
    private readonly BoardCell cellTemplate;
    private readonly Transform boardRoot;

    private readonly BoardCell[,] cells;
    private readonly CellState[,] boardStates;
    private readonly int[,] shipIdByCell;

    private readonly Action<BoardCell> onClickCell;
    private readonly Action<BoardCell> onRightClickCell;
    private readonly Action<BoardCell> onPointerEnterCell;
    private readonly Action<BoardCell> onPointerExitCell;
    private readonly Action<BoardCell, PointerEventData> onDropCell;

    // TODO: 육지 좌표 패턴은 추후 LandPatternSO로 분리 가능
    private readonly Vector2Int[] singleLandPositions =
    {
        new Vector2Int(1, 3),
        new Vector2Int(4, 2),
        new Vector2Int(5, 8),
        new Vector2Int(7, 2),
        new Vector2Int(8, 5),
    };

    // TODO: 고정 섬 모양 데이터는 추후 LandPatternSO로 분리 가능
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
        int _boardSize,
        BoardCell _cellTemplate,
        Transform _boardRoot,
        BoardCell[,] _cells,
        CellState[,] _boardStates,
        int[,] _shipIdByCell,
        Action<BoardCell> _onClickCell,
        Action<BoardCell> _onRightClickCell,
        Action<BoardCell> _onPointerEnterCell,
        Action<BoardCell> _onPointerExitCell,
        Action<BoardCell, PointerEventData> _onDropCell
    )
    {
        boardSize = _boardSize;
        cellTemplate = _cellTemplate;
        boardRoot = _boardRoot;

        cells = _cells;
        boardStates = _boardStates;
        shipIdByCell = _shipIdByCell;

        onClickCell = _onClickCell;
        onRightClickCell = _onRightClickCell;
        onPointerEnterCell = _onPointerEnterCell;
        onPointerExitCell = _onPointerExitCell;
        onDropCell = _onDropCell;
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
                shipIdByCell[x, y] = -1;
            }
        }
    }

    // 현재 과제에서는 고정된 육지 패턴만 배치
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