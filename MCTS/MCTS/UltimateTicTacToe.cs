using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.ComTypes;
using System.Threading.Tasks;
namespace MCTS
{
    class UltimateTicTacToe
    {
        private int firstWins;
        private int secondWins;
        private int draws;
        public GameBoardState state;
        public IUTTTPlayer first;
        public IUTTTPlayer second;
        private IUTTTPlayer current;
        public void RunGame2NTimes(int n)
        {
            for(int i = 0; i < n; i++)
            {
                RunGame(true);
                Console.WriteLine("=================================");
                Console.WriteLine($"Game {i*2 + 1}/{2 * n} Result: {state.gameState}");
                Console.WriteLine("=================================");
                RunGame(false);
                Console.WriteLine("=================================");
                Console.WriteLine($"Game {i*2 + 2}/{2 * n} Result: {state.gameState}");
                Console.WriteLine("=================================");
            }
            Console.WriteLine("=================================");
            Console.WriteLine("first player won  " + firstWins  +" times");
            Console.WriteLine("second player won " + secondWins +" times");
            Console.WriteLine("draws             " + draws);
            Console.WriteLine("=================================");
        }
        public void RunGame(bool firstIsFirst)
        {
            first.Reset();
            second.Reset();
            first.OnStart(firstIsFirst, PositionState.first);
            second.OnStart(!firstIsFirst, PositionState.second);
            state = new GameBoardState(firstIsFirst ? PositionState.first : PositionState.second);
            current = firstIsFirst ? first : second;
            while(true)
            {
                (int, int) move = PassToPlayer(current, 100);
                if(move == (-1,-1))
                {
                    current.OnLose("time out");
                    Console.ForegroundColor = ConsoleColor.Red;
                    string name = first == current ? "first" : "current";
                    Console.WriteLine($"time out by player {name}");
                    Console.ResetColor();
                    firstWins += current == first ? 0 : 1;
                    secondWins += current == second ? 0 : 1;
                    NextPlayer().OnWin();
                    break;
                }
                if(ValidateMove(move))
                {
                    state.ApplyMove(move);
                    if (state.gameState == GameState.running)
                    {
                        first.OnMove(move);
                        second.OnMove(move);
                        current = NextPlayer();
                    }
                    if(state.gameState == GameState.firstWin)
                    {
                        firstWins++;
                        break;
                    }
                    if (state.gameState == GameState.secondWin)
                    {
                        secondWins++;
                        break;
                    }
                    if (state.gameState == GameState.draw)
                    {
                        draws++;
                        break;
                    }
                }
                else
                {
                    current.OnLose("invalid move " + move);
                    Console.ForegroundColor = ConsoleColor.Red;
                    string name = first == current ? "first" : "current";
                    Console.WriteLine($"invalid move by player {name}");
                    Console.ResetColor();
                    firstWins += current == first ? 0 : 1;
                    secondWins += current == second ? 0 : 1;
                    NextPlayer().OnWin();
                    break;
                }
            }
        }
        public IUTTTPlayer NextPlayer()
        {
            return first == current ? second : first;
        }
        public (int,int) PassToPlayer(IUTTTPlayer player, double timeInMilliseconds)
        {
            return player.GetMove(state);
            /*
            var task = Task.Run(() => player.GetMove(state));
            if (task.Wait(TimeSpan.FromMilliseconds(timeInMilliseconds)))
            {
                return task.Result;
            }
            else
            {
                throw new Exception("time out");
            }*/
        }
        bool ValidateMove((int,int) move)
        {
            return state.LegalMoves().Contains(move);
        }
        public void Reset()
        {
            firstWins = 0;
            secondWins = 0;
            draws = 0;
        }
    }
    public interface IUTTTPlayer
    {
        (int,int) GetMove(GameBoardState state);
        void OnLose(string msgWhy);
        void OnWin();
        void Reset();
        void OnMove((int, int) move);
        void OnStart(bool isFirst, PositionState player);
    }
    public enum PositionState
    {
        empty,
        first,
        second
    }
    public enum GameState
    {
        running,
        firstWin,
        secondWin,
        draw
    }
    public struct GameBoardState
    {
        public PositionState[,] boardState;
        public GameState[,] boardBoardState;
        public PositionState turn;
        public (int, int) lastMove;
        public GameState gameState;

        public GameBoardState(PositionState turn)
        {
            this.boardState = new PositionState[9, 9];
            this.boardBoardState = new GameState[3, 3];
            this.turn = turn;
            this.lastMove = (-1, -1);
            gameState = GameState.running;
        }

        public GameBoardState(GameBoardState other)
        {
            this.boardState = (PositionState[,])other.boardState.Clone();
            this.boardBoardState = (GameState[,])other.boardBoardState.Clone();
            this.turn = other.turn;
            this.lastMove = other.lastMove;
            gameState = other.gameState;
        }

