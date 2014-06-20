using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;

namespace TicTacToeLibraryCSharp
{
    /// <summary>
    /// Represents the computer when the user chooses to play against the machine.
    /// </summary>
    public sealed class ComputerPlayer : IPlayer
    {
        public ITurnGame Game { get; set; }

        public ComputerPlayer(ITurnGame game, Tile symbol)
        {
            Game = game;
            m_symbol = symbol;
            if (m_symbol == game.Pieces[0])
                m_opponentSymbol = game.Pieces[1];
            else
                m_opponentSymbol = game.Pieces[0];
            m_emptySymbol = game.Tiles[0];
            m_playerType = PlayerType.Computer;

            m_rng = new Random((int)System.DateTime.Now.Ticks);
        }

        public PlayerType Player
        {
            get
            {
                return m_playerType;
            }
        }

        public Tile Symbol
        {
            get
            {
                return m_symbol;
            }
        }

        public string UserName
        {
            get
            {
                return "Computer";
            }
        }

        public int ID
        {
            get { return 0; }
        }
        
        private PlayerType m_playerType;
        private Tile m_symbol;
        private Tile m_opponentSymbol;
        private Tile m_emptySymbol;
        private Random m_rng;

        public async Task<int> ThinkAsync(IList<char> gameBoard)
        {
            // Copy the contents of the IList to a string.
            char[] arrayTmp = new char[9];
            gameBoard.CopyTo(arrayTmp, 0);
            string board = new String(arrayTmp);

            // Frequently take the center square if the board is empty.
            System.Random random = new System.Random((int)System.DateTime.Now.Ticks);
            if (random.Next() % 2 == 0 && gameBoard.All<char>((ch) => ch == m_emptySymbol.Symbol))
            {
                return 4;
            }

            // Approximate counts of iterations the algorithm makes for each number of available moves at the root level.
            uint[] allIterations = { 0u, 2u, 5u, 15u, 50u, 200u, 930u, 7300u, 60000u, 550000u };
            var moves = MiniMax.AvailableMoves(board, m_emptySymbol.Symbol);
            uint totalIterations = allIterations[moves.Length - 1];

            // Report every 1%.
            uint nextReportIter = totalIterations / 100;
            uint iterBy = nextReportIter;

            // This is called on every iteration of the minimax algorithm.
            MiniMax.CallBackFn callback = /*[total_iterations, &next_report_iter, iter_by, reporter]*/(int iter_count) =>
            {
                if (iter_count == nextReportIter)
                {
                    double progress = 100.0 * iter_count / totalIterations;
                    //reporter.report(Math.Min(progress, 100.0));
                    nextReportIter += iterBy;
                }
            };

            // Run the minimax algorithm in parallel if there are enough iterations.
            // Otherwise, run it serially, as the overhead of parallelism may not benefit.
            int iterCount = 0;
            System.Tuple<int, int> t;
            //if (totalIterations > 500)
            //{
            //    t = parallel_minimax(board, m_symbol, m_opponentSymbol, m_emptySymbol, m_symbol, iterCount, callback);
            //}
            //else
            //{
            t = await MiniMax.Minimax(board, m_symbol.Symbol, m_opponentSymbol.Symbol, m_emptySymbol.Symbol, m_symbol.Symbol, iterCount, callback);
            //}

            // Return the index part.
            return t.Item1;
        }

       /* public IAsyncOperationWithProgress<uint, double> ThinkAsyncOld(IList<char> gameBoard)
        {
            return create_async((progress_reporter<double> reporter) =>
            {
                // Copy the contents of the IVector to a std::array.
                char[] arrayTmp;
                gameBoard.CopyTo(arrayTmp, 0);
                string board = new String(arrayTmp);

                // Frequently take the center square if the board is empty.
                System.Random random = new System.Random((int)System.DateTime.Now.Ticks);
                if (random.Next() %2 == 0 && gameBoard.All<char>((ch) => ch == m_emptySymbol))
                {
                    return 4u;
                }

                // Approximate counts of iterations the algorithm makes for each number of available moves at the root level.
                uint[] allIterations = { 0u, 2u, 5u, 15u, 50u, 200u, 930u, 7300u, 60000u, 550000u };
                var moves = MiniMax.AvailableMoves(board, m_emptySymbol);
                uint totalIterations = allIterations[moves.Length - 1];

                // Report every 1%.
                uint nextReportIter = totalIterations / 100; 
                uint iterBy = nextReportIter;

                // This is called on every iteration of the minimax algorithm.
                MiniMax.CallBackFn callback = (ref int iter_count) =>
                {
                    if (iter_count == nextReportIter)
                    {
                        double progress = 100.0 * iter_count / totalIterations;
                        reporter.report(Math.Min(progress, 100.0));
                        nextReportIter += iterBy;
                    }
                };

                // Run the minimax algorithm in parallel if there are enough iterations.
                // Otherwise, run it serially, as the overhead of parallelism may not benefit.
                int iterCount = 0;
                System.Tuple<int, int> t;
                //if (totalIterations > 500)
                //{
                //    t = parallel_minimax(board, m_symbol, m_opponentSymbol, m_emptySymbol, m_symbol, iterCount, callback);
                //}
                //else
                //{
                    t = MiniMax.Minimax(board, m_symbol, m_opponentSymbol, m_emptySymbol, m_symbol, ref iterCount, callback);
                //}

                // Return the index part.
                return t.Item1;
            });
        }*/
    }
}
