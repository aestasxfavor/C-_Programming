using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// 기초 알고리즘 
// 제일 큰애, 제일 작은애, 같은애 위치, 배열값 복사, 배열 뒤집기
// 0~9까지 숫자 랜덤하게 배열에 들어가야 함.
// 숫자 오름차순, 내림차순 (주말에 다시 해보기 ㅆㅂ 할 수 있음)
// swap

// Swap 공식
//int a = 10;
//int b = 20;

//int temp = a;
//a = b;
//b = temp;

namespace PracticeEx
{
    internal class Program
    {
        class Monster
        {
            int hp;
            public int HP { get; set; }
        }

        static void Main(string[] args)
        {
            // 두 배열 합치기 
            int[] arrayA = new int[10];
            int[] arrayB = new int[10];

            int[] arrayR = new int[arrayA.Length + arrayB.Length];

            for (int i = 0; i < 10; i++)
            {
                //arrayA[i] += (char)(97 + i);      // 이게 머임 아 a~z까지 차례대로? 이게 문자열 파싱인가? 
                arrayA[i] = i;
                arrayB[i] = i + 10;
                // 어떻게 합칠것인가? 
                // 배열을 합친다... 주소.. 포인터? 근데 배열자체가 주소를 갖고있잖아

            }

            for (int i = 0; i < arrayA.Length; i++)
            {
                arrayR[i] = arrayA[i];
            }

            for (int i = 0; i < arrayB.Length; i++)
            {
                arrayR[i + arrayA.Length] = arrayB[i];     // 배열 합치는 공식 
            }

            for (int i = 0; i < arrayR.Length; i++)
            {
                Console.Write(arrayR[i] + " ");
            }
            Console.WriteLine();

            // 배열 하나를 2개로 나누기

            for (int i = 0; i < arrayR.Length; i++)
            {
                if (i < arrayA.Length)
                {
                    arrayA[i] = arrayR[i];
                }
                else
                {
                    arrayB[i - arrayA.Length] = arrayR[i];
                }
                Console.Write(arrayR[i] + " ");
            }

            Console.ReadKey();

            Random rand = new Random();
            for (int i = 0; i < 10; i++)
            {
                int result = rand.Next(0, 10);
                int temp = arrayA[0];
                arrayA[0] = arrayA[result];
                arrayA[result] = temp;
                //int value = rand.Next(0, 10);
                //char temp = str[0];
                //strB += str[value];
                //str.Replace("a", "c");
            }
            string str = "";
            for (int i = 0; i < arrayA.Length; i++)
            {
                str += arrayA[i];

            }
            Console.WriteLine(str);
            Console.WriteLine();


        }
    }
}
