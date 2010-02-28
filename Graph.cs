using System;
using System.Collections.Generic;

namespace bill_ai
{
    class Graph
    {
        int counter;
        Vertex rootVertex;
        bool[,] board;
        Dictionary<Point, Vertex> allVertex;
        List<IndexedPointHash> zones;
        /// <summary>
        /// Articulation Points
        /// </summary>
        public List<Point> ArticulationPoints;

        /// <summary>
        /// initialize graph from board, heavy function
        /// </summary>
        /// <param name="start">
        /// starting point, must be a valid point
        /// </param>
        /// <param name="board">the board</param>
        public Graph(Point p, bool[,] b)
        {
            counter = 1;
            rootVertex = new Vertex(p);
            board = b;
            allVertex = new Dictionary<Point, Vertex>();
            allVertex.Add(p, rootVertex);
            zones = new List<IndexedPointHash>();
            ArticulationPoints = new List<Point>();
        }
        /*
        public void AssignNum(Vertex v)
        {
            v.Num = counter++;
            v.Visited = true;
            foreach (Vertex w in v.Neighbours)
            {
                if (!w.Visited)
                {
                    w.Parent = v;
                    AssignNum(w);
                }
            }
        }

        public void AssignLow(Vertex v)
        {
            v.Low = v.Num;
            foreach (Vertex w in v.Neighbours)
            {
                if (w.Num > v.Num) // Forward Edge
                {
                    AssignLow(w);
                    if (w.Low >= v.Num)
                        ArticulationPoints.Add(v.Self);
                    v.Low = Math.Min(v.Low, w.Low); // Rule 3
                }
                else if (v.Parent != w) // Back Edge
                {
                    v.Low = Math.Min(v.Low, w.Num); // Rule 2
                }
            }
        }
        */

        private List<Vertex> GetNeighbours(Point p)
        {
            List<Vertex> result = new List<Vertex>();
            foreach (Point p1 in Utility.GetValidMoves(p, board))
            {
                if (!allVertex.ContainsKey(p1))
                    allVertex.Add(p1, new Vertex(p1));
                result.Add(allVertex[p1]);
            }

            return result;
        }

        public void FindArtPoints()
        {
            Vertex v = rootVertex;
            int childCount = 0;

            v.Visited = true;
            v.Low = v.Num = counter++; // Rule 1
            foreach (Vertex w in GetNeighbours(v.Self))
            {
                if (!w.Visited) // Forward Edge
                {
                    w.Parent = v;
                    FindArtPoints(w);
                    v.Low = Math.Min(v.Low, w.Low); // Rule 3
                    childCount++; 
                }
                else if (v.Parent != w) // Back Edge
                {
                    v.Low = Math.Min(v.Low, w.Num); // Rule 2
                }
            }

            if (childCount > 1)
                ArticulationPoints.Add(v.Self);
        }

        private void FindArtPoints(Vertex v)
        {
            v.Visited = true;
            v.Low = v.Num = counter++; // Rule 1
            foreach (Vertex w in GetNeighbours(v.Self))
            {
                if (!w.Visited) // Forward Edge
                {
                    w.Parent = v;
                    FindArtPoints(w);
                    v.Low = Math.Min(v.Low, w.Low); // Rule 3
                    if (w.Low >= v.Num)
                        ArticulationPoints.Add(v.Self);
                }
                else if (v.Parent != w) // Back Edge
                {
                    v.Low = Math.Min(v.Low, w.Num); // Rule 2
                }
            }
        }

        public void DivideChambers()
        {
            bool[,] tempBoard = (bool[,])board.Clone();
            List<Point> openSet = new List<Point>();
            // fill AP points to seperate maps into zones
            foreach (Point p in ArticulationPoints)
            {
                tempBoard[p.X, p.Y] = true;
                foreach (Point p1 in Utility.GetValidMoves(p, tempBoard))
                {
                    if (!ArticulationPoints.Contains(p1))
                    {
                        openSet.Add(p1);
                    }
                }
            }

            foreach (Point p in openSet)
            {
                IndexedPointHash zone = Utility.GetOpenSpotForSelf(p, tempBoard);
                if (zone.Count > 0)
                    zones.Add(zone);
            }
        }

        public IndexedPointHash GetLargestZone()
        {
            List<int> indexes = new List<int>();
            int index = 0;
            for (int i = 0; i < zones.Count; i++)
            {
                if (zones[index].Count < zones[i].Count)
                {
                    index = i;
                    indexes.Clear();
                    indexes.Add(i);
                }
                else if (zones[index].Count == zones[i].Count)
                {
                    indexes.Add(i);
                }
            }

            IndexedPointHash result = null;
            if (zones.Count > 0)
            {
                if (indexes.Count == 1)
                    result = zones[index];
                else
                {
                    result = new IndexedPointHash();
                    foreach (int i in indexes)
                    {
                        result.Merge(zones[i]);
                    }
                }

                foreach (Point p in ArticulationPoints)
                {
                    foreach (Point p1 in Utility.GetValidMoves(p, board))
                    {
                        if (result.Contains(p1) && !result.Contains(p))
                        {
                            result.Add(p, 1);
                        }
                    }
                }
            }


            return result;
        }

        private void DisplayZones()
        {
            for (int i = 0; i < Map.Height(); i++)
            {
                for (int j = 0; j < Map.Width(); j++)
                {
                    if (board[j, i])
                        System.Diagnostics.Trace.Write("#");
                    else
                    {
                        bool found = false;
                        foreach (IndexedPointHash ph in zones)
                        {
                            if (ph.Contains(j, i))
                            {
                                System.Diagnostics.Trace.Write(Char.ConvertFromUtf32('a' + zones.IndexOf(ph)));
                                found = true;
                                continue;
                            }
                            if (!found)
                                System.Diagnostics.Trace.Write(" ");
                        }
                    }
                }
                System.Diagnostics.Trace.Write("|" + Environment.NewLine);
            }
        }
    }
}
