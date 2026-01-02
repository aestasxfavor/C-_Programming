using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


/*
 * 2025-12-31
 * 
 * 정적 변수
 * static int A;
 * 
 * 멤버 변수
 * int m_A;     m_를 붙이는 것이 좋음 (현역에서는 저렇게 많이 쓰는듯)
 * 
 * TryParse : 자료형을 파싱(바꿔주는?) 함수
 * 문자열 파싱 문제풀때 저거 써도 되려나
 * 
 */
namespace Practice
{

    public class Program
    {
        static void Main()
        {
            //Building();
            //Triangle();
            //ReverseTriangle();
        }

        static void Building()
        {
            // 내가 입력한 숫자의 크기 만큼 건물 모양 나오게 하기
            // 대신 가로길이도 조절 가능하게 하기
            // 두번 입력으로 
            // ㅁㅁㅁㅁㅁㅁㅁㅁ
            // ㅁㅁㅁㅁㅁㅁㅁㅁ
            // ㅁㅁㅁㅁㅁㅁㅁㅁ
            // ㅁㅁㅁㅁㅁㅁㅁㅁ
            // ㅁㅁㅁㅁㅁㅁㅁㅁ
            // ㅁㅁㅁㅁㅁㅁㅁㅁ
            // ㅁㅁㅁㅁㅁㅁㅁㅁ
            // ㅁㅁㅁㅁㅁㅁㅁㅁ
            #region 한 번 입력으로 빌딩 만들기
            //string str = Console.ReadLine();
            //string str_width = "";
            //string str_height = "";

            //str.Split(' ');
            //int index = 0;

            //for (int y = 0; y < str.Length; y++)
            //{
            //    if (str[y] == ' ')
            //    {
            //        index = y;
            //        break;
            //    }
            //}

            //for (int y = 0; y < index; y++)
            //    str_width += str[y];


            //for (int y = index; y < str.Length; y++)
            //    str_height += str[y];

            //string[] str =Console.ReadLine().Split(' ');

            //int width = 0;
            //int height = 0;

            //if (int.TryParse(str[0], out width) == false ||
            //    int.TryParse(str[1], out height) == false)
            //    Console.WriteLine("파싱 실패");
            //for (int y = 0; y < height; y++)
            //{
            //    for (int x = 0;  x < width;  x++)
            //    {
            //        Console.Write("□");
            //    }
            //    Console.WriteLine();
            //}
            #endregion

            #region 두번 입력해서 빌딩 만들기
            // 배열 쓰는 거 같음
            // 이중 for문도 써야할거같음
            //
            //Console.Write("가로 길이를 입력하세요 : ");
            //string str = Console.ReadLine();
            //int width = 0;
            //if (int.TryParse(str, out width) == false)
            //    Console.WriteLine("파싱에 실패했다.");
            //
            //Console.Write("세로 길이를 입력하세요 : ");
            //str = Console.ReadLine();
            //int height = 0;

            //// 파싱이 되면 true, 그렇지 않으면 false를 반환한다.
            //if (int.TryParse(str, out height) == false)
            //    Console.WriteLine("파싱에 실패했다.");
            //
            //// 코드는 취향차이지만 가독성도 그렇고 이게 보기 좋은듯
            //str = "";
            //for (int x = 0; x < width; x++)
            //    str += "□";
            //for (int y = 0; y < height; y++)
            //    Console.WriteLine(str);
            //
            //
            //for (int y = 0; y < height; y++)
            //{
            //    for (int x = 0; x < width; x++)
            //    {
            //        Console.Write("□");     // string
            //    }
            //    Console.WriteLine(str);
            //}
            #endregion

            string[] str = Console.ReadLine().Split(' ');

            int height = 0;
            int width = 0;

            if (int.TryParse(str[0], out height) == false ||
                int.TryParse(str[1], out width) == false)
                Console.WriteLine("파싱에 실패함");

            for (int y = 0; y < width; y++)
            {
                for (int x = 0; x < height; x++)
                {
                    Console.Write("□ ");
                }
                Console.WriteLine();
            }
            Console.ReadKey();

        }

        #region 피라미드 만들기
        static void Triangle(string _str)
        {
            // 피라미드 만들기? 
            // 난 왜 한 기억이 없냐 뭐지

            //string[] str = Console.ReadLine().Split(' ');
            Console.Write("피라미드의 세로 길이 : ");
            int height = int.Parse(Console.ReadLine());     // 방어코드가 없기 때문에 좋지않은 코드
            Console.Write("피라미드의 가로 길이 : ");
            int width = int.Parse(Console.ReadLine());

            string strTriangle = "";
            string strSpace = "";

            int index = 0;
            while (index < height)
            {
                strSpace = "";
                for (int i = index; i < height - 1; i++)
                {
                    for (int j = 0; j < width; j++)
                        strSpace += " ";
                }
                if (index == 0)
                    strTriangle = "△";
                else
                    for (int i = 0; i < width; i++)
                    {
                        strTriangle += "△";
                    }
                Console.WriteLine(strSpace + strTriangle);
                index++;
            }

            Console.ReadKey();
        }
        #endregion

        static void ReverseTriangle(string _str_1, string _str_2)
        {
            // 실제로는 이런 방식은 쓰면 안된다.
            //int height = int.Parse(Console.ReadLine());
            int height = 0;
            if (!int.TryParse(_str_1, out height))
                Console.WriteLine("파싱 실패");

            int width = 0;
            if (!int.TryParse(_str_2, out width))
                Console.WriteLine("파싱 실패");

            int spaceConut = 0;
            for (int i = height - 1; i >= 0; i--)
            {
                for (int j = 0; j < spaceConut; j++)
                    Console.Write(" ");
                for (int j = 0; j <= i; j++)
                    Console.Write("▽");
                Console.WriteLine();
                spaceConut++;
            }

        }
    }
}
