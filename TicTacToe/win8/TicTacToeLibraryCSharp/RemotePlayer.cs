using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicTacToeLibraryCSharp
{
    /// <summary>
    ///  Represents a player on another machine.
    /// </summary>
    class RemotePlayer : IPlayer
    {
        public ITurnGame Game { get; set; }

        public RemotePlayer(ITurnGame game, User userInfo, Tile symbol )
        {
            Game = game;
            Symbol = symbol;
            Player = PlayerType.Remote;
            UserInfo = userInfo;
        }

        public PlayerType Player { get; set; }

        public Tile Symbol { get; set; }

        public User UserInfo { get; set; }

        public string UserName
        {
            get { return UserInfo.UserName;  }
        }

        public string ID
        {
            get { return UserInfo.id; }
        }

        public Task<int> ThinkAsync(IList<char> gameBoard)
        {
            throw new NotImplementedException();

            // Communicate the move to the Mobile Service
            // which then has to store the move and then send a push notification
            // Generate a task object that waits for the other user's play.
        }
    }
}
