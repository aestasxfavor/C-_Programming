using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StructureEx
{
    internal class MyLinkedList <T>
    {
        public MyNode head;
        public int count = 0;

        public void AddLast(int _data)
        {
            if (head == null) head = new MyNode(_data);

            MyNode currentNode = head;

            while(true)
            {
                if (currentNode.next == null)
                {
                    currentNode.next = new MyNode(_data);
                    break;
                }
                else
                    currentNode = currentNode.next;

            }
            count++;
        }

        public void Search(int _index = 10)
        {
            MyNode currentNode = head;
            for (int i = 0; i < _index; i++)
            {
                currentNode = currentNode.next;
            }
            
        }
    }
}
