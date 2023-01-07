using System;
using System.Collections.Generic;

namespace Unity.Services.Core.Scheduler.Internal
{
    abstract class MinimumBinaryHeap
    {
        internal const float IncreaseFactor = 1.5f;
        internal const float DecreaseFactor = 2.0f;
    }

    class MinimumBinaryHeap<T> : MinimumBinaryHeap
    {
        readonly IComparer<T> m_Comparer;
        readonly int m_MinimumCapacity;

        T[] m_HeapArray;

        internal IReadOnlyList<T> HeapArray => m_HeapArray;

        public int Count { get; private set; }

        public T Min => m_HeapArray[0];

        public MinimumBinaryHeap(int minimumCapacity = 10)
            : this(Comparer<T>.Default, minimumCapacity) {}

        public MinimumBinaryHeap(IComparer<T> comparer, int minimumCapacity = 10)
            : this(null, comparer, minimumCapacity) {}

        internal MinimumBinaryHeap(ICollection<T> collection, IComparer<T> comparer, int minimumCapacity = 10)
        {
            if (minimumCapacity <= 0)
            {
                throw new ArgumentException("capacity must be more than 0");
            }

            m_MinimumCapacity = minimumCapacity;
            m_Comparer = comparer;

            Count = collection?.Count ?? 0;
            var startSize = Math.Max(Count, minimumCapacity);
            m_HeapArray = new T[startSize];
            if (collection is null)
                return;

            // Reset count since we insert all items.
            Count = 0;
            foreach (var item in collection)
            {
                Insert(item);
            }
        }

        public void Insert(T data)
        {
            IncreaseHeapCapacityWhenFull();
            var dataPos = Count;
            m_HeapArray[Count] = data;
            Count++;
            while (dataPos != 0
                   && m_Comparer.Compare(m_HeapArray[dataPos], m_HeapArray[Parent(dataPos)]) < 0)
            {
                Swap(ref m_HeapArray[dataPos], ref m_HeapArray[Parent(dataPos)]);
                dataPos = Parent(dataPos);
            }
        }

        void IncreaseHeapCapacityWhenFull()
        {
            if (Count != m_HeapArray.Length)
            {
                return;
            }

            var newCapacity = (int)Math.Ceiling(Count * IncreaseFactor);
            var newHeapArray = new T[newCapacity];
            Array.Copy(m_HeapArray, newHeapArray, Count);
            m_HeapArray = newHeapArray;
        }

        public void Remove(T data)
        {
            var key = GetKey(data);
            if (key < 0)
                return;

            while (key != 0)
            {
                Swap(ref m_HeapArray[key], ref m_HeapArray[Parent(key)]);
                key = Parent(key);
            }

            ExtractMin();
        }

        public T ExtractMin()
        {
            if (Count <= 0)
            {
                throw new InvalidOperationException("Can not ExtractMin: BinaryHeap is empty.");
            }

            var data = m_HeapArray[0];

            if (Count == 1)
            {
                Count--;
                m_HeapArray[0] = default;
                return data;
            }

            Count--;
            m_HeapArray[0] = m_HeapArray[Count];
            m_HeapArray[Count] = default;
            MinHeapify();
            DecreaseHeapCapacityWhenSpare();
            return data;
        }

        void DecreaseHeapCapacityWhenSpare()
        {
            var spareThreshold = (int)Math.Ceiling(m_HeapArray.Length / DecreaseFactor);
            if (Count <= m_MinimumCapacity
                || Count > spareThreshold)
            {
                return;
            }

            var newHeapArray = new T[Count];
            Array.Copy(m_HeapArray, newHeapArray, Count);
            m_HeapArray = newHeapArray;
        }

        int GetKey(T data)
        {
            var key = -1;
            for (var i = 0; i < Count; i++)
            {
                if (m_HeapArray[i].Equals(data))
                {
                    key = i;
                    break;
                }
            }

            return key;
        }

        void MinHeapify()
        {
            int key;
            var smallest = 0;
            do
            {
                key = smallest;
                UpdateSmallestKey();
                if (smallest == key)
                {
                    return;
                }

                Swap(ref m_HeapArray[key], ref m_HeapArray[smallest]);
            }
            while (smallest != key);

            void UpdateSmallestKey()
            {
                smallest = key;
                var leftKey = LeftChild(key);
                var rightKey = RightChild(key);

                UpdateSmallestIfCandidateIsSmaller(leftKey);
                UpdateSmallestIfCandidateIsSmaller(rightKey);
            }

            void UpdateSmallestIfCandidateIsSmaller(int candidate)
            {
                if (candidate >= Count
                    || m_Comparer.Compare(m_HeapArray[candidate], m_HeapArray[smallest]) >= 0)
                {
                    return;
                }

                smallest = candidate;
            }
        }

        static void Swap(ref T lhs, ref T rhs) => (lhs, rhs) = (rhs, lhs);

        static int Parent(int key) => (key - 1) / 2;

        static int LeftChild(int key) => 2 * key + 1;

        static int RightChild(int key) => 2 * key + 2;
    }
}
