using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestRPG
{
    public enum Map
    {
        Start, Town, River
    }

    public class StageMap
    {
        public int[,] m_seat;
    }
    public class WorldMap
    {
        // 이거 리스트 2차원 배열로 가능할거 같은데 
        //List<List<int>> m_map = new List<List<int>>();
        //
        //public void Map(List<List<int>> map)
        //{
        //
        //}

        Dictionary<Map, StageMap> m_dicMap = new Dictionary<Map, StageMap>();

        private int[,] m_startMap;
        private int[,] m_townMap;
        private int[,] m_riverMap;

        public WorldMap()
        {
            InitMap();
        }

        public void InitMap()
        {
            StageMap startMap = new StageMap();
            startMap.m_seat = new int[,]
            {
                {0,0,0,0,0,0,0,0,0,0,0,0 },
                {0,0,0,0,0,0,0,0,0,0,0,0 },
                {0,0,0,0,1,0,0,0,0,0,0,0 },
                {0,0,0,0,1,0,0,0,0,0,0,0 },
                {0,0,0,0,1,0,0,0,0,0,0,0 },
                {0,0,0,0,1,0,0,0,0,0,0,0 },
                {0,0,0,0,1,0,0,0,0,0,0,0 },
                {0,0,0,0,1,1,1,1,1,1,0,0 },
                {0,0,0,0,0,0,0,0,0,0,0,0 },
                {0,0,0,0,0,0,0,0,0,0,0,0 },
            };

            StageMap townMap = new StageMap();
            townMap.m_seat = new int[,]
            {
                {0,0,0,0,0,0,0,0 },
                {0,0,0,0,0,0,0,0 },
                {0,0,0,0,0,0,0,0 },
                {0,0,0,0,0,0,0,0 },
                {0,0,0,0,0,0,0,0 },
                {0,0,0,0,0,0,0,0 },
                {0,0,0,0,0,0,0,0 },
                {0,0,0,0,0,0,0,0 },
            };

            StageMap riverMap = new StageMap();
            riverMap.m_seat = new int[,]
            {
                 {0,0,0,0,0 },
                {0,0,0,0,0 },
                {0,0,0,0,0 },
                {0,0,0,0,0 },
                {0,0,0,0,0 },
            };

            m_dicMap.Add(Map.Start, startMap);
            m_dicMap.Add(Map.Town, townMap);
            m_dicMap.Add(Map.River, riverMap);

        }

        public int[,] GetMap(Map _map)
        {
            StageMap getMap;
            if (m_dicMap.TryGetValue(_map, out getMap))
                return getMap.m_seat;

            // 맵을 제대로 전달하지 못했을 때 null로 반환
            return null;
        }

        public void ShowScreen(Map _map)
        {
            int[,] curMap = GetMap(_map);


            for (int y = 0; y < curMap.GetLength(0); y++)
            {
                for (int x = 0; x < curMap.GetLength(1); x++)
                {
                    if (curMap[y, x] == 0)
                        Console.Write("□");
                    else if (curMap[y, x] == 1)
                        Console.Write("■");

                    Console.Write(" ");
                }
                Console.WriteLine();
            }
        }
    }
}
