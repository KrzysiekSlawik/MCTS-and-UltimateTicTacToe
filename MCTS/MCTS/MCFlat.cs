using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace MCTS
{
    class MCFlat : IUTTTPlayer
    {
        public MCFlat()
        {
            random = new Random();
            stopwatch = new Stopwatch();
            c = 1.41421356237f;
        }
        MCTSNode root;
        readonly Random random;
        readonly Stopwatch stopwatch;
        PositionState myPosition;
        readonly float c;
        private long time;
        void NodeExpansion(MCTSNode node)
        {
            foreach ((int, int) move in node.state.LegalMoves())
            {
                node.AddChild(move);
            }
        }
        float UCT(MCTSNode node)
        {
            int N = node.parent == null ? 1 : node.parent.n;
            return node.vAvg + c * MathF.Sqrt(MathF.Log2(N) / node.n);
        }
        public (int, int) GetMove(GameBoardState state)
        {
            stopwatch.Restart();
            root = new MCTSNode(state);
            NodeExpansion(root);
            while(stopwatch.ElapsedMilliseconds<time)
            {
                Rollout(ChooseChild());
            }
            int maxN = 0;
            (int, int) move = (-1, -1);
            foreach (MCTSNode child in root.childNodes)
            {
                if (maxN < child.n)
                {
                    move = child.move;
                    maxN = child.n;
                }
            }
            //Console.ForegroundColor = ConsoleColor.Cyan;
            //Console.WriteLine($"MC number of simulations {root.n} with average reward {root.vAvg}");
            //Console.ResetColor();
            //if (time == 1000) Console.WriteLine(root.n);
            time = 100;
            return move;
        }
        MCTSNode ChooseChild()
        {
            if (root.checkFirst.Count != 0)
            {
                int i = random.Next(root.checkFirst.Count - 1);
                MCTSNode child = root.checkFirst[i];
                root.checkFirst.RemoveAt(i);
                return child;
            }
            else
            {
                float max = 0;
                MCTSNode bestChild = null;
                foreach (MCTSNode child in root.childNodes)
                {
                    float value = UCT(child);
                    if (value >= max)
                    {
                        bestChild = child;
                        max = value;
                    }
                }
                return bestChild;
            }
        }
        void Rollout(MCTSNode current)
        {
            GameBoardState state = new GameBoardState(current.state);
            if (state.gameState != GameState.running)
            {
                switch (state.gameState)
                {
                    case GameState.running:
                        break;
                    case GameState.firstWin:
                        if (myPosition == PositionState.first)
                        {
                            BackPropagation(2, current);
                        }
                        else
                        {
                            BackPropagation(0, current);
                        }
                        return;
                    case GameState.draw:
                        BackPropagation(1, current);
                        return;
                    case GameState.secondWin:
                        if (myPosition == PositionState.first)
                        {
                            BackPropagation(0, current);
                        }
                        else
                        {
                            BackPropagation(2, current);
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
                            BackPropagation(2, current);
                        }
                        else
                        {
                            BackPropagation(0, current);
                        }
                        return;
                    case GameState.draw:
                        BackPropagation(1, current);
                        return;
                    case GameState.secondWin:
                        if (myPosition == PositionState.first)
                        {
                            BackPropagation(0, current);
                        }
                        else
                        {
                            BackPropagation(2, current);
                        }
                        return;
                }
            }
        }
        void BackPropagation(int reward, MCTSNode current)
        {
            while (current != null)
            {
                current.n++;
                current.reward += reward;
                current = current.parent;
            }
        }
        public void OnLose(string msgWhy)
        {
            //TODO
        }

        public void OnMove((int, int) move)
        {
            //not required MCFlat doesn't use previous calculations
        }

        public void OnStart(bool isFirst, PositionState player)
        {
            myPosition = player;
            time = 1000;
        }

        public void OnWin()
        {
            //TODO
        }

        public void Reset()
        {
            //TODO
        }
    }
}
