using System;

enum Target
{
    Top,
    Middle,
    Bottom
}

namespace TestProject
{
    public abstract class Unit
    {
        public string Name { get; set; }
        public int HP { get; set; }
        public int ATK { get; set; }

        public abstract void Init();
        public abstract void TakeDamage(int dmg);
    }

    public class Player : Unit
    {
        public override void Init()
        {
            Name = "Player01";
            HP = 50;
            ATK = 10;
        }

        public override void TakeDamage(int dmg)
        {
            HP -= dmg;
            Console.WriteLine($"플레이어가 {dmg} 데미지 받음 (HP: {HP})");
        }
    }

    public class Orc : Unit
    {
        public override void Init()
        {
            Name = "Orc";
            HP = 40;
            ATK = 5;
        }

        public override void TakeDamage(int dmg)
        {
            HP -= dmg;
            Console.WriteLine($"오크가 {dmg} 데미지 받음 (HP: {HP})");
        }
    }

    internal class Program
    {
        static void ShowInfo(Player p, Orc o)
        {
            Console.WriteLine("========================");
            Console.WriteLine($"플레이어 이름: {p.Name}");
            Console.WriteLine($"플레이어 HP: {p.HP}");
            Console.WriteLine($"플레이어 ATK: {p.ATK}");
            Console.WriteLine();
            Console.WriteLine($"오크 이름: {o.Name}");
            Console.WriteLine($"오크 HP: {o.HP}");
            Console.WriteLine($"오크 ATK: {o.ATK}");
            Console.WriteLine("========================\n");
        }

        static Target GetPlayerTarget()
        {
            Console.WriteLine("어디를 공격할까?");
            Console.WriteLine("1. 상단");
            Console.WriteLine("2. 중단");
            Console.WriteLine("3. 하단");

            switch (Console.ReadKey().Key)
            {
                case ConsoleKey.D1: return Target.Top;
                case ConsoleKey.D2: return Target.Middle;
                case ConsoleKey.D3: return Target.Bottom;
                default: return Target.Top;
            }
        }

        static Target GetPlayerDef()
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

        // 턴제 전투
        static void Battle(Player player, Orc orc)
        {
            Random rand = new Random();

            while (player.HP > 0 && orc.HP > 0)
            {
                Console.Clear();
                ShowInfo(player, orc);

                // 1단계 요구사항: 공격/대기 
                Console.WriteLine("행동을 선택하세요");
                Console.WriteLine("1. 공격한다");
                Console.WriteLine("2. 대기한다");

                ConsoleKeyInfo key = Console.ReadKey();
                Console.Clear();

                bool playerAttacked = false;
                Target pAttack = Target.Top;

                if (key.Key == ConsoleKey.D1)
                {
                    // ---------------------------
                    // 1. 플레이어 공격 (상중하)
                    // ---------------------------
                    pAttack = GetPlayerTarget();
                    Console.Clear();

                    Target oDef = RandomTarget(rand);

                    Console.WriteLine($"플레이어 공격: {pAttack}");
                    Console.WriteLine($"오크 방어: {oDef}");

                    if (pAttack == oDef)
                    {
                        Console.WriteLine("오크가 막았다.");
                    }
                    else
                    {
                        orc.TakeDamage(player.ATK);
                    }

                    playerAttacked = true;

                    // 몬스터 죽으면 종료
                    if (orc.HP <= 0)
                    {
                        Console.WriteLine("\n오크를 처치했습니다 당신이 이겼습니다.");
                        Console.ReadKey();
                        Environment.Exit(0);
                        //return;
                    }
                }
                else if (key.Key == ConsoleKey.D2)
                {
                    Console.WriteLine("플레이어는 대기했습니다.");
                }
                else
                {
                    Console.WriteLine("잘못된 입력, 대기한다.");
                }

                Console.ReadKey();
                Console.Clear();


                
                
                ShowInfo(player, orc);

                Target orcAttack = RandomTarget(rand);

                Target playerDef = GetPlayerDef();
                Console.Clear();

                Console.WriteLine($"오크 공격: {orcAttack}");
                Console.WriteLine($"플레이어 방어: {playerDef}");

                if (orcAttack == playerDef)
                {
                    Console.WriteLine("플레이어가 공격을 막았다");
                }
                else
                {
                    player.TakeDamage(orc.ATK);
                }
                

                // 플레이어 죽으면 종료 → 메인으로
                if (player.HP <= 0)
                {
                    Console.WriteLine("\n플레이어 사망 스타트로 돌아갑니다.");
                    Console.ReadKey();
                    player.Init();
                    orc.Init();
                    return;
                }

                Console.ReadKey();
            }
        }


        static void Main()
        {
            Player player = new Player();
            player.Init();

            Orc orc = new Orc();
            orc.Init();

            while (true)
            {
                Console.Clear();
                ShowInfo(player, orc);

                Console.WriteLine("1. 싸운다");
                Console.WriteLine("2. 대기한다");

                ConsoleKeyInfo keyInfo = Console.ReadKey();

                Console.Clear();

                if (keyInfo.Key == ConsoleKey.D1)
                {
                    Battle(player, orc);

                    if (orc.HP <= 0)
                    {
                        orc.Init(); // 새로운 몬스터 생성
                    }
                }
                else if (keyInfo.Key == ConsoleKey.D2)
                {
                    Console.WriteLine("대기합니다.");
                    Console.ReadKey();
                }
                else
                {
                    Console.WriteLine("잘못된 입력");
                    Console.ReadKey();
                }
            }
        }
    }
}
