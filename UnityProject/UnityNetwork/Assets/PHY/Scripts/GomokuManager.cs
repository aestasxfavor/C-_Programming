using UnityEngine;


public enum StoneType
{
    empty = 0,
    black,
    white
}

public enum GameModeType
{
    single = 0,
    multi
}

public enum MyStoneType
{
    black = 0,
    white
}



public class GomokuManager : MonoBehaviour
{
    private static GomokuManager instance;
    public static GomokuManager Instance { get { return instance; } }


    public GameObject stonePF;

    private bool blackTurn = true;

    private StoneType[,] stones = new StoneType[19, 19];
    //private GameModeType gameMode = GameModeType.single;
    //public GameModeType GameMode { get { return gameMode; }  set { gameMode = value; } }
    public GameModeType GameMode { get; set; }

    public MyStoneType MyStone { get; set; }



    public bool IsRunning { get; set; }


    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this.gameObject);
    }

    private void Update()
    {
        if (!IsRunning)
            return;

        if (Input.GetMouseButtonDown(0))
        {
            Vector3 Pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            int x = Mathf.RoundToInt(Pos.x);
            int y = Mathf.RoundToInt(Pos.y);

            // 바둑판 영역 밖이면 돌을 두지 않고 리턴한다.
            if (x < 0 || x >= 19 || y < 0 || y >= 19)
                return;


            if (GameMode == GameModeType.single)
            {
                if (stones[x, y] == StoneType.empty)
                {
                    PutStone(x, y);
                }
            }
            else if (GameMode == GameModeType.multi)
            {
                // 너의 턴이 아니기에 리턴한다.
                if ((MyStone == MyStoneType.black && !blackTurn) ||
                    (MyStone == MyStoneType.white && blackTurn))
                    return;

                if (stones[x, y] == StoneType.empty)
                {
                    PutStone(x, y);
                    ChatManager.Instance.SendGomokuDataEvent(x, y);
                }
            }

        }
    }

    public void PutStone(int _x, int _y)
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
        int dx = _x;
        int dy = _y;
        while (true)
        {
            dx += _dirX;
            dy += _dirY;
            //영역 밖으로 나가면 반복문 끝낸다.
            if (dx < 0 || dx >= 19 || dy < 0 || dy >= 19)
                break;

            if (_putStone == stones[dx, dy])
                count++;
            else
                break;
        }
        return count;
    }
    private void GameOver()
    {
        string str = blackTurn ? "흑돌의 승리입니다." : "백돌의 승리입니다.";
        Debug.Log(str);
        IsRunning = false;
    }

}