using System.Collections.Generic;
using System;

namespace bill_ai
{
    public class SurvivalNode
    {
        internal Point self;
        internal SurvivalNode parent;
        internal List<SurvivalNode> children;
        private int value;
        internal int realValue;
        private List<Point> changedNodes;

        internal List<Point> ChangedNodes
        {
            get
            {
                if (changedNodes.Count == 0)
                {
                    SurvivalNode temp = this;
                    while (temp.parent != null)
                    {
                        changedNodes.Add(temp.self);
                        temp = temp.parent;
                    }
                }

                return changedNodes;
            }
        }

        // real value is the biggest value of its child + 1
        public int CalcRealValue(DateTime turnStart)
        {
            if ((DateTime.Now - turnStart).TotalMilliseconds > MyTronBot.timeOut)
                return 0;

            int largestChildValue = -1;
            foreach (SurvivalNode child in children)
            {
                int cVal = child.CalcRealValue(turnStart) + 1;
                if (cVal > largestChildValue)
                    largestChildValue = cVal;
            }

            realValue = (largestChildValue == -1 ? CalcValueForSurvival() : largestChildValue);
            return realValue;
        }

        public SurvivalNode(Point pos, SurvivalNode pa)
        {
            self = pos;
            parent = pa;
            children = new List<SurvivalNode>(4);
            value = -1;
            changedNodes = new List<Point>(10);
            
            if (pa != null)
                pa.children.Add(this);
        }

        public int CalcValueForSurvival()
        {
            if (value == -1)
            {
                bool[,] tempBoard = (bool[,])Map.walls.Clone();

                // initiate board
                foreach (Point diff in ChangedNodes)
                {
                    tempBoard[diff.X, diff.Y] = true;
                }

                value = Utility.GetOpenSpotForSelf(self, tempBoard).Count;
            }
            return value;
        }

        public List<Point> GetValidMoves()
        {
            // initiate board
            foreach (Point diff in ChangedNodes)
            {
                Map.walls[diff.X, diff.Y] = true;
            }

            List<Point> result = Utility.GetValidMoves(self, Map.walls);

            // restore board
            foreach (Point diff in ChangedNodes)
            {
                Map.walls[diff.X, diff.Y] = false;
            }

            return result;
        }
    }
}
