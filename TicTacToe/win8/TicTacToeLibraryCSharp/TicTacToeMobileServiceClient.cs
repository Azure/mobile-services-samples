using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Windows.Networking.PushNotifications;

namespace TicTacToeLibraryCSharp
{
    /// <summary>
    /// Represents game information from the mobile service custom API getgamesforuser.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class GameInfo
    {
        [JsonProperty("id", Required = Required.Always)]
        public string GameId;

        [JsonProperty("User1", Required = Required.Always)]
        public string User1Key;

        [JsonProperty("User2", Required = Required.Always)]
        public string User2Key;

        [JsonProperty("Board", Required = Required.AllowNull)]
        public string GameBoard
        { get; set; }

        [JsonProperty("GameResult", Required = Required.AllowNull, NullValueHandling = NullValueHandling.Ignore)]
        public GameState GameResult;

        [JsonProperty("CurrentPlayerIndex", Required = Required.AllowNull, NullValueHandling = NullValueHandling.Ignore)]
        public int CurrentPlayerIndex
        { get; set; }

        [JsonProperty("User1Name", Required = Required.Always)]
        public string User1Name
        { get; set; }

        [JsonProperty("User2Name", Required = Required.Always)]
        public string User2Name
        { get; set; }
    }

    /// <summary>
    /// Represents a game: maps onto the database table "games".
    /// </summary>
    [DataTable("Games")]
    public class Game
    {
        public string id;
        public string User1;
        public string User2;
        public string Board;
        public int GameResult;
        public int CurrentPlayerIndex;
        public string Winner;
    }

    /// <summary>   
    /// Represents a move: maps onto the database table "moves".
    /// </summary>
    [DataTable("Moves")]
    public class Move
    {
        public string id;
        public string UserId;
        public string GameId;
        public string NewBoard;
        public int GameResult;
        public int CurrentPlayerIndex;
        public string Winner;
    }

    /// <summary>
    /// Represents a user: maps onto the database table "users".
    /// </summary>
    [DataTable("Users")]
    public class User
    {
        public string id;
        public string UserId;
        public string UserName;

        public User()
        { }

        public User(string idIn, string name)
        {
            id = idIn;
            UserName = name;
            UserId = null;
        }
    }

    /// <summary>
    ///  Represents a channel for mobile services communication. Maps onto the
    ///  databsae table "channels".
    /// </summary>
    [DataTable("Channels")]
    public class Channel
    {
        public string id;
        public string channelUri;
        public string installationId;
    }

    /// <summary>
    /// Represents a link between users and channels. Maps onto the database table "userchannels".
    /// </summary>
    [DataTable("Userchannels")]
    public class UserChannel
    {
        public string id;
        public string ChannelId;
        public string UserKey;
    }

    /// <summary>
    /// Represents a link between users and friends. Maps onto the database table "userfriends".
    /// </summary>
    [DataTable("Userfriends")]
    public class UserFriend
    {
        public string id;
        public string User1Key;
        public string User2Key;
        public string User2Name;
    }

    /// <summary>
    /// Handles communication with the mobile service.
    /// </summary>
    public class TicTacToeMobileServiceClient
    {
        private MobileServiceClient client;
        private MobileServiceUser mobileServiceUser;
        private User userInfo;
        private PushNotificationChannel channel;


        public TicTacToeMobileServiceClient(MobileServiceClient clientIn, PushNotificationChannel channelIn, MobileServiceUser userIn)
        {
            client = clientIn;
            mobileServiceUser = userIn;
            channel = channelIn;
            userInfo = new User();
        }

        /// <summary>
        /// Create a game in the mobile service.
        /// </summary>
        /// <param name="userKey1">The id of the first user.</param>
        /// <param name="userKey2">The id of the second user.</param>
        /// <param name="game">The local game object to create in the mobile service.</param>
        /// <returns>The id of the created game.</returns>
        public async Task<string> CreateGame(string userKey1, string userKey2, ITurnGame game)
        {
            Game gameObj = new Game
            {
                User1 = userKey1,
                User2 = userKey2,
                Board = game.GameBoard,
                GameResult = (int)game.Result,
                CurrentPlayerIndex = game.CurrentPlayerIndex,
                Winner = ""
            };
            // Insert a new entry in the "games" table
            await client.GetTable<Game>().InsertAsync(gameObj);
           
            // Return the gameID
            return (string) gameObj.id;
        }

        public async Task<Game> GetGame(string gameId)
        {
            return await client.GetTable<Game>().LookupAsync(gameId);
        }
        
