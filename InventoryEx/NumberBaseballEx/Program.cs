using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/*
 * 숫자 야구 게임
 * 1. 컴퓨터 숫자 리스트 완성하기
 * 2. 입력 3개 받기
 * 3. 스트라이크 볼 판정하기 (if문 써서 비교하기)
 * 4. Console.Write로 출력하기
 * 지금 빠진거
 * 머잇지... 주말동안 다시 해봐야겠다 
*/
namespace NumberBaseballEx
{

    internal class Program
    {
        static void ShowStartScreen()
        {
            Console.WriteLine("야구게임에 오신것을 환영합니다.");
            Console.WriteLine("※룰 : 중복되지 않은 숫자 3개를 입력해주세요.");
        }

        static int[] GetNumber()
        {
            Random rand = new Random();
            int[] arrayRandNum = new int[10];
            for (int i = 0; i < 10; i++)
                arrayRandNum[i] = i;
            for (int i = 0; i < 3; i++)
            {
                int result = rand.Next(i, 10);
                int temp = arrayRandNum[i];
                arrayRandNum[i] = arrayRandNum[result];
                arrayRandNum[result] = temp;
            }
            return arrayRandNum;
        }

        static bool IsNumberCheck()
        {
            string str = Console.ReadLine();
            //문자열 길이가 3개인지 체크
            if (str.Length != 3)
            {
                Console.WriteLine("입력하신 문자 길이가 3개가 아닙니다.");
            }
            else
            {
                bool _isNum = true;
                bool _isDuplication = true;
                for (int i = 0; i < 3; i++)
                {
                    //받은 문자열들이 전부 숫자인지 체크
                    char temp = str[i];
                    if (48 <= temp && temp <= 57)
                    {
                        //이 숫자들이 중복이 안되는지 체크
                        for (int a = 0; a < str.Length; a++)
                        {
                            for (int b = a + 1; b < str.Length; b++)
                            {
                                if (str[a] == str[b])
                                {
                                    _isDuplication = false;
                                }
                                else
                                {

                                }
                            }
                        }
                    }
                    else
                    {
                        _isNum = false;
                    }
                }

                if (_isNum)
                {
                    if (_isDuplication)
                    {
                        Console.WriteLine("입력받은게 모두 정상적인 숫자입니다.");
                        return true;
                    }
                    else
                    {
                        Console.WriteLine("중복된 숫자가 있습니다.");
                    }
                }
                else
                {
                    Console.WriteLine("입력받은 것 중에 문자가 있습니다.");
                }
            }

            return false;
        }

        static void Main(string[] args)
        {
            ShowStartScreen();

            //컴퓨터가 기억하는 우리가 맞춰야 하는 숫자.
            int[] arrayNum = new int[3];
            int[] arrayResult = GetNumber();
            int[] arrayCorrectNum = new int[3];

            for (int i = 0; i < arrayNum.Length; i++)
                arrayNum[i] = arrayResult[i];

            for (int i = 0; i < arrayCorrectNum.Length; i++)
            {
                arrayCorrectNum[i] = arrayResult[i];
            }


            //while (true)
            //{
            //    string str = Console.ReadLine();
               
            //    if (IsNumberCheck(str))
            //    {
            //        int[] arrayEnterNum = new int[3];
            //        for (int i = 0; i < arrayEnterNum.Length; i++)
            //        {
            //            arrayEnterNum[i] = Convert.ToInt32(str[i]);
            //        }

            //        int strike = 0;
            //        int ball = 0;

            //        int[] arrayCorrectNum = new int[3];
            //        for (int i = 0; i < arrayCorrectNum.Length; i++)
            //        {

            //            for (int j = 0; j < arrayEnterNum.Length; j++)
            //            {
            //                if (arrayCorrectNum[i] == arrayEnterNum[j])
            //                {
            //                    if (i == j)
            //                    {
            //                        strike++;
            //                    }
            //                    else
            //                    {
            //                        ball++;
            //                    }
            //                }
            //            }
            //        }

            //        Console.WriteLine(strike + " S");
            //        Console.WriteLine(ball + " B");
            //        Console.WriteLine();
            //    }
            //    else
            //    {

            //    }
              
                
                
                

            //}


        }
       
    }

    
}
