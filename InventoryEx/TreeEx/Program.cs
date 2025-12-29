using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Omok
{

    public class Omok
    {
        //오목 판 사이즈
        private int m_size;
        private int[,] m_seat;

        //현재 내가 둘수 있는 좌표 X값
        private int m_curX;
        public int CurX
        {
            get { return m_curX; }
            set
            {
                m_curX = value;
                if (m_curX < 0)
                    m_curX = 0;
                else if (m_curX > m_size - 1)
                    m_curX = m_size - 1;
            }
        }

        //현재 내가 둘수 있는 좌표 Y값
        private int m_curY;
        public int CurY
        {
            get { return m_curY; }
            set
            {
                m_curY = value;
                if (m_curY < 0)
                    m_curY = 0;
                else if (m_curY > m_size - 1)
                    m_curY = m_size - 1;
            }
        }

        public bool m_isBlackTurn;
        public bool IsPlaying { get; set; }

        /// <summary>
        /// 오목의 생성자
        /// </summary>
        /// <param name="_size"> 오목판 사이즈 </param>
        public Omok(int _size)
        {
            m_size = _size;
            CurX = m_size / 2;
            CurY = m_size / 2;
            m_seat = new int[m_size, m_size];
            for (int y = 0; y < m_size; y++)
            {
                for (int x = 0; x < m_size; x++)
                {
                    m_seat[y, x] = 0;
                }
            }
            m_isBlackTurn = true;
            IsPlaying = true;
        }


        /// <summary>
        /// 처음 시작했을때 오목판을 세팅하는 메서드
        /// </summary>
        /// <returns></returns>

        /// <summary>
        /// 현재 오목판 상황을 보여주는 메서드
        /// </summary>
        /// <param name="_seat"> 만들어진 오목판의 시트 </param>
        public void ShowOmok()
        {
            Console.Clear();
            for (int y = 0; y < m_seat.GetLength(0); y++)
            {
                for (int x = 0; x < m_seat.GetLength(1); x++)
                {
                    if (CurX == x && CurY == y)
                        Console.Write("C");
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
                Console.WriteLine("흑돌 턴");
            else
                Console.WriteLine("백돌 턴");
        }

        /// <summary>
        /// 오목 조작 관련 메서드
        /// </summary>
        public void InputKey()
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
                case ConsoleKey.RightArrow:
                case ConsoleKey.D:
                    CurX++;
                    break;
                case ConsoleKey.LeftArrow:
                case ConsoleKey.A:
                    CurX--;
                    break;

                case ConsoleKey.Spacebar:
                    PutStone();
                    break;

                default:
                    break;
            }


        }


        /// <summary>
        /// 돌을 두는 메서드
        /// </summary>
        private void PutStone()
        {
            int x = m_curX;
            int y = m_curY;

            if (m_seat[y, x] == 0)
            {
                //if (m_isBlackTurn)
                //    m_seat[y, x] = 1;       // 행렬과 관련있음

                //else
                //    m_seat[y, x] = 2;
                m_seat[y, x] = m_isBlackTurn ? 1 : 2;   // 삼항
                if (!CheckFiveStone())
                    m_isBlackTurn = !m_isBlackTurn;

            }
        }

        /// <summary>
        /// 돌이 5개 연결 되는지 체크하는 함수
        /// </summary>
        /// <returns></returns>
        private bool CheckFiveStone()
        {
            int checkNum = m_isBlackTurn ? 1 : 2;
            int x = CurX;
            int y = CurY;

            // 가로 체크 계산
            for (int i = 0; i < 5; i++)
            {
                int count = 0;
                for (int j = 0; j < 5; j++)
                {
                    int temp = i + j - 4;
                    if (temp < 0 || temp >= m_size)
                        continue;

                    if (m_seat[y, x - temp] == checkNum)
                    {
                        count++;
                    }

                    if (count == 5)
                    {
                        IsPlaying = false;
                        return true;
                    }


                }
            }

            // 세로 체크 계산
            for (int i = 0; i < 5; i++)
            {
                int count = 0;
                for (int j = 0; j < 5; j++)
                {
                    int temp = i + j - 4;
                    if (y - temp < 0 || y - temp >= m_size &&
                        x - temp < 0 || x - temp >= m_size)
                        continue;

                    if (m_seat[y, x - temp] == checkNum)
                    {
                        count++;
                    }

                    if (count == 5)
                    {
                        IsPlaying = false;
                        return true;
                    }


                }
            }

            // 대각선 체크 계산
            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    
                }
            }

            //m_seat[y, x];   // 내가 방금 돌을 둔 좌표
            return true;
        }

        public bool GetIsBlackTurn()
        {
            return m_isBlackTurn;
        }
    }

    internal class Program
    {
        static void Main(string[] args)
        {
            Omok omok = new Omok(13);


            while (true)
            {
                omok.ShowOmok();
                omok.InputKey();
            }
            if (omok.GetIsBlackTurn())
                Console.WriteLine("흑돌 승리");
            else
                Console.WriteLine("백돌 승리");
        }



    }
}


// 오목을 만들것입니다.
// 이제 우리 뭐함?
// 1. 흑돌과 백돌이 필요하다.
// - 한번씩 번갈아가며 둬야한다.
// 2. 오목판 - (좌표) 
// - 완
// 3. 입력받기 - 인풋처리
// - 완
// 4. 내가 어디 둘것인지 대한 좌표 - 현재 나의 좌표
// - 완
// 5. 알고리즘
// - 많은 데이터를 저장하고 관리하는게 알고리즘
//  룰정리 - 문서로 표현할 줄 알아야한다.
// 가로 세로 오른쪽아래대각선 오른쪽위대각선
// 돌은 둔 순간 그 돌을 기준으로 5개가 연결되는지를 체크해야한다.