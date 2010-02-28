using System.Collections.Generic;
using System.Collections;
using System;

namespace bill_ai
{
    public static class Utility
    {
        public static List<DirectionEnum> GetValidMoves(Point p)
        {
            int x = p.X;
            int y = p.Y;
            List<DirectionEnum> validMoves = new List<DirectionEnum>();

            if (!Map.IsWall(x, y - 1))
            {
                validMoves.Add(DirectionEnum.North);
            }
            if (!Map.IsWall(x + 1, y))
            {
                validMoves.Add(DirectionEnum.East);
            }
            if (!Map.IsWall(x, y + 1))
            {
                validMoves.Add(DirectionEnum.South);
            }
            if (!Map.IsWall(x - 1, y))
            {
                validMoves.Add(DirectionEnum.West);
            }

            return validMoves;
        }

        public static List<Point> GetValidMoves(Point p, bool[,] board)
        {
            List<Point> result = new List<Point>();
            int x = p.X;
            int y = p.Y;
            if (!IsWall(x+1, y, board))
            {
                result.Add(new Point(x + 1, y));
            }
            if (!IsWall(x - 1, y, board))
            {
                result.Add(new Point(x - 1, y));
            }
            if (!IsWall(x, y+1, board))
            {
                result.Add(new Point(x, y+1));
            }
            if (!IsWall(x, y-1, board))
            {
                result.Add(new Point(x, y-1));
            }

            return result;
        }

        public static Point OffsetPoint(Point p, DirectionEnum d)
        {
            Point result = new Point(p.X,p.Y);

            switch (d)
            {
                case DirectionEnum.North:
                    result.X--;
                    break;

                case DirectionEnum.East:
                    result.Y++;
                    break;

                case DirectionEnum.South:
                    result.X++;
                    break;

                case DirectionEnum.West:
                    result.Y--;
                    break;
            }

            return result;
        }

        public static IndexedPointHash GetOpenSpotForSelf(Point p, bool[,] board)
        {
            IndexedPointHash spots = new IndexedPointHash();

            Queue<Point> openSet = new Queue<Point>();
            openSet.Enqueue(p);

            while (openSet.Count > 0)
            {
                Point cur = openSet.Dequeue();

                // add tp neighbors to openSet
                foreach (Point tp in GetValidMoves(cur, board))
                {
                    openSet.Enqueue(tp);
                    board[tp.X, tp.Y] = true;
                    if (spots != null)
                        spots.Add(tp, 1);
                }
            }

            return spots;
        }

        public static bool IsWall(int x, int y, bool[,] board)
        {
            if (x < 0 || y < 0 || x >= board.GetUpperBound(0) || y >= board.GetUpperBound(1))
            {
                return true;
            }
            else
            {
                return board[x, y];
            }
        }

        public static DirectionEnum CalcDirection(Point src, Point dest)
        {
            DirectionEnum result = DirectionEnum.North;

            if (dest.X == src.X)
            {
                if (dest.Y == src.Y + 1)
                    result = DirectionEnum.South;
                else if (dest.Y == src.Y - 1)
                    result = DirectionEnum.North;
            }
            else if (dest.Y == src.Y)
            {
                if (dest.X == src.X + 1)
                    result = DirectionEnum.East;
                else if (dest.X == src.X - 1)
                    result = DirectionEnum.West;
            }

            return result;
        }

        public static DirectionEnum CalcDirection(SurvivalNode src, SurvivalNode dest)
        {
            if (src != null || src != null)
                return CalcDirection(src.self, dest.self);
            else
                return DirectionEnum.North;
        }

