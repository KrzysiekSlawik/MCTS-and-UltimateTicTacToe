using System;
using System.Collections.Generic;
using System.Text;

namespace MCTS
{
    class RandomPlayer : IUTTTPlayer
    {
        readonly Random random;
        public RandomPlayer()
        {
            random = new Random();
        }
        public (int, int) GetMove(GameBoardState state)
        {
            List<(int, int)> moves = state.LegalMoves();
            return moves[random.Next(moves.Count-1)];
        }

        public void OnLose(string msgWhy)
        {
            //
        }

        public void OnMove((int, int) move)
        {
            //
        }

        public void OnStart(bool isFirst, PositionState player)
        {
            //
        }

        public void OnWin()
        {
            //
        }

        public void Reset()
        {
            //
        }
    }
}
