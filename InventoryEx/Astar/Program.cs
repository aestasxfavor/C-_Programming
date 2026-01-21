using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astar
{

    public class Node
    {
        // x, y 위치 좌표
        public int X { get; set; }
        public int Y { get; set; }
        // 벽인지 아닌지 - (움직일수 있는 곳인지 아닌지)
        public bool IsWall { get; set; }

        // 실제 이동한 거리 비용
        public float G { get; set; }
        // 예상 이동 거리
        public float H { get; set; }
        // 이동 거리 총 비용 값
        public float F => G + H;// G + H

        // 내가 이동하기 이전의 노드
        public Node ParentNode { get; set; }

        public Node(int _x, int _y, bool _isWall)
        {
            X = _x;
            Y = _y;
            IsWall = _isWall;
        }
    }


    // 맵에 대한 모든 정보를 가지고 있어야한다.
    public class Astar
    {
        private Node[,] m_grid;
        private int m_width;
        private int m_height;
        public Astar(int[,] _map)
        {
            m_width = _map.GetLength(1);
            m_height = _map.GetLength(0);
            m_grid = new Node[m_height, m_width];
            for (int y = 0; y < m_height; y++)
            {
                for (int x = 0; x < m_width; x++)
                {
                    //bool isWall = false;
                    //if (_map[y, x] == 1)
                    //    isWall = true;
                    bool isWall = _map[y, x] == 1 ? true : false;
                    m_grid[y, x] = new Node(x, y, isWall);
                }
            }
        }


        public List<Node> FindPath(int _startX, int _startY, int _goalX, int _goalY)
        {
            // 예외처리
            if (_startX < 0 || _startX >= m_width ||
                _startY < 0 || _startY >= m_height ||
                _goalX < 0 || _goalX >= m_width ||
                _goalY < 0 || _goalY >= m_height)
            {
                Console.WriteLine("스타트 좌표나 골 좌표가 영역을 벗어났습니다.");
                return null;
            }

            if (_startX == _goalX &&
                _startY == _goalY)
            {
                Console.WriteLine("스타트 좌표나 골 좌표 겹쳤습니다.");
                return null;
            }


            Node startNode = m_grid[_startY, _startX];
            Node goalNode = m_grid[_goalX, _goalY];

            if (startNode.IsWall || goalNode.IsWall)
            {
                Console.WriteLine("스타트 지점이나 골 지점이 벽입니다.");
                return null;
            }

            // 노드들 리셋
            ResetNodes();

            // 여기서부터 본격적으로 길찾기 시작하는겁니다.
            // 둘다 자료형들을 관리하는 Collections.Generic.
            // 어느 자료구조가 효율이 좋냐에 따라서 사용이 된다.

            // Dictionary 해쉬 값을 통해서 접근하는 친구 - 계산이 조금 더 들어가는 대신 빠르다. 검색이
            // 자료구조는 내가 그때 그때 필요한 즉, 그 상황에 연산이 빠른 자료구조를 사용하면 된다.

            // 내가 앞으로 갈수 있는 노드들을 정리해서 넣은것들.
            List<Node> openList = new List<Node>();

            // 내가 이미 갔던 곳 - 즉 다시 갈 필요가 없는 노드들.
            HashSet<Node> closeList = new HashSet<Node>();

            startNode.G = 0;
            startNode.H = GetH(startNode, goalNode);
            openList.Add(startNode);


            // 길 찾을때 까지 or 길이 없을때까지
            while (openList.Count > 0)
            {
                //Node curNode = openList
                //    .OrderBy((n) => { return n.F; })
                //    .ThenBy((n) => { return n.H})
                //    .First();

                // 가장 가까운 첫번째 노드 가져오기
                Node curNode = openList
                        .OrderBy(n => n.F)
                        .ThenBy(n => n.H)
                        .First();

                if (curNode == goalNode)
                {
                    // 길 찾기 완료.
                    // 리스트를 리턴해주면 된다.
                    return RetracePath(curNode);
                }

                openList.Remove(curNode);
                closeList.Add(curNode);

                // 자료구조에서 가장 자주 사용하는 반복문
                // for문은 index 위치를 체크할수있지만
                // 자료구조에 있는것들을 가져올뿐 index를 알수는 없다.

                // A스타 길찾기 알고리즘 핵심
                // curNode 기준으로 갈수있는 노드들의 정보와 필요한 데이터 값들을 적용.
                foreach (Node nearFriend in GetNearFriends(curNode))
                {
                    // 벽이면 넣을 필요 없고, 이미 갔던곳은 넣을 필요 없다.
                    if (nearFriend.IsWall == true || closeList.Contains(nearFriend))
                        continue;
                    float resultG = curNode.G + GetDistance(curNode, nearFriend);

                    // 새로운 길이 이동거리가 더 짧을 때 와
                    // 아예 완전 처음 보는 노드 일 때
                    if (resultG < nearFriend.G || !openList.Contains(nearFriend))
                    {
                        nearFriend.G = resultG;
                        nearFriend.H = GetH(nearFriend, goalNode);
                        nearFriend.ParentNode = curNode;
                        if (!openList.Contains(nearFriend))
                            openList.Add(nearFriend);
                    }
                }

            }

            // 다 확인했는데 도착할수있는 길이 없다.
            return null;
        }

        /// <summary>
        /// 간단하게 x,y 변화량 값
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        private float GetH(Node a, Node b)
        {
            return Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);
        }
        /// <summary>
        /// 대각선이 포함된 길이까지 한번에
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        private float GetDistance(Node a, Node b)
        {
            int dx = Math.Abs(a.X - b.X);
            int dy = Math.Abs(a.Y - b.Y);
            if (dx > dy)
                return 1.4f * dy + (dx - dy);
            else
                return 1.4f * dx + (dy - dx);
        }

        private void ResetNodes()
        {
            for (int y = 0; y < m_height; y++)
            {
                for (int x = 0; x < m_width; x++)
                {
                    m_grid[y, x].G = float.MaxValue;
                    m_grid[y, x].H = 0;
                    m_grid[y, x].ParentNode = null;
                }
            }
        }


        private List<Node> GetNearFriends(Node node)
        {
            List<Node> nearFriends = new List<Node>();

            for (int y = -1; y <= 1; y++)
            {
                for (int x = -1; x <= 1; x++)
                {
                    if (x == 0 && y == 0)
                        continue;

                    int checkX = node.X + x;
                    int checkY = node.Y + y;

                    if (checkX < 0 || checkX >= m_width ||
                        checkY < 0 || checkY >= m_height)
                        continue;

                    nearFriends.Add(m_grid[checkY, checkX]);
                }
            }

            return nearFriends;
        }


        private List<Node> RetracePath(Node _endNode)
        {
            List<Node> resultPath = new List<Node>();
            Node curNode = _endNode;

            while (curNode.ParentNode != null)
            {
                resultPath.Add(curNode);
                curNode = curNode.ParentNode;
            }

            return resultPath;
        }



    }



    public class Program
    {
        static void Main(string[] args)
        {
            // 시작 좌표들과 도착 좌표
            int startX = 1;
            int startY = 3;
            int goalX = 8;
            int goalY = 8;
            // 맵을 그려보세요.
            int[,] map = InitMap();
            ShowMap(map, startX, startY, goalX, goalY);


            Astar astar = new Astar(map);

            List<Node> path = astar.FindPath(startX, startY, goalX, goalY);

            for (int i = 0; i < path.Count; i++)
            {
                Console.WriteLine($"{path[i].X} {path[i].Y}   ");
            }


            if (path == null)
            {
                //길찾기 실패.

                return;
            }

            Console.WriteLine("길찾기 완료");

            ShowMap(map, startX, startY, goalX, goalY, path);


            Console.ReadLine();
        }


        // 10 x 10 
        static int[,] InitMap()
        {
            int[,] map = new int[,]
                {
                    { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
                    { 0, 0, 0, 0, 1, 0, 0, 0, 0, 0},
                    { 0, 0, 0, 0, 1, 0, 0, 0, 0, 0},
                    { 0, 0, 0, 0, 1, 0, 0, 0, 0, 0},
                    { 0, 0, 0, 0, 1, 0, 0, 0, 0, 0},
                    { 0, 0, 0, 0, 1, 0, 0, 0, 0, 0},
                    { 0, 0, 0, 0, 1, 0, 0, 0, 0, 0},
                    { 0, 0, 0, 0, 1, 0, 0, 0, 0, 0},
                    { 0, 0, 1, 0, 0, 0, 0, 0, 0, 0},
                    { 0, 0, 1, 1, 0, 0, 0, 0, 0, 0}
                };
            return map;
        }
        static void ShowMap(int[,] _map, int _startX, int _startY, int _goalX, int _goalY, List<Node> _path = null)
        {
            int width = _map.GetLength(1);
            int height = _map.GetLength(0);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    bool _bool = false;
                    if (_path != null)
                    {
                        for (int i = 0; i < _path.Count; i++)
                        {
                            if (_path[i].X == x && _path[i].Y == y)
                            {
                                Console.Write("O ");
                                _bool = true;
                            }
                        }
                    }
                    if (_bool)
                        continue;

                    if (_startX == x && _startY == y)
                        Console.Write("S");
                    else if (_goalX == x && _goalY == y)
                        Console.Write("G");
                    else if (_map[y, x] == 0)
                        Console.Write("'");
                    else if (_map[y, x] == 1)
                        Console.Write("X");
                    Console.Write(" ");
                }
                Console.WriteLine();
            }
        }

    }
}
