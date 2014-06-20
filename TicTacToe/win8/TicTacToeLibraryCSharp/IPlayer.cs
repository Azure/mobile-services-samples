using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicTacToeLibraryCSharp
{
    public enum PlayerType
    {
        Local,
        Remote,
        None
    };

    /// <summary>
    /// Interface type for players, local and remote.
    /// </summary>
    public interface IPlayer
    {
        PlayerType Player
        {
            get;
        }

        Tile Symbol
        {
            get;
        }

        string UserName
        {
            get;
        }

        string ID
        {
            get;
        }

        Task<int> ThinkAsync(IList<char> gameBoard);
    };
}
