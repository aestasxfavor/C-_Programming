using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
/*
 * 1. 맵 만들기
 * 2. 캐릭터 클래스
 * 3. 인벤토리
 * 4. 아이템 만들기
 */

namespace TestRPG
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Warrior player = new Warrior("player01", 100, 20, 1, 30, 15);
            WorldMap map = new WorldMap();

            while (true)
            {
                map.ShowScreen(Map.Start);
                player.ShowStatus();

                Console.ReadLine();
            }
        }
    }
}
