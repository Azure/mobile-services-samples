using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicTacToeLibraryCSharp
{
    public class MiniMax
    {
        // Finds all available moves.
        // A value of 0 means that the move is not available; 1 means that the cell is empty and a move is available.
        public static int[] AvailableMoves(string board, char empty)
        {
            int[] result = new int[board.Length];
            for (int i = 0; i < board.Length; i++)
            {
                result[i] = (board[i] == empty) ? 1 : 0;
            }
            return result;
        }

        public static char Winner(string board, char player, char opponent, char empty)
        {
                /*
                  |0|1|2|
                  |3|4|5|
                  |6|7|8|
                 */
            int[][] permutations = new int[][]
                { // Horizontals
                  new int[] {0, 1, 2} ,
                  new int[] {3, 4, 5} ,
                  new int[] {6, 7, 8} ,

                  // Verticals
                  new int[] {0, 3, 6} ,
                  new int[] {1, 4, 7} ,
                  new int[] {2, 5, 8} ,

                  // Diagonals
                  new int[] {0, 4, 8} ,
                  new int[] {2, 4, 6} };

            foreach (var permutation in permutations)
            {
                int count = 0;
                for (int index = 0; index < permutation.Length; index++)
                {
                    int boardIndex = permutation[index];
                    if (board[boardIndex] == player)
                    {
                        count++;
                    }
                }
                if (count == permutation.Length)
                {
                    return player;
                }

                count = 0;
                for (int index= 0; index < permutation.Length; index++)
                {
                    int boardIndex = permutation[index];
                    if (board[boardIndex] == opponent)
                    {
                        count++;
                    }
                }
                if (count == permutation.Length)
                {
                    return opponent;
                }
            }
            return empty;
        }

    public static bool Terminal(string board, char player, char opponent, char empty)
    {
        var moves = AvailableMoves(board, empty);
        bool moveAvailable = false;
        foreach(var move in moves)
        {
            if (move == 1)
            {
                moveAvailable = true;
            }
        }
        return moveAvailable == false || Winner(board, player, opponent, empty) != empty;
    }

    public delegate void CallBackFn(int iterCount);

    // Serial version of the minimax algorithm.
    public async static Task<Tuple<int, int>> Minimax(string board, char player, char opponent, char empty, char max, int iterCount, CallBackFn iterCallback)
    {
        ++iterCount;
        iterCallback(iterCount);
        var space = new Tuple<int, int> (-1, 0);
        var w = Winner(board, player, opponent, empty);
        if (w == empty)
        {
            space = new Tuple<int, int>(space.Item1, 0);
        }
        else if (w == max) 
        {
            space = new Tuple<int, int>(space.Item1, 1);
        }
        else
        {
            space = new Tuple<int, int>(space.Item1, -1);
        }
        if (Terminal(board, player, opponent, empty))
        {
            return space;
        }

        bool first = true;
        Tuple<int, int> best = new Tuple<int, int>(0, 0);
        var moves = AvailableMoves(board, empty);
        for (int i = 0; i < moves.Length; i++)
        {
            if (moves[i] != 0)
            {
                StringBuilder new_board = new StringBuilder(board);
                new_board[i] = player;
                Tuple<int, int> temp = await MiniMax.Minimax(new_board.ToString(), opponent, player, empty, max, iterCount, iterCallback);
                temp = new Tuple<int, int>(i, temp.Item2);

                // If the new move is better than our previous move, take it.
                if (first || (player == max && temp.Item2 > best.Item2) || (player != max && temp.Item2 < best.Item2))
                { 
                    first = false;
                    best = temp; 
                }
            }
        }
        return best;
        }

    }
}