        /// <summary>
        ///real A* implementation
        /// </summary>
        /// <returns>null if no path is found</returns>
        public static List<Point> FindPath(Point src, Point dest, bool[,] board)
        {            
            if (src.Equals(dest))
                return null;

            List<Point> closedSet = new List<Point>();
            SortedPointList openSet = new SortedPointList();
            Dictionary<Point, int> gScore = new Dictionary<Point, int>();
            Dictionary<Point, int> hScore = new Dictionary<Point, int>();
            Dictionary<Point, int> fScore = new Dictionary<Point, int>();
            Dictionary<Point, Point> cameFrom = new Dictionary<Point, Point>();

            gScore[src] = 0;
            hScore[src] = GetHeuristicDistance(src, dest);
            fScore[src] = hScore[src];
            openSet.Add(src, fScore[src]);


            while (openSet.Count > 0)
            {
                Point cur = openSet.Pop();
                closedSet.Add(cur);
                if (GetHeuristicDistance(cur, dest) == 1)
                {
                    cameFrom[dest] = cur;
                    return ReconstructPath(cameFrom, dest);
                }

                foreach (Point child in GetValidMoves(cur, board))
                {
                    if (closedSet.Contains(child))
                        continue;

                    int tentativeGScore = gScore[cur] + GetHeuristicDistance(cur, child);
                    int tentativeHScore = GetHeuristicDistance(child, dest);
                    bool tentativeIsBetter = false;
                    if (!openSet.ContainsPoint(child))
                    {
                        openSet.Add(child, tentativeGScore + tentativeHScore);
                        tentativeIsBetter = true;
                    }
                    else if (gScore.ContainsKey(child) && tentativeGScore < gScore[child])
                    {
                        tentativeIsBetter = true;
                    }

                    if (tentativeIsBetter)
                    {
                        cameFrom[child] = cur;
                        gScore[child] = tentativeGScore;
                        hScore[child] = tentativeHScore;
                        fScore[child] = gScore[child] + hScore[child];
                    }
                }
            }

            return null;
        }

        private static List<Point> ReconstructPath(Dictionary<Point, Point> cameFrom, Point cur)
        {
            List<Point> result;
            if (cameFrom.ContainsKey(cur))
            {
                result = ReconstructPath(cameFrom, cameFrom[cur]);
            }
            else
            {
                result = new List<Point>();
            }
            result.Add(cur);

            return result;
        }

        public static int GetHeuristicDistance(Point a, Point b)
        {
            return Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);
        }

        public static IndexedPointHash InterceptPointHash(IndexedPointHash myOpenSpots, IndexedPointHash hisOpenSpots)
        {
            IndexedPointHash validSpots = new IndexedPointHash();
            for (int i = myOpenSpots.Count - 1; i >= 0; i--)
            {
                Point spot = myOpenSpots.GetPointAt(i);
                if (hisOpenSpots.Contains(spot))
                {
                    validSpots.Add(spot, 1);
                    myOpenSpots.Remove(spot);
                    hisOpenSpots.Remove(spot);
                }
            }

            return validSpots;
        }

        public static int GetHeuristicScore(Point me, Point him, bool[,] board)
        {
            int score = int.MinValue;

            IndexedPointHash myOpenSpots =
                GetOpenSpotForSelf(me, (bool[,])board.Clone());
            IndexedPointHash hisOpenSpots =
                GetOpenSpotForSelf(him, (bool[,])board.Clone());
            IndexedPointHash validSpots = InterceptPointHash(myOpenSpots, hisOpenSpots);
            // if we are sepeprated
            if (validSpots.Count < myOpenSpots.Count || validSpots.Count < hisOpenSpots.Count)
            {
                if (myOpenSpots.Count > hisOpenSpots.Count)
                    score = MyTronBot.sepScore;
                else if (myOpenSpots.Count < hisOpenSpots.Count)
                    score = -MyTronBot.sepScore;
                else
                    score = MyTronBot.drawScore;
            }
            
            if (score == int.MinValue)
                score = GetHeuristicScore(ref me, ref him, board, validSpots);

            return score;
        }

