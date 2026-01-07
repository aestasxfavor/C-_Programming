using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

///
/// 계산기 만들기
/// 1-1 정수 2개를 입력하여 덧셈 출력하기
///

namespace Practice_1
{
    public class Calculator
    {
        private float a;
        private float b;
        private float sum;

        public void Additon()
        {
            ConsoleKeyInfo keyInfo = Console.ReadKey();

            while(true)
            {

                Console.WriteLine($"첫번째 입력 값 : {a}");

                Console.WriteLine($"두번째 입력 값 : {b}");

                sum = a + b;
                Console.WriteLine($"더한 값 : {sum}");
            }
        }

        public void Subtraction()
        {

        }

        public void Multiplication()
        {
        }

        public void Division()
        {
        }
    }
    internal class Program
    {
        static void Main(string[] args)
        {
            Calculator calculator = new Calculator();
            calculator.Additon();

            string strA = Console.ReadLine();
            string strB = Console.ReadLine();

            int a = int.Parse(strA);
            int b = int.Parse(strB);

            Console.WriteLine($"덧셈: {a + b}");
            Console.WriteLine($"뺄셈: {a - b}");
            Console.WriteLine($"곱셈: {a * b}");
            Console.WriteLine($"나눗셈: {a / b}");

        }
    }
}
