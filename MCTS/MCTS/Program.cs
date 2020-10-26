using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace MCTS
{
    class Program
    {
        static void Main(string[] args)
        {
            UltimateTicTacToe uttt = new UltimateTicTacToe();
            uttt.first = new MCTS();
            uttt.second = new MCFlat();
            uttt.RunGame2NTimes(100);
            uttt.Reset();
            uttt.second = new RandomPlayer();
            uttt.RunGame2NTimes(100);

        }
    }
}
