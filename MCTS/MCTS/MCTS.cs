﻿using System;
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
        public MCTS(float c = 2)
        {
            root = null;
            current = null;
            stopWatch = new Stopwatch();
            random = new Random();
            this.c = c;
        }
        readonly Stopwatch stopWatch;
        readonly Random random;
        public MCTSNode root;
        MCTSNode current;
        PositionState myPosition;
        readonly float c;
        long time;
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
            (int, int) move = (-1, -1);
            foreach (MCTSNode child in root.childNodes)
            {
                if (maxN < child.n)
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
                    current = ChooseChild();
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
            List<(int, int)> lm = current.state.LegalMoves();
            foreach ((int, int) move in lm)
            {
                current.AddChild(move);
            }
        }
        void Rollout() 
        {
            GameBoardState state = new GameBoardState(current.state);
            List<(int, int)> legalMoves = state.LegalMoves();
            while (legalMoves.Count != 0)
            {
                state.ApplyMove(legalMoves[random.Next(legalMoves.Count - 1)]);
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
        void BackPropagation(int reward)
        {
            while (current != null)
            {
                if(current.state.turn == myPosition)
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
            //if (time == 1000)
            Console.WriteLine(root.n);
            time = 100;
            return Guess();
        }

        public void OnLose(string msgWhy)
        {
            //logging TODO
        }

        public void OnWin()
        {

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
            time = 1000;
        }

        public void OnMove((int, int) move)
        {
            ChangeRoot(move);
        }
    }
}
