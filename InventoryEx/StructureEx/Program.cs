using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StructureEx
{
    internal class Program
    {
        // 근본은 배열이다.

        /*
         * Add : 추가
         * Insert : 원하는 위치에 추가
         * Remove : 지울 때 사용
         * RemoveAt : 몇번 째 인덱스에 있는 자료형을 지움
         * Clear : 비우기 
         * Sort : 내부 데이터를 정렬
        */
        public class MyList<T>
        {
            public T[] arr;
            private int count;
            public int Count { get { return count; } }
            private int capacity;
            public int Capacity { get { return capacity; } }
            public MyList()
            {
                arr = new T[4];
                count = 0;
                capacity = arr.Length;
            }

            public void Add(T _data)
            {
                // count가 capacity보다 크면
                if (count >= capacity)      // = 붙여주기
                    Resize();   // 사이즈 늘려주기

                arr[count] = _data;
                count++;

            }

            private void Resize()
            {
                // arr = new T[count]; <- 이건 새로운 배열을 만드는 것이기 때문에
                // 기존 배열에 담겨있던 값이 전부 날아간다.
                T[] tempArr = new T[count * 2];

                for (int i = 0; i < count; i++)
                {
                    tempArr[i] = arr[i];
                }

                arr = tempArr;
                capacity = arr.Length;
            }

            public void Insert(int _index, T _data)
            {
                arr[_index] = _data;

                for (int i = count; i > _index; i--)
                {
                    arr[i] = arr[i - 1];
                }

            }

            public void Remove(T _data)
            {
                for (int i = 0; i < count; i++)
                {
                    if (_data.Equals(arr[i]))
                    {
                        for (int j = i; j < count - 1; j++)
                        {
                            arr[j] = arr[j + 1];
                            count--;
                            break;
                        }
                    }
                }
            }

            public void RemoveAt(int _index)
            {
                for (int i = _index; i < count; i++)
                {
                    arr[i] = arr[i + 1];
                }
                    count--;
            }

            public void RemoveAll(T _data)
            {
                // 전체 삭제
                for (int i = 0; i < count; i++)
                {
                    for (int j = i; j < count; j++)
                    {
                        arr[j] = arr[i];
                        break;
                    }
                }
            }

            public void Sort(int _index, int _count, T _data)
            {
                // Swap해야할거같은데 
                if (_index < 0)
                {

                }


            }

            public void Clear()
            {

            }
        }
        static void Main(string[] args)
        {
            List<int> list = new List<int>();

            for (int i = 0; i < 10; i++)
                list.Add(i);

            list.Insert(5, -20);
            list.Remove(3);
            list.RemoveAt(4);
            list.Sort();
            
            for (int i = 0; i < list.Count; i++)
            {
                Console.Write(list[i] + " ");
            }

            Console.WriteLine();
            Console.WriteLine($"list.Count : {list.Count} list.Capacity : {list.Capacity}");

            MyList<int> myList = new MyList<int>();

            for (int i = 0; i < 10; i++)
                myList.Add(i);

            myList.Insert(5, -20);
            myList.Remove(3);
            myList.RemoveAt(4);

            for (int i = 0; i < myList.Count; i++)
                Console.Write(myList.arr[i] + " ");

            Console.WriteLine();
            Console.WriteLine($"list.Count : {myList.Count} list.Capacity : {myList.Capacity}");


            LinkedList<int> linkedList = new LinkedList<int>();

            MyLinkedList<int> myLinkedList = new MyLinkedList<int>();
        }

    }
}
