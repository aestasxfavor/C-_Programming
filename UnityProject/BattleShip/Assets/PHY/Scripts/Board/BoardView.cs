using UnityEngine;

public class BoardView : MonoBehaviour
{
    [SerializeField] private BoardCell cellTemplate;

    private const int BoardSize = 11;

    private BoardCell[,] cells = new BoardCell[BoardSize, BoardSize];
    private void Start()
    {
        CreateBoard();
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

                cell.Init(x, y, OnClickCell);
            }
        }
    }

    private void OnClickCell(BoardCell cell)
    {
        Debug.Log($"Clicked Cell: X={cell.X}, Y={cell.Y}, State={cell.State}");
    }
}
