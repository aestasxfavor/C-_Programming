using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// 2025-12-30

// 1. 바둑판을 출력한다. - 2차원 배열로
// 2. 내가 어디를 선택하는지 조작이되야 한다.
//    - 인풋 처리가 되야한다
//    - wasd 움직였을 때 C가 움직이는것이 보여야한다.
// 3. 게임에서 돌이 5개가 연결되었는지에 대한 체크를 해야한다.

namespace OmokEx
{
    public class Omok
    {
        private int[,] m_seat;
        private int m_size;

        private int m_curX;
        //이 값은 0보다 작아질수 없고 m_size 이상일수 없다.
        public int CurX
        {
            get { return m_curX; }
            set
            {
                m_curX = value;
                if (m_curX < 0)
                    m_curX = 0;
                else if (m_curX >= m_size)
                    m_curX = m_size - 1;
            }
        }
        private int m_curY;
        //이 값은 0보다 작아질수 없고 m_size 이상일수 없다.
        public int CurY
        {
            get { return m_curY; }
            set
            {
                m_curY = value;
                if (m_curY < 0)
                    m_curY = 0;
                else if (m_curY >= m_size)
                    m_curY = m_size - 1;
            }
        }

        private bool m_isBlackTurn;

        private bool m_isPlaing;

        private int[,] dirs = new int[,]
        {
        { +1, +0 }, // 가로
        { +0, +1 }, // 세로
        { +1, +1 }, // 대각선 오름차순
        { +1, -1 }  // 대각선 내림차순
        };

        /// <summary>
        /// 오목판의 생성자
        /// </summary>
        /// <param name="_size"> 이값은 최소 7 ~ 19 이다. </param>
        public Omok(int _size = 19)
        {
            if (_size < 7)
            {
                Console.WriteLine($"당신이 입력한 값이 {_size} 라서 최소값으로 변환합니다.");
                _size = 7;
                Console.ReadKey();
            }
            else if (_size > 19)
            {
                Console.WriteLine($"당신이 입력한 값이 {_size} 라서 최대값으로 변환합니다.");
                _size = 19;
                Console.ReadKey();
            }

            m_seat = new int[_size, _size];
            m_size = _size;

            for (int y = 0; y < _size; y++)
            {
                for (int x = 0; x < _size; x++)
                {
                    m_seat[y, x] = 0;
                }
            }

            CurX = _size / 2;
            CurY = _size / 2;

            m_isBlackTurn = true;
            m_isPlaing = true;
        }

        /// <summary>
        /// 현재 오목판의 상황을 출력해줌
        /// </summary>
        public void ShowScreen()
        {
            Console.Clear();
            for (int y = 0; y < m_seat.GetLength(0); y++)
            {
                for (int x = 0; x < m_seat.GetLength(1); x++)
                {
                    if (CurX == x && CurY == y)
                    {
                        if (m_isBlackTurn)
                            Console.Write("b");
                        else
                            Console.Write("w");
                    }
                    else if (m_seat[y, x] == 1)
                        Console.Write("B");
                    else if (m_seat[y, x] == 2)
                        Console.Write("W");
                    else if (m_seat[y, x] == 0)
                        Console.Write("'");

                    Console.Write(" ");
                }
                Console.WriteLine();
            }


            if (m_isBlackTurn)
                Console.WriteLine("흑돌 차례");
            else
                Console.WriteLine("백돌 차례");

        }

        /// <summary>
        /// 입력하는 조작
        /// </summary>
        public void InputOmok()
        {
            ConsoleKeyInfo keyInfo = Console.ReadKey();

            switch (keyInfo.Key)
            {
                case ConsoleKey.UpArrow:
                case ConsoleKey.W:
                    CurY--;
                    break;
                case ConsoleKey.DownArrow:
                case ConsoleKey.S:
                    CurY++;
                    break;
                case ConsoleKey.LeftArrow:
                case ConsoleKey.A:
                    CurX--;
                    break;
                case ConsoleKey.RightArrow:
                case ConsoleKey.D:
                    CurX++;
                    break;

                case ConsoleKey.Spacebar:
                    InputStone();
                    break;

                default:
                    break;
            }


        }

        /// <summary>
        /// 스페스바를 눌렀을때 돌을 놓는 메서드
        /// </summary>
        private void InputStone()
        {
            if (m_seat[CurY, CurX] == 0)
            {
                if (m_isBlackTurn)
                    m_seat[CurY, CurX] = 1;
                else
                    m_seat[CurY, CurX] = 2;


                if (!CheckFiveStone())
                    m_isBlackTurn = !m_isBlackTurn;
            }

        }