        public GameState ApplyMove((int, int) move)
        {
            boardState[move.Item1, move.Item2] = turn;
            lastMove = move;
            turn = turn == PositionState.first ? PositionState.second : PositionState.first;
            UpdateBoardState();
            return GetGameState();
        }
        public List<(int, int)> LegalMoves()
        {
            int rx = (lastMove.Item1 % 3);
            int ry = (lastMove.Item2 % 3);
            List<(int, int)> legalMoves = new List<(int, int)>();
            if (rx != -1 && boardBoardState[rx, ry] == GameState.running)
            {
                for (int xi = 0; xi < 3; xi++)
                {
                    for (int yi = 0; yi < 3; yi++)
                    {
                        if (boardState[rx * 3 + xi, ry * 3 + yi] == PositionState.empty)
                        {
                            legalMoves.Add((rx * 3 + xi, ry * 3 + yi));
                        }
                    }
                }
            }
            else
            {
                for (int x = 0; x < 3; x++)
                {
                    for (int y = 0; y < 3; y++)
                    {
                        if (boardBoardState[x, y] == GameState.running)
                        {
                            for (int xi = 0; xi < 3; xi++)
                            {
                                for (int yi = 0; yi < 3; yi++)
                                {
                                    if (boardState[x * 3 + xi, y * 3 + yi] == PositionState.empty)
                                    {
                                        legalMoves.Add((x * 3 + xi, y * 3 + yi));
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return legalMoves;
        }

        public GameState GetGameState()
        {
            int rx = (lastMove.Item1 / 3);
            int ry = (lastMove.Item2 / 3);
            if (boardBoardState[rx, ry] == GameState.running) return GameState.running;
            //columns
            for (int x = 0; x < 3; x++)
            {
                GameState state = boardBoardState[x, 0];
                if (state == GameState.running) continue;
                for (int y = 1; y < 3; y++)
                {
                    if (state != boardBoardState[x, y])
                    {
                        break;
                    }
                    else
                    {
                        if (y == 2)
                        {
                            gameState = state;
                            return state;
                        }
                    }
                }
            }
            //rows
            for (int y = 0; y < 3; y++)
            {
                GameState state = boardBoardState[0, y];
                if (state == GameState.running) continue;
                for (int x = 1; x < 3; x++)
                {
                    if (state != boardBoardState[x, y])
                    {
                        break;
                    }
                    else
                    {
                        if (x == 2)
                        {
                            gameState = state;
                            return state;
                        }
                    }
                }
            }
            //diagonals
            for (int i = 0; i < 2; i++)
            {
                GameState state = boardBoardState[0, 2 * i];
                if (state == GameState.running) continue;
                for (int j = 1; j < 3; j++)
                {
                    if (state != boardBoardState[j, 2 * i + j * (i == 1 ? -1 : 1)])
                    {
                        break;
                    }
                    else
                    {
                        if (j == 2)
                        {
                            gameState = state;
                            return state;
                        }
                    }
                }
            }
            //draw
            int win = 0;
            int lose = 0;
            for (int y = 0; y < 3; y++)
            {
                for (int x = 0; x < 3; x++)
                {
                    switch (boardBoardState[x, y])
                    {
                        case GameState.running:
                            return GameState.running;
                        case GameState.firstWin:
                            win++;
                            break;
                        case GameState.secondWin:
                            lose++;
                            break;
                        default:
                            break;
                    }
                }
            }
            gameState = win > lose ? GameState.firstWin : GameState.secondWin;
            return win > lose ? GameState.firstWin : GameState.secondWin;
        }

        private void UpdateBoardState()
        {
            int rx = (lastMove.Item1 / 3);
            int ry = (lastMove.Item2 / 3);
            int ix = lastMove.Item1;
            int iy = lastMove.Item2;
            //columns
            for (int x = rx * 3; x < rx * 3 + 3; x++)
            {
                PositionState state = boardState[x, ry * 3];
                if (state == PositionState.empty) continue;
                for (int y = ry * 3 + 1; y < ry * 3 + 3; y++)
                {
                    if (state != boardState[x, y])
                    {
                        break;
                    }
                    else
                    {
                        if (y == ry * 3 + 2)
                        {
                            boardBoardState[rx, ry] = state == PositionState.first ? GameState.firstWin : GameState.secondWin;
                            return;
                        }
                    }
                }
            }
            //rows
            for (int y = ry * 3; y < ry * 3 + 3; y++)
            {
                PositionState state = boardState[rx * 3, y];
                if (state == PositionState.empty) continue;
                for (int x = rx * 3 + 1; x < rx * 3 + 3; x++)
                {
                    if (state != boardState[x, y])
                    {
                        break;
                    }
                    else
                    {
                        if (x == rx * 3 + 2)
                        {
                            boardBoardState[rx, ry] = state == PositionState.first ? GameState.firstWin : GameState.secondWin;
                            return;
                        }
                    }
                }
            }
            //diagonals
            for (int i = 0; i < 2; i++)
            {
                PositionState state = boardState[rx * 3, ry * 3 + 2 * i];
                if (state == PositionState.empty) continue;
                for (int j = 1; j < 3; j++)
                {
                    if (state != boardState[rx * 3 + j, ry * 3 + 2 * i + j * (i == 1 ? -1 : 1)])
                    {
                        break;
                    }
                    else
                    {
                        if (j == 2)
                        {
                            boardBoardState[rx, ry] = state == PositionState.first ? GameState.firstWin : GameState.secondWin;
                            return;
                        }
                    }
                }
            }
            //draw
            for (int y = ry * 3; y < ry * 3 + 3; y++)
            {
                for (int x = rx * 3; x < rx * 3 + 3; x++)
                {
                    if (boardState[x, y] == PositionState.empty)
                    {
                        return;
                    }
                }
            }
            boardBoardState[rx, ry] = GameState.draw;
        }
        override public string ToString()
        {
            string output = "";
            for (int x = 0; x < 9; x++)
            {
                for (int y = 0; y < 9; y++)
                {
                    output += boardState[x, y] == PositionState.first ? "O" : boardState[x, y] == PositionState.second ? "X" : " ";
                }
                output += "\n";
            }
            return output;
        }
    }
}
