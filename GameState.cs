using System;
using System.Collections.Generic;

namespace bill_ai
{
    public class GameState
    {
        internal bool[,] board;
        internal Point me;
        internal Point enemy;

        // used for 
        internal bool isForMe;

        public GameState(Point m, Point e, bool fm, bool[,] b)
        {
            board = (bool[,])b.Clone();
            me = m;
            enemy = e;
            isForMe = fm;
        }
    }
}
