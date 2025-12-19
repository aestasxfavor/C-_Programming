using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StringEx
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // 문자열 오름차순 내림차순
            char c = '3';   // 문자
            string str = "abcdef";   // 문자열
            str = str.Replace('c', 'a');        // replace를 쓰면 지정한 문자를 다른걸로 바꿔주는거구나
            string temp = str.Substring(1, 5);
            temp = temp.Replace('a','c');

            string result = str[0] + temp;
            // "a" + "bcdef"
           
            c = (char)97;


            Console.WriteLine(str);
            Console.ReadLine();
        }
    }
}
