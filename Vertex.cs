using System;
using System.Collections.Generic;

namespace bill_ai
{
    class Vertex
    {
        public Point Self;
        public int Num;
        public int Low;
        public bool Visited;
        public Vertex Parent;

        public Vertex(Point p)
        {
            Self = p;
            Num = Low = 0;
            Visited = false;
            Parent = null;
        }
    }
}
