using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Practice2
{
    public static class Extenstion
    {
        public static void YoMan(this int a)
        {

        }
    }
    public class PPP
    {

    }

    // delegate: 대리자? 
    public class TestDelegate
    {
        public delegate void MyDelegate(int a, int b);

        // 매개변수가 하나고 값을 반환하지 않음
        public Action<int> TestAction;

        // 매개변수가 하나고 지정된 값 반환
        public Func<int, int> TestFunc;

        public void Test()

        {
            List<NumCheck> listTest = new List<NumCheck>();
            listTest.Add(new NumCheck(100, 100));
            listTest.Add(new NumCheck(100, 200));
            listTest.Add(new NumCheck(100, 300));

            listTest.Sort((a, b) => { return a.Gold.CompareTo(b.Gold); });
            listTest.OrderBy(a => a.Gold).ThenBy(a => a.Silver).First();

            int kkk = 0;

            kkk.YoMan();
        }
    }

    public class NumCheck
    {
        public int Gold { get; set; }
        public int Silver { get; set; }

        public NumCheck(int _gold, int _silver)
        {
            Gold = _gold;
            Silver = _silver;
        }
    }

    internal class Program
    {
        static void Main(string[] args)
        {
            // 정수들 저장하기 10개
            int[] arr1 = new int[] { 1, 2, 3 };
            int[] arr2 = new int[10];
            int[] arr3 = { 1, 2, 3, 4, 5 };

            long[] arr4 = new long[10];

            // 정수들을 저장한다 100개
            int[] arrNum = new int[100];
            for (int i = 0; i < arrNum.Length; i++)
            {
                arrNum[i] = i * 10;
            }

            //arrNum = new int[200]; // 이렇게 하면 기존에 100개 저장한 건 날라감

            // 배열인데 동시에 확장된 기능
            List<int> list = new List<int>();
            int capacity = list.Capacity;
            for (int i = 0; i < 100; i++)
            {
                list.Add(i * 10);
            }

            int[,] map = new int[,]
            {
                {1, 2, 3, },
                {4, 5, 6, },
                {7, 8, 9, },
            };

            // var는 웬만해선 쓰지 않는걸 비추한다. 오른쪽에 있는걸 보고 유추해야함

            list.Remove(555);   // 555의 값ㅇ르 가지고 있는 녀석을 제거
            list.RemoveAt(555); // 555번째 index를 제거
            list.Clear();       // 모든 요소 비우기

            list.Contains(55);
            list.Sort();
            list.Reverse();


            Dictionary<string, int> dict = new Dictionary<string, int>();
            // 딕셔너리 쓰는 이유: 검색이 빠름

            dict.Add("Apple", 12);
            dict.Add("Banana", 23);
            dict.Add("Carrot", 31);

            if (dict.ContainsKey("Carrot"))
            {
                Console.WriteLine(dict["Carrot"]);
            }

            int temp = 0;
            if (dict.TryGetValue("Carrot", out temp))
            {
                Console.WriteLine(temp);
            }
            else
            {
                Console.WriteLine("존재하지 않음");
            }

            //foreach(KeyValuePair<string, int> fruit in dict)
            //{
            //    fruit.Value;    // string
            //    fruit.Key;      // int
            //}


            foreach (int value in list)
            {
                Console.WriteLine(value);
            }

            for (int i = 0; i < list.Count; i++)
            {
                Console.WriteLine(list[i]);
            }

            // 둘다 같은 반복문이다.




        }
    }
}
