using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicTacToeLibraryCSharp
{
    class LocalPlayer : IPlayer
    {
        public ITurnGame Game { get; set; }

        public LocalPlayer(ITurnGame game, User user, Tile symbol)
        {
            Game = game;
            Symbol = symbol;
            Player = PlayerType.Local;
            UserName = user.UserName;
            ID = user.id;
        }

        public PlayerType Player { get; set; }

        public Tile Symbol { get; set; }

        public string UserName { get; set; }

        public string ID { get; set; }

        public Task<int> ThinkAsync(IList<char> gameBoard)
        {
            throw new NotImplementedException();
        }

    }

    

    
}
