using System;
using System.Collections.Generic;
using bill_ai;

class MyTronBot{

    private static bool canReachEnemy = true;
    private static DateTime turnStart;
    public static int timeOut = 800;
    private static bool hasTimedOut = false;
    private static int mmLevel = 4;
    private static bool firstTurn = true;

    // minimax scorings
    public const int winScore = 1000;
    public const int sepScore = 800;
    public const int drawScore = 0;

#if DEBUG
    private static int scoreCallCount = 0;
#endif

    public static int MakeMove()
    {
#if DEBUG
        scoreCallCount = 0;
#endif
        turnStart = DateTime.Now;
        Point me = new Point(Map.MyX(), Map.MyY());
        Point enemy = new Point(Map.OpponentX(), Map.OpponentY());
        List<DirectionEnum> validMoves = new List<DirectionEnum>();

        validMoves = Utility.GetValidMoves(me);
        DirectionEnum move = DirectionEnum.Invalid;

        // always reset mmLevel
        mmLevel = 2;
        hasTimedOut = false;
        if (validMoves.Count > 0)
        {
            if (canReachEnemy)
            {
                // do path finding
                List<Point> path = Utility.FindPath(me, enemy, Map.walls);
                if (path == null || path.Count <= 1)
                {
                    // if unreachable
                    //  set canReachEnemy, try to survive
                    canReachEnemy = false;
                    move = DoBFSurvival(me);
                }
                else 
                {
                    DirectionEnum temp = DirectionEnum.Invalid;
                    SortedPointList sortedValidMoves = new SortedPointList();
                    foreach (Point p in Utility.GetValidMoves(me, Map.walls))
                    {
                        sortedValidMoves.Add(p, Utility.GetHeuristicScore(p, enemy, Map.walls));
                    }
                    while (!hasTimedOut)
                    {
                        DoCloseCombat(me, enemy, path, ref sortedValidMoves);
                        move = Utility.CalcDirection(me, sortedValidMoves[sortedValidMoves.Count - 1]);
                        mmLevel += 2;
                    }
#if DEBUG
                    Console.Error.WriteLine("mmLevel = {0}, scoreCallCount = {1} ", mmLevel - 2, scoreCallCount);
#endif

                }
            }
            else
            {
                // survival mode
                move = DoBFSurvival(me);
            }

            // do not suicide if above calc went wrong
            if (!validMoves.Contains(move))
            {
                move = validMoves[0];
            }
        }
#if DEBUG
        Console.Error.WriteLine("elapsed: {0}",(DateTime.Now - turnStart).TotalMilliseconds);
#endif
        return (int)move;
    }

#region search algorithms

    private static DirectionEnum DoBFSurvival(Point me)
    {
        SurvivalNode root = new SurvivalNode(me, null);
        Queue<SurvivalNode> leafQueue = new Queue<SurvivalNode>();
        foreach (Point move in root.GetValidMoves())
        {
            SurvivalNode leaf = new SurvivalNode(move, root);
            leafQueue.Enqueue(leaf);
        }

        SurvivalNode bestChild = null;
        int currentLevel = 0;
#if DEBUG
        int calcCount = 0;
#endif
        while (leafQueue.Count > 0 && (DateTime.Now - turnStart).TotalMilliseconds < timeOut)
        {
            SurvivalNode cur = leafQueue.Dequeue();
            // score calculation
            cur.CalcValueForSurvival();
#if DEBUG
            ++calcCount;
#endif
            if (cur.ChangedNodes.Count > currentLevel)
            {
                // one level finished, find best node
                SurvivalNode tempBest = null; 
                foreach (SurvivalNode child in root.children)
                {
                    child.CalcRealValue(turnStart);
                    if (tempBest == null || tempBest.realValue < child.realValue)
                    {
                        tempBest = child;
                    }
                    else if (tempBest.realValue == child.realValue &&
                        Utility.GetValidMoves(tempBest.self).Count > Utility.GetValidMoves(child.self).Count)
                    {
                        // prefer corner > edge > center
                        tempBest = child;
                    }

                    if ((DateTime.Now - turnStart).TotalMilliseconds > timeOut)
                        goto OUTSIDE;
                }

                bestChild = tempBest;
                currentLevel = cur.ChangedNodes.Count;

            }

            foreach (Point move in cur.GetValidMoves())
            {
                leafQueue.Enqueue(new SurvivalNode(move, cur));
            }
        }

        // shouldn't never get in here, just in case...
        OUTSIDE: if (bestChild == null)
        {
            foreach (SurvivalNode child in root.children)
            {
                child.CalcRealValue(turnStart);
                if (bestChild == null || bestChild.realValue < child.realValue)
                {
                    bestChild = child;
                }
            }
        }

#if DEBUG
        Console.Error.WriteLine("currentLevel = {0}, calcCount = {1}", currentLevel, calcCount);
#endif
        return Utility.CalcDirection(root, bestChild); 
    }

