using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace MCTS
{
    public class MCTSNode
    {
        public (int, int) move;
        public MCTSNode parent;
        public List<MCTSNode> childNodes;
        public List<MCTSNode> checkFirst;
        public int n;
        public int reward;
        public float vAvg
        {
            get
            {
                return (float)reward / (float)n;
            }
        }
        public GameBoardState state;
        public MCTSNode(PositionState turn)
        {
            move = (-1, -1);
            parent = null;
            n = 0;
            reward = 0;
            state = new GameBoardState(turn);
            childNodes = new List<MCTSNode>();
            checkFirst = new List<MCTSNode>();
        }
        public MCTSNode(GameBoardState state)
        {
            move = (-1, -1);
            parent = null;
            n = 0;
            reward = 0;
            this.state = state;
            childNodes = new List<MCTSNode>();
            checkFirst = new List<MCTSNode>();
        }
        private MCTSNode((int, int) move, MCTSNode parent)
        {
            this.move = move;
            this.parent = parent;
            n = 0;
            reward = 0;
            state = new GameBoardState(parent.state);
            state.ApplyMove(move);
            childNodes = new List<MCTSNode>();
            checkFirst = new List<MCTSNode>();
        }
        internal void AddChild((int, int) move)
        {
            MCTSNode child = new MCTSNode(move, this);
            childNodes.Add(child);
            checkFirst.Add(child);
        }
    }
    class MCTS : IUTTTPlayer
    {
        public MCTS(PositionState turn, float c = 1.41421356237f)
        {
            root = new MCTSNode(turn);
            current = root;
            stopWatch = new Stopwatch();
            random = new Random();
            this.c = c;
        }
        public MCTS()
        {
            root = null;
            current = null;
            stopWatch = new Stopwatch();
            random = new Random();
            c = 1.41421356237f;
        }
        Stopwatch stopWatch;
        Random random;
        public MCTSNode root;
        MCTSNode current;
        PositionState myPosition;
        float c;
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
                    return;
                }
            }
            root.AddChild(move);
            root = root.childNodes[0];
            root.parent = null;
            return;
        }
        public (int, int) Guess()
        {
            int maxN = 0;
            (int, int) move = (-1, -1);
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
            if (current == null) current = root;
            if (IsLeaf(current))
            {
                if (current.n == 0)
                {
                    Rollout();
                }
                else
                {
                    NodeExpansion();
                    ChooseChild();
                    Rollout();
                }
            }
            else
            {
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
            GameBoardState state = new GameBoardState(current.state);
            if (state.gameState != GameState.running)
            {
                switch (state.gameState)
                {
                    case GameState.running:
                        break;
                    case GameState.firstWin:
                        if(myPosition == PositionState.first)
                        {
                            BackPropagation(2);
                        }
                        else
                        {
                            BackPropagation(0);
                        }
                        return;
                    case GameState.draw:
                        BackPropagation(1);
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
                }
            }

            while (true)
            {
                List<(int, int)> legalMoves = state.LegalMoves();
                switch (state.ApplyMove(legalMoves[random.Next(legalMoves.Count - 1)]))
                {
                    case GameState.running:
                        break;
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
                    case GameState.draw:
                        BackPropagation(1);
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
                }
            }
        }
        void BackPropagation(int reward)
        {
            while (current != null)
            {
                current.n++;
                current.reward += reward;
                current = current.parent;
            }
            current = root;
        }
        bool IsLeaf(MCTSNode node)
        {
            return node.childNodes.Count == 0;
        }
        MCTSNode ChooseChild()
        {
            if (current.checkFirst.Count != 0)
            {
                //int i = random.Next(current.checkFirst.Count - 1);
                MCTSNode child = current.checkFirst[0];
                current.checkFirst.RemoveAt(0);
                return child;
            }
            else
            {
                float max = 0;
                MCTSNode bestChild = null;
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
            int N = node.parent == null ? 1 : node.parent.n;
            return node.vAvg + c * MathF.Sqrt(MathF.Log2(N) / node.n);
        }

        public (int, int) GetMove(GameBoardState state)
        {
            ImproveGuess(100);
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"MCTS number of simulations {root.n} with average reward {root.vAvg}");
            Console.ResetColor();
            return Guess();
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
        }

        public void OnStart(bool isFirst, PositionState myPosition)
        {
            this.myPosition = myPosition;
            root = new MCTSNode(isFirst ? myPosition : myPosition == PositionState.first ? PositionState.second : PositionState.first);
            current = root;
        }

        public void OnMove((int, int) move)
        {
            ChangeRoot(move);
        }
    }
}
