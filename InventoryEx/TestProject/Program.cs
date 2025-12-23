using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
/// <summary>
/// 전체적으로 리팩토링만 하면 될듯
/// </summary>
enum State
{
    Idle, Attack
}

enum Target
{
    Top, Bottom, Middle
}

namespace TestProject
{
    public abstract class Character
    {
        public string Name { get; set; }
        public int HP { get; set; }
        public int ATK { get; set; }

        public int Damage { get; set; }

        bool IsDead { get; set; }   // 얘도 3단계 턴제까지 가야한다면 그때 사용하기

        public abstract void CharacterInfo();
        public abstract void TakeDamage();
    }

    public class Player : Character
    {
        public override void CharacterInfo()
        {
            Name = "player01";
            HP = 50;
            ATK = 10;
        }

        public override void TakeDamage()       // 3단계 턴제까지 간다면 쓸 함수
        {
            HP -= Damage;
            if (HP <= 0)
            {
                Console.WriteLine("플레이어가 사망하였습니다.");
                Console.WriteLine("졌습니다.");
            }
        }
    };

    public class Monster : Character
    {
        public override void CharacterInfo()
        {
            Name = "monster";
            HP = 40;
            ATK = 5;
        }

        public override void TakeDamage()
        {
            HP -= Damage;
            if (HP <= 0)
            {
                Console.WriteLine("몬스터가 죽었습니다.");
                Console.WriteLine("당신이 이겼습니다.");
            }
        }
    }

    internal class Program
    {
        static void ShowInfo(Player player, Monster monster)
        {
            Console.WriteLine("스테이터스 창");
            Console.WriteLine();
            Console.WriteLine($"name : {player.Name}");
            Console.WriteLine($"hp : {player.HP}");
            Console.WriteLine($"atk : {player.ATK}");

            Console.WriteLine();
            Console.WriteLine($"name : {monster.Name}");
            Console.WriteLine($"hp : {monster.HP}");
            Console.WriteLine($"atk : {monster.ATK}");

            Console.WriteLine();
        }

      
        static Target AttackToMonster()
        {
            Console.Clear();
            Console.WriteLine("어디를 공격할까?");
            Console.WriteLine("1. 상단");
            Console.WriteLine("2. 중단");
            Console.WriteLine("3. 하단");
            

            ConsoleKeyInfo keyInfo = Console.ReadKey();

            switch (keyInfo.Key)
            {
                case ConsoleKey.D1:
                    return Target.Top;

                case ConsoleKey.D2:
                    return Target.Middle;

                case ConsoleKey.D3:
                    return Target.Bottom;

                default:
                    Console.WriteLine("잘못된 입력");
                    return Target.Top;   
            }
        }

        static Target MonsterDef(Random rand)
        {
            int value = rand.Next(0, 3);
            return (Target)value;
        }

        /// <summary>
        /// 2단계 과정 여기를 반복해야할듯 
        /// </summary>
        static State Battle(Player player, Monster monster)
        {
            Random rand = new Random();

            // 1. 플레이어 공격 선택
            Target playerAttack = AttackToMonster();

            // 2. 몬스터 랜덤 방어
            Target monsterDef = MonsterDef(rand);

            Console.Clear();

            Console.WriteLine($"플레이어 공격: {playerAttack}");
            Console.WriteLine($"몬스터 방어: {monsterDef}");

            // 3. 맞았는지 판정
            if (playerAttack == monsterDef)
            {
                Console.WriteLine("몬스터가 공격을 막았다. 데미지 없음");
                Console.ReadLine();
            }
            else
            {
                monster.HP -= player.ATK;
                Console.WriteLine($"{player.ATK} 데미지 들어감. (몬스터 HP: {monster.HP})");
                Console.ReadLine();
                

            }

            // 4. 몬스터 죽음 체크
            if (monster.HP <= 0)
            {
                Console.WriteLine("몬스터가 죽었습니다. 당신의 승리!");
                Environment.Exit(0);
            }

            return State.Attack;
        }

        static void Main(string[] args)
        {
            Player player = new Player();
            player.CharacterInfo();

            Monster monster = new Monster();
            monster.CharacterInfo();

            while (true)
            {
                ShowInfo(player, monster);

                
                Console.WriteLine("1. 몬스터와 싸운다.");
                Console.WriteLine("2. 대기한다.");

                ConsoleKeyInfo keyInfo = Console.ReadKey();
            

                switch (keyInfo.Key)
                {
                    case ConsoleKey.D1:
                        Console.WriteLine("몬스터와 싸운다.");
                        Battle(player, monster);
                        Console.Clear();
                        break;

                    case ConsoleKey.D2:
                        Console.WriteLine("대기한다.");
                        Console.Clear();
                        break;

                    default:
                        Console.WriteLine("잘못된 입력이다");
                        Console.Clear();
                        break;
                }
            }
        }
    }
}


// 화면에 플레이어와 플레이어 이름, hp, atk이 떠야함
// 수요일에 할 테스트 과제 
// 스테이터스 창
// 화면 - 플레이어 정보
// name :
// hp :
// atk :

// 입력받을 수 있어야한다.
// 1. 몬스터와 싸운다. -> 몬스터와 싸운다.
// 2. 대기한다. -> 대기한다.
// 3. 다른 경우 -> 잘못된 입력입니다.

// 1. 무조건 화면에 띄운다.
// 2. 입력을 받아서 결과창이 나와야한다.
// 3. 반복되어야한다.
// ============================ 위 과정이 1단계

// 추가부분
// 위 과정이 되면 클래스로 만든다.
// 싸운다 선택 이후 플레이어는 상단, 중단, 하단을 공격할 수 있다.
// 몬스터는 이 세 부위 상단, 중단, 하단을 랜덤하게 막고
// 막으면 공격은 무효, 못막으면 데미지가 들어온다.
// 몬스터가 죽을때까지
// ============================ 위 과정이 2단계


// 추추가
// 턴제 방식으로 플레이어 공격시 그 다음은 몬스터가 공격한다.
// 플레이어는 상단, 중단, 하단의 공격을 막아야한다.
// 플레이어나 몬스터 둘중 하나가 죽을때까지 
// ============================ 위 과정이 3단계
















// 추추추가 : 상속도 써라? Character만들어서 Player랑 Monster상속해야되나보네 반복문 쓰라고? 
// 매서드(함수로 최대한 빼기)
// 시험 시간 : 2시간 30분 
