using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestProject
{

    public class Player
    {
        public string Name { get; set; }
        public int HP { get; set; }
        public int ATK { get; set; }


    }

    enum State
    {
        Idle, Attack
    }

    static State FightPoint()
    {
        ConsoleKeyInfo keyInfo = Console.ReadKey();
        switch (keyInfo.Key)
        {
            case ConsoleKey.D1:

                
        }

    }
    internal class Program
    {
        static void Main(string[] args)
        {
            Player player = new Player();

            player.Name = "player01";
            player.HP = 50;
            player.ATK = 10;

            while (true)
            {
                // 나중에 while로 감싸야할듯 

                Console.WriteLine("스테이터스 창");
                Console.WriteLine($"name : {player.Name}");
                Console.WriteLine($"hp : {player.HP}");
                Console.WriteLine($"atk : {player.ATK}");

                Console.WriteLine("1. 몬스터와 싸운다.");
                Console.WriteLine("2. 대기한다.");

                ConsoleKeyInfo keyInfo = Console.ReadKey();

                switch (keyInfo.Key)
                {
                    case ConsoleKey.D1:
                        Console.WriteLine(" 몬스터와 싸운다.");
                         Console.ReadKey();
                        break;

                    case ConsoleKey.D2:
                        Console.WriteLine(" 대기한다.");
                        Console.ReadKey();

                        break;
                    default:
                        Console.WriteLine(" 잘못된 입력이다");
                         Console.ReadKey();

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

// 번외 (추가부분)
// 위 과정이 되면 클래스로 만든다.
// 싸운다 선택 이후 플레이어는 상단, 중단, 하단을 공격할 수 있다.
// 몬스터는 이 세 부위 상단, 중단, 하단을 랜덤하게 막고
// 막으면 공격은 무효 못막으면 데미지가 들어온다.
// 몬스터가 죽을때까지
// ============================ 위 과정이 2단계


// + 추가
// 턴제 방식으로 플레이어 공격시 그 다음은 몬스터가 공격한다.
// 플레이어는 상단, 중단, 하단의 공격을 막아야한다.
// 플레이어나 몬스터 둘중 하나가 죽을때까지 
// ============================ 위 과정이 3단계

// 시험 시간 : 2시간 30분 
// 나는 길어야 한시간이겠다 



// Random이랑 switch if 등 진짜 기본적인걸로 시험보네 이러면 벼락치기 가능
