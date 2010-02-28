using System;
using System.Collections.Generic;
using System.Text;

namespace bill_ai
{
    public class IndexedPointHash
    {
        int[] slotValues; // hashcode => value
        List<int> indexedSlot; // index => hashcode
        const int arraySize = 5050;

        public IndexedPointHash()
        {
            slotValues = new int[arraySize];
            indexedSlot = new List<int>();
        }

        public void Add(Point p, int value)
        {
            if (slotValues[p.GetHashCode()] == 0)
            {
                slotValues[p.GetHashCode()] = value;
                indexedSlot.Add(p.GetHashCode());
            }
        }

        public int Count
        {
            get
            {
                return indexedSlot.Count;
            }
        }

        public bool Contains(Point p)
        {
            if (slotValues[p.GetHashCode()] == 0)
                return false;
            else
                return true;
        }

        public bool Contains(int x, int y)
        {
            if (slotValues[x * 100 + y] == 0)
                return false;
            else
                return true;
        }

        public int this[Point p]
        {
            get
            {
                return slotValues[p.GetHashCode()];
            }
        }

        public int this[int i]
        {
            get
            {
                return slotValues[indexedSlot[i]];
            }
        }

        public Point GetPointAt(int i)
        {
            int slotVal = indexedSlot[i];
            return new Point(slotVal / 100, slotVal % 100);
        }

        public void Remove(Point p)
        {
            slotValues[p.GetHashCode()] = 0;
            indexedSlot.Remove(p.GetHashCode());
        }

        public IEnumerable<Point> GetAllPoints()
        {
            foreach (int slotVal in indexedSlot)
            {
                yield return new Point(slotVal / 100, slotVal % 100);
            }
        }

        public void Merge(IndexedPointHash right)
        {
            for (int i = 0; i < arraySize; i++)
            {
                if (right.slotValues[i] != 0)
                {
                    slotValues[i] = right.slotValues[i];
                    indexedSlot.Add(i);
                }
                    
            }
        }
    }
}
