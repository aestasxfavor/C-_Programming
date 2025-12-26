using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StructureExLinkedList
{
    internal class MyLinkedList
    {
        public MyNode head;
        public int count = 0;

        public void AddLast(int _data)
        {
            if(head == null) head = new MyNode(_data);
            count++;
        }
    }
}