    private static void DoCloseCombat(Point me, Point enemy, List<Point> path, ref SortedPointList sortedValidMoves)
    {
        GameState rootState = new GameState(me, enemy, true, Map.walls);
        SortedPointList newSortedValidMoves = new SortedPointList();

        for (int i = sortedValidMoves.Count - 1; i >= 0; i--)
        {
            Point p = sortedValidMoves[i];
            bool[,] newBoard = (bool[,])rootState.board.Clone();
            newBoard[p.X, p.Y] = true;
            GameState newState = new GameState(rootState.enemy, p, false, newBoard);
            int minVal = -AlphaBeta(newState, mmLevel, int.MinValue + 1, int.MaxValue -1);
#if DEBUG
            Console.Error.WriteLine("Direction: {0}, score: {1}, hasTimedOut {2}", 
                Utility.CalcDirection(me, p).ToString(), minVal, hasTimedOut.ToString());
#endif
            newSortedValidMoves.Add(p, minVal);
        }

        
        if (hasTimedOut)
        {
            // if timed out, we do not consider the new score, but with one exception:
            //   if a really bad/good score is discovered, do take note of it
            for (int i = 0; i < newSortedValidMoves.Count; i++)
            {
                if (newSortedValidMoves.GetValue(i) > winScore / 2 ||
                    newSortedValidMoves.GetValue(i) < -winScore / 2)
                {
                    sortedValidMoves.Remove(newSortedValidMoves[i]);
                    sortedValidMoves.Add(newSortedValidMoves[i], newSortedValidMoves.GetValue(i));
                }
            }
        }
        else
            sortedValidMoves = newSortedValidMoves;

    }

    private static int AlphaBeta(GameState state, int depth, int alpha, int beta)
    {
        // time out checks
        if (hasTimedOut || (DateTime.Now - turnStart).TotalMilliseconds > timeOut)
        {
            hasTimedOut = true;
            return drawScore;
        }
        // end game conditions
        int score = EndGameScore(state, depth);
        if (score != int.MinValue)
            return score;

        List<Point> validMoves = Utility.GetValidMoves(state.me, state.board);
        // add enemy spot to moves, when it's enemy's turn 
        if (!state.isForMe && Utility.GetHeuristicDistance(state.me, state.enemy) == 1)
        {
            validMoves.Add(state.enemy);
        }
        
        foreach (Point p1 in validMoves)
        {
            bool[,] newBoard = (bool[,])state.board.Clone();
            newBoard[p1.X, p1.Y] = true;
            GameState newState = new GameState(state.enemy, p1, !state.isForMe, newBoard);
            int minVal = -AlphaBeta(newState, depth - 1, -beta, -alpha);
            alpha = Math.Max(alpha, minVal);

            if (beta <= alpha)
                break;
        }

        return alpha;
    }

    private static int EndGameScore(GameState state, int depth)
    {
        int myValidMoves = Utility.GetValidMoves(state.me, state.board).Count;
        int enemyValidMoves = Utility.GetValidMoves(state.enemy, state.board).Count;

        if (state.me.Equals(state.enemy))
            return drawScore;
        if (state.isForMe)
        {
            if (myValidMoves == 0 && enemyValidMoves == 0)
                return drawScore;
            else if (myValidMoves == 0)
                return -winScore;
            else if (enemyValidMoves == 0)
                return winScore;

            // check if seperated, see who has more spots
            IndexedPointHash myOpenSpots = 
                Utility.GetOpenSpotForSelf(state.me, (bool[,])state.board.Clone());
            IndexedPointHash hisOpenSpots =
                Utility.GetOpenSpotForSelf(state.enemy, (bool[,])state.board.Clone());
            IndexedPointHash validSpots = Utility.InterceptPointHash(myOpenSpots, hisOpenSpots);
            if (validSpots.Count < myOpenSpots.Count || validSpots.Count < hisOpenSpots.Count)
            {
                if (myOpenSpots.Count > hisOpenSpots.Count)
                    return (sepScore - (mmLevel - depth) / 2);
                else if (myOpenSpots.Count < hisOpenSpots.Count)
                    return (-sepScore + (mmLevel - depth) / 2);
                else
                    return drawScore;
            }

            // check if neighbour is choke point
            // if yes seperate open spots into zones
            // find the biggest zone
            //  if enemy is in the zone
            //   GetHeuristicScore on the big zone
            //  else
            //   return sepScore

            if (depth <= 0)
            {
#if DEBUG
                scoreCallCount++;
#endif
                // find articulation point
                Graph graph = new Graph(validSpots.GetPointAt(0), state.board);
                graph.FindArtPoints();
                graph.DivideChambers();
                IndexedPointHash largestZone = graph.GetLargestZone();
                // heuristic score
                return Utility.GetHeuristicScore(
                    ref state.me,ref state.enemy, state.board, largestZone == null ? validSpots:largestZone);
            }
        }
        else
        {
            if (myValidMoves == 0)
                if (Utility.GetHeuristicDistance(state.me, state.enemy) == 1)
                    return drawScore; // assuming draw is better for enemy
                else
                    return -winScore;
            else if (enemyValidMoves == 0)
                return winScore;
        }

        return int.MinValue;
    }
#endregion

   public static void Main()
   {
       try
       {
           while (true)
           {
               Map.Initialize();

               if (firstTurn)
               {
                   firstTurn = false;
               }

               Map.MakeMove(MakeMove());
           }
       }
       catch (Exception ex)
       {
#if DEBUG
           Console.Error.WriteLine("FATAL ERROR: " + ex.ToString());
           Console.ReadLine();           
#endif
           Environment.Exit(1);
       }
   }
}
       
