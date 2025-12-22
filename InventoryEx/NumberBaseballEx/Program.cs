using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

/*
 * 숫자 야구 게임 만들기
 * 1. 컴퓨터 숫자 리스트 완성하기 -> 컴퓨터가 숫자 3개 정하기
 * 2. 컴퓨터 숫자 3개 정하기
 * 3. 플레이어가 뭐든 입력이 가능하도록
 * 3-1. 문자열의 길이가 3인지
 * 3-2. 입력된 문자들이 숫자인지 
 * 4. 내가 올바르게 입력한 숫자가 컴퓨터가 처음 정한 숫자가 맞는지 비교학
 * 4-1. 숫자와 위치가 같은 경우 : S
 * 4-2. 숫자만 같은 경우 : B
 * 5. 몇번만에 맞췄는지에 대한 결과와 끝난다는 결과창이 나와야한다.
 * 5-1. 게임이 꺼지거나 다시 숫자를 집어넣어서(초기화) 다시 게임이 진행되야한다.
 * --------------
 * 룰
 * 플레이어는 중복되지 않는 숫자 3개를 입력한다.
 * 중복되는 숫자거나 숫자가 3개가 아니라면 아니라고 알려준다.
 * 숫자 3개 입력시 비교를 하며 스트라이크와 볼을 체크해준다.
 * 
*/
namespace NumberBaseballEx
{

    internal class Program
    {

        static void ShowStartScreen()
        {
            Console.WriteLine("숫자 야구게임에 오신 것을 환영");
            Console.WriteLine("중복되지 않는 숫자 3개 입력해라.");
        }

        static int[] GetRandom()
        {
            Random rand = new Random();
            int[] arrRand = new int[10];
            for (int i = 0; i < arrRand.Length; i++)
            {
                arrRand[i] = i;
            }

            for (int i = 0; i < arrRand.Length; i++)
            {
                int randResult = rand.Next(i, 10);
                // Swap하기 
                int temp = arrRand[i];
                arrRand[i] = arrRand[randResult];
                arrRand[randResult] = temp;
            }

            int[] arrResult = new int[3];
            for (int i = 0; i < arrResult.Length; i++)
            {
                arrResult[i] = arrRand[i];
            }

            return arrResult;


        }

        static bool GetCheckNumber(string str)
        {
            bool isLength = true;
            bool isNumber = true;
            bool isOverlap = true;

            if (str.Length != 3)
            {
                // char tempC = '0'; 일때 int로 바꾸면 48
                // char tempC = '9'; 일때 int로 바꾸면 57

                for (int i = 0; i < str.Length; i++)
                {
                    char tempC = str[i];
                    if (48 >= tempC || tempC >= 57)
                        isNumber = false;
                }

                // 숫자들이 중복되지 않는지 검사 
                if (isLength && isNumber)
                {
                    for (int i = 0; i < str.Length; i++)
                    {
                        for (int j = i + 1; j < str.Length; j++)
                        {
                            if (str[i] == str[j])
                            {
                                isOverlap = false;

                            }
                        }
                    }

                }

                if (!isOverlap)
                    Console.WriteLine("중복되는 숫자있음");


            }

            if (!isNumber)
            {
                Console.WriteLine("문자가 섞여있음");
            }

            if (isLength && isNumber && isOverlap)
                return true;
            else
                return false;

        }
        static void Main()
        {
            ShowStartScreen();
            int[] arrayCorrect = GetRandom();
            int inputResult = 0;        // 입력한 값

            while (true)
            {
                string inputStr = Console.ReadLine();
                bool _bool = GetCheckNumber(inputStr);


                if (_bool)
                {
                    inputResult++;
                    int strike = 0;
                    int ball = 0;

                    for (int i = 0; i < inputStr.Length; i++)
                    {
                        for (int j = 0; j < inputStr.Length; j++)
                        {
                            int tempNum = inputStr[i] - 48;
                            if (tempNum == arrayCorrect[j])
                            {
                                if (i == j)
                                {
                                    strike++;
                                }
                                else
                                {
                                    ball++;
                                }
                            }
                        }
                    }

                    Console.WriteLine($"Strike : {strike} ");
                    Console.WriteLine($"Ball : {ball} ");
                    if (strike == 3)
                    {
                        Console.WriteLine("정답");
                        Console.WriteLine($"시도한 횟수 : {inputResult}");

                        Console.ReadKey();

                        Console.Clear();
                        ShowStartScreen();

                        inputResult = 0;
                    }
                }


            }
        }

    }


}
