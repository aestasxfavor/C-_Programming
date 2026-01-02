using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DelegateEx // 대리자 
{
    public abstract class Unit
    {
        public abstract void TestFunc();

    }

    public class Monster : Unit
    {
        public override void TestFunc()
        {

        }
    }

    public class Item
    {
        private string m_name;
        private int m_price;
        public int Price { get { return m_price; } }
        private int m_atk;
        public int Attack { get { return m_atk; } }
        private int m_range;

        public float Testsum => m_atk + m_range;

        public delegate void ItemDelegate();
        public void TestDele(ItemDelegate dele)
        {
            dele();
            
        }

        // 딜리게이트를 통해 만들어진 것들
        // return을 하지 않는다 = Action
        public Action<int, int> TestAction;

        // return을 한다 = Func
        public Func<int> TestFunc;

        public void TestConsoleWrite()
        {
            Console.WriteLine("그냥 함수");
        }

        public delegate void Button();

        public void OnButton(Button button)
        {
            button();
        }

    }
    internal class Program
    {
        static void Main(string[] args)
        {
            List<Item> list = new List<Item>();

            list.Add(new Item());

            list[0].TestDele(() => { Console.WriteLine("delegate"); });    // 람다식 표현

            list[0].TestConsoleWrite();
            list[0].TestAction = (a, b) => { Console.WriteLine($"{a} {b} = {a + b}"); };

            list[0].TestAction.Invoke(1, 5);


            list.Sort((a, b) => { return a.Price.CompareTo(b.Price); });
        }
    }
}
