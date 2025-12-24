using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

enum Target
{
    Top, Bottom, Middle
}

namespace FightSystem
{
    public abstract class Character
    {
        public string Name { get; set; }
        public int HP { get; set; }
        public int ATK { get; set; }

        public abstract void CharacterInfo();

        public abstract void Init();
        public abstract void TakeDamage(int damage);

    }

    public class Unit : Character
    {
        public override void CharacterInfo()
        {
            Name = "player01";
            HP = 50;
            ATK = 10;
        }

        public override void Init()
        {
            Name = "player01";
            HP = 50;
            ATK = 10;
        }

        public override void TakeDamage(int damage)
        {
            HP -= damage;
            if (HP <= 0)
                Console.WriteLine("플레이어가 사망했습니다. 초기화면으로 되돌아갑니다.");

        }
    }

    public class Orc : Character
    {
        public override void CharacterInfo()
        {
            Name = "Orc";
            HP = 40;
            ATK = 5;
        }

        public override void Init()
        {
            Name = "Orc";
            HP = 40;
            ATK = 5;
        }

        public override void TakeDamage(int damage)
        {
            if (HP <= 0)
                Console.WriteLine("오크가 죽었습니다 당신이 이겼습니다.");
        }
    }
    internal class Program
    {

        static void ShowInfo(Unit p, Orc o)
        {
            Console.WriteLine("===========================");
            Console.WriteLine($"이름 : {p.Name}");
            Console.WriteLine($"체력 : {p.HP}");
            Console.WriteLine($"공격력 : {p.ATK}");
            Console.WriteLine();
            Console.WriteLine($"이름 : {o.Name}");
            Console.WriteLine($"체력 : {o.HP}");
            Console.WriteLine($"공격력 : {o.ATK}");
            Console.WriteLine("===========================");
        }
        static Target UnitAttack()
        {
            Console.WriteLine("어디를 공격할까?");
            Console.WriteLine("1. 상단");
            Console.WriteLine("2. 중단");
            Console.WriteLine("3. 하단");

            ConsoleKeyInfo keyInfo = Console.ReadKey();

            switch (keyInfo.Key)
            {
                case ConsoleKey.D1: return Target.Top;
                case ConsoleKey.D2: return Target.Middle;
                case ConsoleKey.D3: return Target.Bottom;
                default: return Target.Top;

            }

        }

        static Target UnitDef()
        {
            Console.WriteLine("어디를 막을까?");
            Console.WriteLine("1. 상단");
            Console.WriteLine("2. 중단");
            Console.WriteLine("3. 하단");

            ConsoleKeyInfo keyInfo = Console.ReadKey();

            switch (keyInfo.Key)
            {
                case ConsoleKey.D1: return Target.Top;
                case ConsoleKey.D2: return Target.Middle;
                case ConsoleKey.D3: return Target.Bottom;
                default: return Target.Top;

            }

        }

        static Target RandomTarget(Random rand)
        {
            return (Target)rand.Next(0, 3);
        }

        //static Target Battle(Unit p, Orc o)
        //{
        //    Random random = new Random();

        //    while(p.HP > 0 && o.HP > 0)
        //    {
        //        Console.Clear();
        //        ShowInfo(p, o);

        //        Console.WriteLine("행동을 선택하세요.");
        //        Console.WriteLine("1. 공격");
        //        Console.WriteLine("2. 대기");

        //        ConsoleKeyInfo keyInfo = Console.ReadKey();
        //        Console.Clear();

        //        bool targetAttacked = false;
        //        Target pAttack = Target.Top; 

               
                
        //    }
        //        Console.WriteLine($"플레이어가 피해를 입었습니다. player: HP {p.HP}");
        //        Console.WriteLine($"오크가 피해를 입었습니다. orc: HP {o.HP}");
        //}
        
        static void Main(string[] args)
        {
            Unit player = new Unit();
            player.CharacterInfo();

            Orc orc = new Orc();
            orc.CharacterInfo();

            while (true)
            {
                Target p = UnitAttack();
                Target o;
                
            }
        }
    }
}