        private bool CheckFiveStone()
        {
            int x = CurX;
            int y = CurY;

            int checkNum = m_isBlackTurn ? 1 : 2;



            for (int i = 0; i < 4; i++)
                return CheckFiveStoneTwo(x, y, checkNum, dirs[i, 0], dirs[i, 1]);

            //CheckFiveStoneTwo(x, y, checkNum, 1, 0);
            //CheckFiveStoneTwo(x, y, checkNum, 0, 1);
            //CheckFiveStoneTwo(x, y, checkNum, 1, 1);
            //CheckFiveStoneTwo(x, y, checkNum, 1, -1);
            // 가로 계산
            //for (int i = 0; i < 5; i++)
            //{
            //    int count = 0;
            //    for (int j = 0; j < 5; j++)
            //    {
            //        int xDt = x + i + j - 4;
            //        int yDt = y;
            //        if (xDt < 0 || xDt >= m_size ||
            //            yDt < 0 || yDt >= m_size)
            //            continue;

            //        if (m_seat[yDt, xDt] == checkNum)
            //            count++;

            //        if (count == 5)
            //        {
            //            //오목 완성
            //            m_isPlaing = false;
            //            return true;
            //        }
            //    }
            //}
            //// 새로 계산
            //for (int i = 0; i < 5; i++)
            //{
            //    int count = 0;
            //    for (int j = 0; j < 5; j++)
            //    {
            //        int xDt = x;
            //        int yDt = y + i + j - 4;
            //        if (xDt < 0 || xDt >= m_size ||
            //            yDt < 0 || yDt >= m_size)
            //            continue;

            //        if (m_seat[yDt, xDt] == checkNum)
            //            count++;

            //        if (count == 5)
            //        {
            //            //오목 완성
            //            m_isPlaing = false;
            //            return true;
            //        }
            //    }
            //}



            //if (m_seat[y - 4, x] == checkNum &&
            //    m_seat[y - 3, x] == checkNum &&
            //    m_seat[y - 2, x] == checkNum &&
            //    m_seat[y - 1, x] == checkNum &&
            //    m_seat[y - 0, x] == checkNum)
            //{ 

            //}


            //if (m_seat[y, x - 4] == checkNum &&
            //    m_seat[y, x - 3] == checkNum &&
            //    m_seat[y, x - 2] == checkNum &&
            //    m_seat[y, x - 1] == checkNum &&
            //    m_seat[y, x - 0] == checkNum)
            //{

            //}
            //else if (m_seat[y, x - 3] == checkNum &&
            //    m_seat[y, x - 2] == checkNum &&
            //    m_seat[y, x - 1] == checkNum &&
            //    m_seat[y, x - 0] == checkNum &&
            //    m_seat[y, x + 1] == checkNum)
            //{

            //}
            //else if (m_seat[y, x - 2] == checkNum &&
            //    m_seat[y, x - 1] == checkNum &&
            //    m_seat[y, x - 0] == checkNum &&
            //    m_seat[y, x + 1] == checkNum &&
            //    m_seat[y, x + 2] == checkNum)
            //{

            //}
            //else if (m_seat[y, x - 1] == checkNum &&
            //    m_seat[y, x - 0] == checkNum &&
            //    m_seat[y, x + 1] == checkNum &&
            //    m_seat[y, x + 2] == checkNum &&
            //    m_seat[y, x + 3] == checkNum)
            //{

            //}
            //else if (m_seat[y, x + 0] == checkNum &&
            //    m_seat[y, x + 1] == checkNum &&
            //    m_seat[y, x + 2] == checkNum &&
            //    m_seat[y, x + 3] == checkNum &&
            //    m_seat[y, x + 4] == checkNum)
            //{

            //}




            return false;
        }

        private bool CheckFiveStoneTwo(int _x, int _y, int _checkNum, int _xDt, int _yDt)
        {
            for (int i = 0; i < 5; i++)
            {
                int count = 0;
                for (int j = 0; j < 5; j++)
                {
                    int xDt = _x; //= _x + i + j - 4;
                    int yDt = _y; //= _y;
                    if (_xDt == 1)
                        xDt = _x + i + j - 4;

                    if (_yDt == 1)
                        yDt = _y + i + j - 4;
                    else if (_yDt == -1)
                        yDt = _y - i - j + 4;

                    if (xDt < 0 || xDt >= m_size ||
                        yDt < 0 || yDt >= m_size)
                        continue;

                    if (m_seat[yDt, xDt] == _checkNum)
                        count++;

                    if (count == 5)
                    {
                        //오목 완성
                        m_isPlaing = false;
                        return true;
                    }
                }
            }


            return false;
        }

        public bool GetIsPlaing()
        {
            return m_isPlaing;
        }

        public void ShowResult()
        {
            if (m_isBlackTurn)
            {
                Console.WriteLine("흑돌의 승리입니다.");
            }
            else
            {
                Console.WriteLine("백돌의 승리입니다.");
            }
            Console.ReadLine();
        }

    }

    public class Program
    {
        static void Main()
        {
            Omok omok = new Omok(13);

            while (omok.GetIsPlaing())
            {
                omok.ShowScreen();
                omok.InputOmok();
            }
            omok.ShowResult();
        }
    }
}