        public static int GetHeuristicScore(ref Point me, ref Point him, bool[,] board, IndexedPointHash validSpots)
        {
            IndexedPointHash mine = new IndexedPointHash();
            IndexedPointHash his = new IndexedPointHash();
            int myValidCount = 0;
            int hisValidCount = 0;
            mine.Add(me, -1);
            his.Add(him, -1);

            List<Point> myPathToHeaven = Utility.FindPath(me, validSpots.GetPointAt(0), board);
            for (int i = 0; myPathToHeaven != null && i < myPathToHeaven.Count; i++)
            {
                if (validSpots.Contains(myPathToHeaven[i]))
                    break;
                else
                    validSpots.Add(myPathToHeaven[i], 1);
            }
            List<Point> hisPathToHeaven = Utility.FindPath(him, validSpots.GetPointAt(0), board);
            for (int i = 0; hisPathToHeaven != null && i < hisPathToHeaven.Count; i++)
            {
                if (validSpots.Contains(hisPathToHeaven[i]))
                    break;
                else
                    validSpots.Add(hisPathToHeaven[i], 1);
            }

            int index = 0;
            while (index < mine.Count || index < his.Count)
            {
                if (index < mine.Count)
                {
                    foreach (Point p in GetValidMoves(mine.GetPointAt(index), board))
                    {
                        // in order to be added to mine list, following conditions must apply
                        // 0, must exist in validSpots
                        // 1, not exist in mine
                        // 2, not exist in his 
                        if (!mine.Contains(p)) 
                        {
                            if (!his.Contains(p) || mine[index] + 1 <= his[p])
                            {
                                mine.Add(p, mine[index] + 1);
                                if (validSpots.Contains(p))
                                    myValidCount++;

                                if (mine[p] < his[p])
                                {
                                    his.Remove(p);
                                    hisValidCount--;
                                }
                            }
                        }
                    }
                }

                if (index < his.Count)
                {
                    foreach (Point p in GetValidMoves(his.GetPointAt(index), board))
                    {
                        // in order to be added to mine list, following conditions must apply
                        // 0, must exist in validSpots
                        // 1, not exist in his
                        // 2, not exist in mine, 
                        //      unless hisScore[current]+1 <= myScore[child]
                        if (!his.Contains(p))
                        {
                            if (!mine.Contains(p) || his[index] + 1 <= mine[p])
                            {
                                his.Add(p, his[index] + 1);
                                if (validSpots.Contains(p))
                                    hisValidCount++;

                                if (his[p] < mine[p])
                                {
                                    mine.Remove(p);
                                    myValidCount--;
                                }
                            }
                        }
                    }
                }

                index++;
            }

            return myValidCount - hisValidCount;
        }

        public static void DrawBoard(GameState state)
        {
            Point p1 = state.me;
            Point p2 = state.enemy;
            bool[,] board = state.board;
            for (int i = 0; i < Map.Height(); i++)
            {
                for (int j = 0; j < Map.Width(); j++)
                {
                    if (p1.X == j && p1.Y == i)
                        System.Diagnostics.Trace.Write("1");
                    else if (p2.X == j && p2.Y == i)
                        System.Diagnostics.Trace.Write("2");
                    else if (board[j, i])
                        System.Diagnostics.Trace.Write("#");
                    else
                        System.Diagnostics.Trace.Write(" ");
                }
                System.Diagnostics.Trace.Write("|" + Environment.NewLine);
            }
        }

        public static void DrawBoard(Point p1, bool[,] board)
        {
            for (int i = 0; i < Map.Height(); i++)
            {
                for (int j = 0; j < Map.Width(); j++)
                {
                    if (p1.X == j && p1.Y == i)
                        System.Diagnostics.Trace.Write("1");
                    else if (board[j, i])
                        System.Diagnostics.Trace.Write("#");
                    else
                        System.Diagnostics.Trace.Write(" ");
                }
                System.Diagnostics.Trace.Write("|" + Environment.NewLine);
            }
        }
    }
}
