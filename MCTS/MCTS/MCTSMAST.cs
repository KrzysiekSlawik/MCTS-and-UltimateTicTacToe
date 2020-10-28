using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Linq;

namespace MCTS
{
    class MASTData
    {
        public int NMe;
        public int NOther;
        public int RMe;
        public int ROther;
        public float Quality(bool me)
        {
            if(me)
            {
                if (NMe == 0) return 0;
                return (float)RMe / (float)NMe;
            }
            else
            {
                if (NOther == 0) return 0;
                return (float)ROther / (float)NOther;
            }
        }
    }
    class MCTSMAST : IUTTTPlayer
    {
        public MCTSMAST(PositionState turn, float c = 1.41421356237f)
        {
            root = new MCTSNode(turn);
            current = root;
            stopWatch = new Stopwatch();
            random = new Random();
            this.c = c;
            InitMAST(0.4);
        }
        public MCTSMAST(float c, double e)
        {
            root = null;
            current = null;
            stopWatch = new Stopwatch();
            random = new Random();
            this.c = c;
            
            InitMAST(e);
        }

        private void InitMAST(double e)
        {
            trackedMoves = new List<(int, int)>();
            EMAST = e;
            MAST = new MASTData[9, 9];
            for (int x = 0; x < 9; x++)
            {
                for (int y = 0; y < 9; y++)
                {
                    MAST[x, y] = new MASTData();
                }
            }
        }

        readonly Stopwatch stopWatch;
        readonly Random random;
        public MCTSNode root;
        MCTSNode current;
        PositionState myPosition;
        readonly float c;
        long time;
        List<(int, int)> trackedMoves;
        MASTData[,] MAST;
        double EMAST;
        private MASTData GetMast((int,int)move)
        {
            if (move == (-1, -1)) return null;
            return MAST[move.Item1, move.Item2];
        }
        public void ImproveGuess(long timeLimitMs)
        {
            stopWatch.Restart();
            while (stopWatch.ElapsedMilliseconds < timeLimitMs)
            {
                TreeTraversal();
            }
        }
        public void ChangeRoot((int, int) move)
        {
            foreach (MCTSNode child in root.childNodes)
            {
                if (child.move == move)
                {
                    root = child;
                    root.parent = null;
                    current = root;
                    return;
                }
            }
            root.AddChild(move);
            root = root.childNodes[0];
            root.parent = null;
            current = root;
            return;
        }
        public (int, int) Guess()
        {
            int maxN = -1;
            (int, int) move = (0, 0);
            foreach (MCTSNode child in root.childNodes)
            {
                if (maxN <= child.n)
                {
                    move = child.move;
                    maxN = child.n;
                }
            }
            return move;
        }
        void TreeTraversal()
        {
            if (IsLeaf(current))
            {
                if (current.n == 0)
                {
                    Rollout();
                }
                else
                {
                    NodeExpansion();
                    trackedMoves.Add(current.move);
                    current = ChooseChild();
                    Rollout();
                }
            }
            else
            {
                trackedMoves.Add(current.move);
                current = ChooseChild();
            }
        }
        void NodeExpansion()
        {
            foreach ((int, int) move in current.state.LegalMoves())
            {
                current.AddChild(move);
            }
        }
        void Rollout()
        {
            trackedMoves.Add(current.move);
            GameBoardState state = new GameBoardState(current.state);
            List<(int, int)> legalMoves = state.LegalMoves();
            while (legalMoves.Count != 0)
            {
                (int,int) mv = MASTChoice(legalMoves, state.turn);
                state.ApplyMove(mv);
                trackedMoves.Add(mv);
                legalMoves = state.LegalMoves();
            }
            switch (state.gameState)
            {
                case GameState.firstWin:
                    if (myPosition == PositionState.first)
                    {
                        BackPropagation(2);
                    }
                    else
                    {
                        BackPropagation(0);
                    }
                    return;
                case GameState.secondWin:
                    if (myPosition == PositionState.first)
                    {
                        BackPropagation(0);
                    }
                    else
                    {
                        BackPropagation(2);
                    }
                    return;
                case GameState.draw:
                    BackPropagation(1);
                    return;
            }
        }

        private (int, int) MASTChoice(List<(int, int)> legalMoves, PositionState turn)
        {
            if(random.NextDouble() > EMAST)
            {
                bool me = turn == myPosition;

                float maxQ = -1;
                (int, int) bestMove = (-1,-1);
                foreach ((int, int) move in legalMoves)
                {
                    float q = GetMast(move).Quality(me);
                    if(maxQ < q)
                    {
                        maxQ = q;
                        bestMove = move;
                    }
                }
                return bestMove;
            }
            else
            {
                return legalMoves[random.Next(legalMoves.Count - 1)];
            }
        }
        void BackPropagation(int reward)
        {
            while (current != null)
            {
                if (current.state.turn == myPosition)
                {
                    current.n++;
                    current.reward += 2 - reward;
                    current = current.parent;
                }
                else
                {
                    current.n++;
                    current.reward += reward;
                    current = current.parent;
                }
            }
            current = root;
            bool me = false;
            foreach((int,int) move in trackedMoves)
            {
                MASTData d = GetMast(move);
                if(d != null)
                {
                    if (me)
                    {
                        d.NMe++;
                        d.RMe += reward;
                    }
                    else
                    {
                        d.NOther++;
                        d.ROther += 2 - reward;
                    }
                }
                me = !me;
            }
            trackedMoves.Clear();
        }
        bool IsLeaf(MCTSNode node)
        {
            return node.childNodes.Count == 0;
        }
        MCTSNode ChooseChild()
        {
            if (current.checkFirst.Count != 0)
            {
                int i = random.Next(current.checkFirst.Count - 1);
                MCTSNode child = current.checkFirst[i];
                current.checkFirst.RemoveAt(i);
                return child;
            }
            else
            {
                float max = -1;
                MCTSNode bestChild = current;
                foreach (MCTSNode child in current.childNodes)
                {
                    float value = UCT(child);
                    if (value > max)
                    {
                        bestChild = child;
                        max = value;
                    }
                }
                return bestChild;
            }
        }
        float UCT(MCTSNode node)
        {
            return node.vAvg + c * MathF.Sqrt(MathF.Log2(node.parent.n) / node.n);
        }

        public (int, int) GetMove(GameBoardState state)
        {
            ImproveGuess(time);
            Decay();
            time = 100;
            return Guess();
        }
        private void Decay()
        {
            for (int x = 0; x < 9; x++)
            {
                for (int y = 0; y < 9; y++)
                {
                    MASTData d = MAST[x, y];
                    d.NMe /= 100;
                    d.RMe /= 100;
                    d.NOther /= 100;
                    d.ROther /= 100;
                }
            }
        }
        public void OnLose(string msgWhy)
        {
            //logging TODO
        }

        public void OnWin()
        {
            //logging TODO
        }

        public void Reset()
        {
            root = null;
            current = null;
            InitMAST(EMAST);
        }

        public void OnStart(bool isFirst, PositionState myPosition)
        {
            this.myPosition = myPosition;
            root = new MCTSNode(isFirst ? myPosition : myPosition == PositionState.first ? PositionState.second : PositionState.first);
            current = root;
            time = 1000;
        }

        public void OnMove((int, int) move)
        {
            ChangeRoot(move);
        }
    }
}
