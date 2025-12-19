using System.Threading.Tasks;

namespace ListEx
{

    public abstract class Character
    {
        protected int atk = 3;

        public Character()
        {
            Console.WriteLine("1");
        }

        public Character(int a)
        {
            Console.WriteLine("2");
            atk = a;
        }

        public abstract void Attack();      // 자식에서 맘대로 사용하라는 의미 


        public virtual int GetAtk()
        {
            return atk;
        }

        public void Damage()
        {
            int _atk = GetAtk();
            Console.WriteLine(_atk);
        }
    }

    public class Monster : Character
    {
        public override void Attack()
        {
            throw new NotImplementedException();
        }
    }

    public class Player : Character
    {
        public Player(int a) : base(a)
        {

            Console.WriteLine("11");
        }

        public override int GetAtk()
        {
            //this.GetAtk();      // 상속받은 내 함수
            base.GetAtk();      // 기본 함수 
            return atk * 2;
        }

        public override void Attack()
        {
            throw new NotImplementedException();
        }
    }

    public class Inven<T>
    {
        public T data;
    }


    public class Program
    {
        static void Main(string[] args)
        {
            // Dictionary는 반복문 사용을 할 수 없음
            Dictionary<string, int> players = new Dictionary<string, int>();
            players.Add("Knight", 1);
            players.Add("King", 2);

            // Queue와 Stack : 둘다 선입선출
            int a = 0;
            int b = 3;
            Console.WriteLine(b);
            Console.WriteLine(a);
            int value = 0;
            string tempkey = "Knight";
          
            // LinkedList

            if (players.ContainsKey("Knight") == true)
            {
                value = players[tempkey];
            }
            else
            {
                Console.WriteLine("존재하지 않는 아이템");
            }
               

            Inven<int> invenA = new Inven<int>();
            invenA.data = 0;

            Inven<string> stringA = new Inven<string>();
            stringA.data = "";

            List<int> ints = new List<int>();
            for (int i = 0; i < 10; i++)
            {
                ints.Add(i);
                Console.WriteLine("Count : " + ints.Count);
                Console.WriteLine("Capacity : " + ints.Capacity);
            }
            LinkedList<int> list = new LinkedList<int>();
            Dictionary<string, int> dict = new Dictionary<string, int>();
            Stack<int> stack = new Stack<int>();
            Queue<int> queue = new Queue<int>();
            HashSet<int> visited = new HashSet<int>();
        }
    }
}