using System;
using System.Collections.Generic;
using System.Text;

namespace bill_ai
{
    class SortedPointList
    {
        Dictionary<Point, int> itemValues;
        List<Point> orderedItems;

        public SortedPointList()
        {
            itemValues = new Dictionary<Point, int>();
            orderedItems = new List<Point>();
        }

        public int Count
        {
            get
            {
                return orderedItems.Count;
            }
        }

        public void Add(Point p, int fScore)
        {
            if (itemValues.ContainsKey(p))
            {
                if (fScore < itemValues[p])
                {
                    itemValues[p] = fScore;
                }
            }
            else
            {
                int index = -1;
                for (int i = 0; i < orderedItems.Count; i++)
                {
                    if (fScore < itemValues[orderedItems[i]])
                    {
                        index = i;
                        break;
                    }
                }

                if (index >= 0)
                    orderedItems.Insert(index, p);
                else
                    orderedItems.Add(p);
                itemValues.Add(p, fScore);
            }
        }

        public Point this[int i]
        {
            get
            {
                return orderedItems[i];
            }
            set
            {
                orderedItems[i] = value;
            }
        }

        public List<Point> Keys
        {
            get
            {
                return orderedItems;
            }
        }

        public int GetValue(int i)
        {
            return itemValues[orderedItems[i]];
        }

        /// <summary>
        /// removes and returns the first item
        /// </summary>
        /// <returns></returns>
        public Point Pop()
        {
            Point result = orderedItems[0];
            orderedItems.Remove(result);
            itemValues.Remove(result);
            return result;
        }

        public void Remove(Point p)
        {
            itemValues.Remove(p);
            orderedItems.Remove(p);
        }

        public bool ContainsPoint(Point p)
        {
            return itemValues.ContainsKey(p);
        }
    }
}
