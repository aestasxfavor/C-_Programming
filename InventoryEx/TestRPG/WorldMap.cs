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

        public StageMap()
        {
        }

        public StageMap(int[,] _seat)
        {
            m_seat = new int[_seat.GetLength(0), _seat.GetLength(1)];
            for (int y = 0; y < _seat.GetLength(0); y++)
                for (int x = 0; x < _seat.GetLength(1); x++)
                    m_seat[y, x] = _seat[y, x];
        }

        public int GetSeatLength(int _dimension)
        {
            return m_seat.GetLength(_dimension);
        }

        public int GetSeatInfo(int _y, int _x)
        {
            return m_seat[_y, _x];
        }

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

        private Map m_curMap;
        private StageMap m_curStageMap;

        public WorldMap(Map _map = Map.Start)
        {
            InitMap(_map);
        }

        public void InitMap(Map _map)
        {
            StageMap startMap = new StageMap(new int[,]
                 {
                    { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
                    { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
                    { 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0},
                    { 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0},
                    { 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0},
                    { 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0},
                    { 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0},
                    { 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0},
                    { 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0},
                    { 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0},
                    { 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0},
                    { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
                    { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
                    { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
                    { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
                    { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
                 });
            StageMap townMap = new StageMap(new int[,]
                {
                {0, 0, 0, 0, 0, 0, 0, 0 },
                {0, 0, 0, 0, 0, 0, 0, 0 },
                {0, 0, 0, 0, 0, 0, 0, 0 },
                {0, 0, 0, 0, 0, 0, 0, 0 },
                {0, 0, 0, 0, 0, 0, 0, 0 },
                {0, 0, 0, 0, 0, 0, 0, 0 },
                {0, 0, 0, 0, 0, 0, 0, 0 },
                {0, 0, 0, 0, 0, 0, 0, 0 },
                });
            StageMap riverMap = new StageMap(new int[,]
                {
                {0, 0, 0, 1, 1, 0, 0, 0 },
                {0, 0, 0, 1, 1, 0, 0, 0 },
                {0, 0, 0, 1, 1, 0, 0, 0 },
                {0, 0, 0, 1, 1, 0, 0, 0 },
                {0, 0, 0, 1, 1, 0, 0, 0 },
                {0, 0, 0, 1, 1, 0, 0, 0 },
                {0, 0, 0, 1, 1, 0, 0, 0 },
                {0, 0, 0, 1, 1, 0, 0, 0 },
                });

            m_dicMap.Add(Map.Start, startMap);
            m_dicMap.Add(Map.Town, townMap);
            m_dicMap.Add(Map.River, riverMap);


            SetCurMap(_map);
        }
        private void SetCurMap(Map _map)
        {
            if (m_dicMap.TryGetValue(_map, out m_curStageMap))
            {
                m_curMap = _map;
            }
            else
            {
                Console.WriteLine("처음 초기화 하는 부분에서 맵을 가져오다가 실패했습니다.");
            }
        }


        public int[,] GetMap(Map _map)
        {
            StageMap getMap;
            if (m_dicMap.TryGetValue(_map, out getMap))
                return getMap.m_seat;

            // 맵을 제대로 전달하지 못했을 때 null로 반환
            return null;
        }

        public void ShowScreen(Character _player)
        {
            for (int y = 0; y < m_curStageMap.GetSeatLength(1); y++)
            {
                for (int x = 0; x < m_curStageMap.GetSeatLength(0); x++)
                {
                    // 플레이어의 위치를 표시
                    if (_player.CurX == x && _player.CurY == y)
                        Console.Write("P");

                    // 움직일수 있는 좌표를 표시
                    else if (m_curStageMap.GetSeatInfo(y, x) == 0)
                        Console.Write("'");

                    // 움직일수 없는 벽을 좌표로 표시
                    else if (m_curStageMap.GetSeatInfo(y, x) == 1)
                        Console.Write("+");

                    Console.Write(" ");
                }
                Console.WriteLine();
            }
        }
        public int GetCurmapSize(int dimension)
        {
            return m_curStageMap.GetSeatLength(dimension);
        }

        public StageMap GetStageMap()
        {
            return m_curStageMap;
        }
    }
}