        /// <summary>
        /// Submit a move to record it in the mobile service.
        /// </summary>
        /// <param name="userId">The player making the move.</param>
        /// <param name="game">The game object which has the move details.</param>
        public async void MakeMove(string userId, ITurnGame game)
        {
            var moveObj = new Move
            {
                UserId = game.UserPlayer.ID,
                GameId = game.ID,
                NewBoard = game.GameBoard,
                GameResult = (int)game.Result,
                CurrentPlayerIndex = game.CurrentPlayerIndex,
            };
            if (game.Winner != null)
                moveObj.Winner = game.Winner.UserName;
            else
                moveObj.Winner = "";

            // Create a new entry in the moves table
            // (this also sends a push notification to the other user)
            Dictionary<string, string> parameter = new Dictionary<string, string>();
            parameter.Add("OpponentUserId", game.OpponentPlayer.ID.ToString() );

            await client.GetTable<Move>().InsertAsync(moveObj, parameter);

            // Update the games table with the latest board.
            var gameRecord = new JObject();
            gameRecord.Add("id", game.ID);
            gameRecord.Add("Board", game.GameBoard);
            gameRecord.Add("GameResult", (int)game.Result);
            gameRecord.Add("CurrentPlayerIndex", game.CurrentPlayerIndex);
            if (game.Winner != null)
                gameRecord.Add("Winner", game.Winner.UserName);
            else
                gameRecord.Add("Winner", "");
            await client.GetTable<Game>().UpdateAsync(gameRecord);
        }

        /// <summary>
        /// Get the user name for the current user and set the property for reading.
        /// </summary>
        /// <returns></returns>
        public async Task AwaitUserName()
        {
            //List<User> users = await client.GetTable<User>()
            //    .Where((item) => item.UserId == mobileServiceUser.UserId)
            //    .ToListAsync();

            var x = client.GetTable<User>().Where((item) => item.UserId == mobileServiceUser.UserId);
            
            List<User> users = await x.ToListAsync();

            if (users.Count == 1)
            {
                userInfo.UserName = users[0].UserName;
                userInfo.id = users[0].id;
                // not needed in .NET version since no channels table exists.
                //await this.LoginUser();
            }
            else if (users.Count > 1)
            {
                // UserId has multiple usernames. Not supported.
                throw new NotImplementedException();
            }
            else
            {
                userInfo.UserName = null;
                userInfo.id = "-1";
            }
        }

        public User UserInfo
        {
            get { return userInfo; }
        }

        /// <summary>
        ///  Search the user table for a name that contains the given string.
        /// </summary>
        /// <param name="userSearchString">The search string.</param>
        /// <returns>A list of search results as user information.</returns>
        public async Task<List<User>> SearchUsers(string userSearchString)
        {
            return await client.GetTable<User>()
                .Where((item) => item.UserName.Contains(userSearchString))
                .ToListAsync();

        }


        /// <summary>
        /// Add a new user to the users table in the mobile service.
        /// </summary>
        /// <param name="userName">The username of the new user.</param>
        /// <returns></returns>
        public async Task AddUser(string userName)
        {
            User userObj = new User
            {
                UserId = mobileServiceUser.UserId,
                UserName = userName
            };

            await client.GetTable<User>().InsertAsync(userObj);

        }

        /// <summary>
        /// When a user logs in, record their UserId and ChannelId so that they are associated.
        /// This lets us identify the channel to send the push notification to a specific user.
        /// </summary>
        /// <returns></returns>
        public async Task LoginUser()
        {
            // Get the channelId
            var channels = await client.GetTable<Channel>()
                .Where(item => item.channelUri == channel.Uri)
                .ToListAsync();

            UserChannel userChannel = new UserChannel
            {
                ChannelId = channels[0].id,
                UserKey = userInfo.id
            };
            await client.GetTable<UserChannel>().InsertAsync(userChannel);
        }

        /// <summary>
        /// Add a friend for a user. The friend relationship is required in order to 
        /// initiate a game.
        /// </summary>
        /// <param name="key1"></param>
        /// <param name="user2"></param>
        /// <returns></returns>
        public async Task AddUserFriend(string key1, User user2)
        {
            UserFriend userFriend = new UserFriend { User1Key = key1, User2Key = user2.id, User2Name = user2.UserName };
            await client.GetTable<UserFriend>().InsertAsync(userFriend);
        }

        /// <summary>
        /// Get friend list to populate the friends dropdown.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public async Task<List<UserFriend>> GetFriends(string key)
        {
            return await client.GetTable<UserFriend>()
                .Where(item => item.User1Key == key)
                .ToListAsync();
        }

        /// <summary>
        /// Get all games in which the user is a player.
        /// </summary>
        /// <param name="userKey">The id of the player.</param>
        /// <returns></returns>
        public async Task<List<GameInfo>> GetAllMyGames(string userKey)
        {
            // This one should call a custom API. But first, make sure there is data in the table.
            // If not, short circuit since the custom API depends on the schema having been created.
            // This should short-circuit only when the user has just installed the sample and there
            // isn't yet any data or schema in the database.
            var result1 = await client.GetTable<Game>().Take(1).ToListAsync();
            if (result1.Count == 0)
                return new List<GameInfo>();
            
            // Asynchronously call the custom API using the GET method. 
            var result2 = await this.client
                .InvokeApiAsync<List<GameInfo>>("getgamesforuser", System.Net.Http.HttpMethod.Get,
                new Dictionary<string, string> { {"UserKey", userKey.ToString()} });

            return result2;
        }

    }
}
