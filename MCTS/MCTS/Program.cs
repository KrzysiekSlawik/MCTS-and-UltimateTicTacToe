using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace MCTS
{
    class Program
    {
        static void Main(string[] args)
        {
            TestMASTFLAT();
        }
        static void TestMCTSFLAT()
        {
            Console.WriteLine("================");
            Console.WriteLine("TESTS MCTS vs FLAT");
            Console.WriteLine("MCTS c=1.9");
            Run4Threads(TestMCTSFLAT1);
            Console.WriteLine("MCTS c=2.05");
            Run4Threads(TestMCTSFLAT2);
            Console.WriteLine("MCTS c=2.2");
            Run4Threads(TestMCTSFLAT3);
            Console.WriteLine("MCTS c=2.35");
            Run4Threads(TestMCTSFLAT4);
        }
        static void TestMASTFLAT()
        {
            Console.WriteLine("================");
            Console.WriteLine("TESTS MCTS(MAST) vs FLAT");
            Console.WriteLine("MCTS c=2.05, e=0.4");
            Run4Threads(TestMASTFLAT1);
            Console.WriteLine("MCTS c=2.05, e=0.3");
            Run4Threads(TestMASTFLAT2);
            Console.WriteLine("MCTS c=2.05, e=0.2");
            Run4Threads(TestMASTFLAT3);
            Console.WriteLine("MCTS c=2.05, e=0.1");
            Run4Threads(TestMASTFLAT4);
        }
        static (int, int) TestMASTFLAT1()
        {
            UltimateTicTacToe uttt = new UltimateTicTacToe();
            uttt.first = new MCTSMAST(2.05f, 0.4);
            uttt.second = new MCFlat();
            return uttt.RunGame2NTimes(10);
        }
        static (int, int) TestMASTFLAT2()
        {
            UltimateTicTacToe uttt = new UltimateTicTacToe();
            uttt.first = new MCTSMAST(2.05f, 0.3);
            uttt.second = new MCFlat();
            return uttt.RunGame2NTimes(10);
        }
        static (int, int) TestMASTFLAT3()
        {
            UltimateTicTacToe uttt = new UltimateTicTacToe();
            uttt.first = new MCTSMAST(2.05f, 0.2);
            uttt.second = new MCFlat();
            return uttt.RunGame2NTimes(10);
        }
        static (int, int) TestMASTFLAT4()
        {
            UltimateTicTacToe uttt = new UltimateTicTacToe();
            uttt.first = new MCTSMAST(2.05f, 0.1);
            uttt.second = new MCFlat();
            return uttt.RunGame2NTimes(10);
        }
        static (int,int) TestMCTSFLAT1()
        {
            UltimateTicTacToe uttt = new UltimateTicTacToe();
            uttt.first = new MCTS(1.9f);
            uttt.second = new MCFlat();
            return uttt.RunGame2NTimes(5);
        }
        static (int, int) TestMCTSFLAT2()
        {
            UltimateTicTacToe uttt = new UltimateTicTacToe();
            uttt.first = new MCTS(2.05f);
            uttt.second = new MCFlat();
            return uttt.RunGame2NTimes(5);
        }
        static (int, int) TestMCTSFLAT3()
        {
            UltimateTicTacToe uttt = new UltimateTicTacToe();
            uttt.first = new MCTS(2.2f);
            uttt.second = new MCFlat();
            return uttt.RunGame2NTimes(5);
        }
        static (int, int) TestMCTSFLAT4()
        {
            UltimateTicTacToe uttt = new UltimateTicTacToe();
            uttt.first = new MCTS(2.35f);
            uttt.second = new MCFlat();
            return uttt.RunGame2NTimes(5);
        }
        static void Run4Threads(Func<(int,int)> f)
        {
            var t1 = Task.Run(f);
            var t2 = Task.Run(f);
            var t3 = Task.Run(f);
            var t4 = Task.Run(f);
            t1.Wait();
            t2.Wait();
            t3.Wait();
            t4.Wait();
            Console.WriteLine($"first won: {t1.Result.Item1 + t2.Result.Item1 + t3.Result.Item1 + t4.Result.Item1}");
            Console.WriteLine($"second won: {t1.Result.Item2 + t2.Result.Item2 + t3.Result.Item2 + t4.Result.Item2}");
        }
    }
}
