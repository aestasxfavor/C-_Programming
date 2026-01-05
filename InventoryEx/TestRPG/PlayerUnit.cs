using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestRPG
{
    public class PlayerUnit : Character, IMoveController
    {
        private int[,] dirs = new int[,]
        {
            {0,-1 },    // 위
            {0,1 },     // 아래
            {-1,0 },    // 왼쪽
            {1,0 }      // 오른쪽
        };

        public void InputMove(ConsoleKeyInfo _keyinfo, StageMap _stageMap)
        {
            int dir = 0;
            switch (_keyinfo.Key)
            {
                case ConsoleKey.UpArrow:
                case ConsoleKey.W: dir = 0; break;

                case ConsoleKey.DownArrow:
                case ConsoleKey.S: dir = 1; break;

                case ConsoleKey.LeftArrow:
                case ConsoleKey.A: dir = 2; break;

                case ConsoleKey.RightArrow:
                case ConsoleKey.D: dir = 3; break;

                default: return;
            }

            MoveFunc(dirs[dir, 0], dirs[dir, 1], _stageMap);

        }

        public void MoveFunc(int _dtX, int _dtY, StageMap _stageMap)
        {
            CurX += _dtX;
            CurY += _dtY;

            int sizeX = _stageMap.GeSeatLength(1);
            int sizeY = _stageMap.GetSeatLength(0);

            if (CurX < 0)
                CurX = 0;
            else if (CurX >= sizeX)
                CurX = sizeX - 1;

            if (CurY < 0)
                CurY = 0;
            else if(CurY >= sizeY)
                CurY = sizeY - 1;

            if(_stageMap.GetSeatInfo(CurX, CurY) ==  null)
            {
                CurX = _dtX;
                CurY = _dtY;
            }
        }

        public override void ShowStatus()
        {
            throw new NotImplementedException();
        }
    }
}
