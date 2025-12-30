using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmokEx
{
    public class Omok
    {
        private int size;
        private int[,] seat;

        private int curX;
        private int curY;

        public int CurX
        {
            get { return curX; }
            set { curX = value; }
        }

        private bool isBlackTurn;
    }

    internal class Program
    {
        static void Main(string[] args)
        {
        }
    }
}
