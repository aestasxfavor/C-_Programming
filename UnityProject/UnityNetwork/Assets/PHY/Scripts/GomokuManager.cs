using UnityEngine;


public enum StoneType
{
    empty = 0,
    black,
    white
}

public class GomokuManager : MonoBehaviour
{

    public GameObject stonePF;

    private bool blackTurn;

    private StoneType[,] stones = new StoneType[19, 19];


    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 Pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            int x = Mathf.RoundToInt(Pos.x);
            int y = Mathf.RoundToInt(Pos.y);

            if (x < 0 || x >= 19 || y < 0 || y >= 19)
                return;
            if (stones[x, y] == StoneType.empty)
            {
                PutStone(x, y);
            }


        }
    }

    private void PutStone(int _x, int _y)
    {
        GameObject stone = Instantiate(stonePF, new Vector3(_x, _y, 0f), Quaternion.identity);
        stone.GetComponent<Stone>().SetInit(blackTurn);
        stones[_x, _y] = blackTurn ? StoneType.black : StoneType.white;

        if (CheckStone(_x, _y))
        {
            // 게임 끝
            GameOver();
        }
        else
        {
            blackTurn = !blackTurn;
        }

    }

    private bool CheckStone(int _x, int _y)
    {
        StoneType putStone = stones[_x, _y];

        int[] dirX = new int[4] { 1, 0, 1, 1 };
        int[] dirY = new int[4] { 0, 1, 1, -1 };

        for (int i = 0; i < 4; i++)
        {
            int count = 1;
            count += GetCheckCount(_x, _y, -dirX[i], -dirY[i], putStone);
            count += GetCheckCount(_x, _y, dirX[i], dirY[i], putStone);
            // 5줄이라서 승패완료.
            if (count == 5)
                return true;
        }
        return false;
    }
    private int GetCheckCount(int _x, int _y, int _dirX, int _dirY, StoneType _putStone)
    {
        int count = 0;
        int dx = _x + _dirX;
        int dy = _y + _dirY;
        while (dx >= 0)
        {
            //영역 밖으로 나가면 반복문 끝낸다.
            if (dx < 0 || dx >= 19 || dy < 0 || dy >= 19)
                break;

            if (_putStone == stones[dx, dy])
                count++;
            else
                break;
            dx += _dirX;
            dy += _dirY;
        }
        return count;
    }
    private void GameOver()
    {
        string str = blackTurn ? "흑돌의 승리입니다." : "백돌의 승리입니다.";
        Debug.Log(str);
    }

}