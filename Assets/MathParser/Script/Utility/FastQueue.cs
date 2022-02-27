//From https://github.com/MrOkiDoki/Reliable-And-Fast-Queue-C-Sharp

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MathExpParser.Utility
{
    class FastQueue<T>
    {
        T[] queue;
        int index = 0;
        public FastQueue()
        {
            queue = new T[4];
        }
        /// <summary>
        /// Buffer Count
        /// </summary>
        /// <param name="count"></param>
        public FastQueue(int count)
        {
            if (count < 0)
                throw new Exception();
            queue = new T[count];
        }

        void Check()
        {
            if (queue.Length == index)
                System.Array.Resize(ref queue, queue.Length + 128);
        }

        public int Count { get { return this.index; } }
        public int BufferSize { get { return this.queue.Length; } }

        public void Enqueue(T item)
        {
            Check();
            queue[index] = item;
            index++;
        }
        public T Dequeue()
        {
            if (index == 0)
                return (T)(object)(null);
            index--;
            return queue[index];
        }


        public void Clear()
        {
            index = 0;
        }
    }
}
