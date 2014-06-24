using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml.Media;
using TicTacToeLibraryCSharp.Common;

#pragma warning disable 4014

namespace TicTacToeLibraryCSharp
{
    /// <summary>
    /// Event arguments for the process turn event.
    /// </summary>
    public class ProcessTurnEventArgs : EventArgs
    {
        public int MyTurnGamesCount
        {
            get;
            set;
        }
    }

    /// <summary>
    /// An event handler that is called when a remote player makes a play while the current player has the app open.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    public delegate void ProcessTurnEventHandler(object sender, ProcessTurnEventArgs args);

    /// <summary>
    /// The possible game results.
    /// </summary>
    public enum GameResult
    {
        None,
        Winner,
        Tie
    };
    
    /// <summary>
    /// A value that indicates whether the game has started, and whether it's over or not.
    /// </summary>
    public enum GameState
    {
        NotStarted,
        InProgress,
        Done
    };

    /// <summary>
    /// The base interface for turn-based games.
    /// </summary>
    public interface ITurnGame
    {
        GameState State
        {
            get;
            set;
        }

        GameResult Result
        {
            get;
            set;
        }

        IPlayer Winner
        {
            get;
            set;
        }

        string GameBoard
        {
            get;
            set;
        }

        bool IsMyTurn
        {
            get;
        }

        IPlayer CurrentPlayer
        {
            get;
        }

        IPlayer NextPlayer
        {
            get;
        }

        string CurrentPlayerName
        {
            get;
        }

        string NextPlayerName
        {
            get;
        }

        int CurrentPlayerIndex
        {
            get;
            set;
        }

        IPlayer UserPlayer
        {
            get;
        }

        IPlayer OpponentPlayer
        {
            get;
        }

        Task NextTurn();


        List<IPlayer> Players
        {
            get;
            set;
        }

        void Start();

        bool CheckEndOfGame();

        List<Tile> Tiles
        {
            get;
        }

        List<Tile> Pieces
        {
            get;
        }

        void SetGameBoard(int index, char value);

        string ID
        {
            get;
            set;
        }

        // Used for computer players
        bool IsThinking
        {
            get;
        }

        void InitializeBoard();
    }

    /// <summary>
    /// The base class for board spaces and pieces.
    /// </summary>
    public class Tile
    {
        private static int count = 0;

        public int ID
        {
            get;
            private set;
        }

        public char Symbol
        {
            get;
            private set;
        }

        public Tile(char symbol)
        {
            Symbol = symbol;
            ID = count++;
        }
    }

    /// <summary>
    /// The TicTicToe game.
    /// </summary>
    [Windows.UI.Xaml.Data.Bindable]
    [Windows.Foundation.Metadata.WebHostHidden]
    public class TicTacToeGame : BindableBase, ITurnGame
    {
        static GameProcessor processor;

        List<IPlayer> players;
        int userPlayerIndex;

        Task<int> m_currentThinkOp;

        StringBuilder gameBoard;

        static List<Tile> tiles;
        static List<Tile> pieces;

        public List<Tile> Tiles
        {
            get { return tiles; }
        }
        public List<Tile> Pieces
        {
            get { return pieces; }
        }

        public static char CrossSymbol
        {
            get { return '\u274C'; }
        }

        public static char NoughtSymbol
        {
            get { return '\u2B55'; }
        }

        public static char EmptySymbol
        {
            get { return ' '; }
        }

        static TicTacToeGame()
        {
            tiles = new List<Tile>();
            tiles.Add(new Tile(EmptySymbol));

            pieces = new List<Tile>();
            pieces.Add(new Tile(CrossSymbol));
            pieces.Add(new Tile(NoughtSymbol));
        }

        /// <summary>
        ///  Create a new game.
        /// </summary>
        public TicTacToeGame()
        {

            State = GameState.NotStarted;
            Players = new List<IPlayer>();
            gameBoard = new StringBuilder("         ");

        }

        /// <summary>
        ///  Create the game object for an existing game
        /// </summary>
        /// <param name="gameInfo"></param>
        /// <param name="users"></param>
        /// <param name="userId"></param>
        public TicTacToeGame(GameProcessor processorIn, GameInfo gameInfo, List<User> users, string userId)
        {
            processor = processorIn;
            State = gameInfo.GameResult;
            Players = new List<IPlayer>();
            for (int index = 0; index < users.Count; index++)
            {
                if (String.Equals(users[index].id, userId))
                {
                    Players.Add(new LocalPlayer(this, users[index], TicTacToeGame.pieces[index]));
                    userPlayerIndex = index;
                }
                else
                {
                    Players.Add(new RemotePlayer(this, users[index], TicTacToeGame.pieces[index]));
                }
            }
            gameBoard = new StringBuilder(gameInfo.GameBoard);
            CurrentPlayerIndex = gameInfo.CurrentPlayerIndex;
            ID = gameInfo.GameId;
        }

        /// <summary>
        /// Start the game.
        /// </summary>
        public void Start()
        {
            // The current user always starts the game, so their index is 0.
            userPlayerIndex = 0;
            CurrentPlayerIndex = 0;
            State = GameState.InProgress;
        }


        public GameState State
        {
            get;
            set;
        }

        public GameResult Result
        {
            get;
            set;
        }

        public IPlayer Winner
        {
            get;
            set;
        }

        public bool IsThinking
        {
            get { return m_currentThinkOp != null; }
        }

        public string GameBoard
        {
            get { return gameBoard.ToString(); }
            set { gameBoard = new StringBuilder(value); }
        }

        public bool IsMyTurn
        {
            get { return players[CurrentPlayerIndex] == this.UserPlayer; }
        }

        public IPlayer CurrentPlayer
        {
            get { return players[CurrentPlayerIndex]; }
        }

        public IPlayer NextPlayer
        {
            get { return players[(CurrentPlayerIndex + 1) % players.Count]; }
        }

        public IPlayer OpponentPlayer
        {
            get { return players[(userPlayerIndex + 1) % players.Count]; }

        }

        public string CurrentPlayerName
        {
            get { return players[CurrentPlayerIndex].UserName; }
        }

        public string NextPlayerName
        {
            get { return players[(CurrentPlayerIndex + 1) % players.Count].UserName; }
        }

        public string GameID
        {
            get { return this.ID.ToString(); }
        }

        public int CurrentPlayerIndex
        {
            get;
            set;
        }

        public IPlayer UserPlayer
        {
            get { return players[userPlayerIndex]; }
        }

        public async Task NextTurn()
        {
            CurrentPlayerIndex = (CurrentPlayerIndex + 1) % players.Count;

            m_currentThinkOp = CurrentPlayer.ThinkAsync(new List<char>(this.GameBoard.ToString()));
            int move = await m_currentThinkOp;
            m_currentThinkOp = null;

            // Choose the cell.
            processor.Cells.ElementAt((int)move).Select(null);

        }

        public List<IPlayer> Players
        {
            get { return players; }
            set { players = value; }
        }

        int[] winningPath;
        public int[] WinningPath
        {
            get { return winningPath; }
        }

        public bool FindWinningPath()
        {
            /*
            |0|1|2|
            |3|4|5|
            |6|7|8|
            */
            int[][] permutations = new int[][]
            {  
                // Horizontals
                new int[] {0, 1, 2},
                new int[] {3, 4, 5},
                new int[] {6, 7, 8},

                // Verticals
                new int[] {0, 3, 6},
                new int[] {1, 4, 7},
                new int[] {2, 5, 8},

                // Diagonals
                new int[] {0, 4, 8},
                new int[] {2, 4, 6},
            };

            foreach (int[] permutation in permutations)
            {
                int size = permutation.Length;

                foreach (Tile piece in pieces)
                {
                    int count = 0;
                    for (int index = 0; index < permutation.Length; index++)
                    {
                        int boardIndex = permutation[index];
                        if (this.GameBoard[boardIndex] == piece.Symbol)
                        {
                            count++;
                        }
                    }
                    if (count == permutation.Length)
                    {
                        winningPath = permutation;
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        ///  Implementation for 3x3 board.
        /// </summary>
        /// <returns></returns>
        public bool CheckEndOfGame()
        {
            if (FindWinningPath())
            {
                this.Result = GameResult.Winner;
                this.Winner = this.CurrentPlayer;
                this.CurrentPlayerIndex = -1;
                return true;
            }
            else
            {
                // Check for a tie.
                int count1 = 0;
                for (int index = 0; index < GameBoard.Length; index++)
                {
                    if (GameBoard[index] != ' ')
                        count1++;
                }

                if (count1 == GameBoard.Length)
                {
                    this.Result = GameResult.Tie;
                    this.CurrentPlayerIndex = -1;
                    return true;
                }
                return false;
            }
        }

        public void SetGameBoard(int index, char value)
        {
            gameBoard[index] = value;
        }

        public string ID
        {
            get;
            set;
        }

        public void InitializeBoard()
        {
            // Initialize game board state.
            for (int index = 0; index < GameBoard.Length; index++)
            {
                SetGameBoard(index, EmptySymbol);
            }
        }
        

        public static GameProcessor Processor
        {
            get { return processor; }
        }

    }

    /// <summary>
    /// Handles the functionality of the user interface,
    /// including the property values for the display, the commands
    /// for each button, registration and sign-in management, and
    /// control of games and turns.
    /// </summary>
    [Windows.UI.Xaml.Data.Bindable]
    public sealed class GameProcessor : Common.BindableBase
    {

        // *********************************************** //
        //         Private fields                          //
        // *********************************************** //

        // The index of the selected opponent in the "Choose Opponent" dropdown list.
        int selectedPlayerOption = -1;

        // The index of the selected user in the "search for friends" dropdown list.
        int selectedUserIndex;

        // The index of the selected cell on the board.
        int selectedCellIndex = -1;

        // The index of the selected game in the my turn games list.
        int selectedGameIndex;

        // The user id of the local player.
        string userKey;
        
        // The user name of the local player.
        string userNameText;

        // The text that indicates the user is signed in with a particular username.
        string signedInAsText;

        // The text of the Sign In or Sign Out or Register button, as appropriate.
        string signInButtonText;

        // The text entered in the search for users textbox.
        string userNameSearchText;

        // True if search results exist and should be shown in the UI.
        bool showSearchResults;

        // The list of all games for this user.
        List<ITurnGame> gameList;

        // The list of active games for this user.
        List<GameInfo> activeGamesForUser;

        // The list of games where it's the user's turn.
        ObservableCollection<ITurnGame> myTurnGamesForUser;

        // The list of friends (possible opponents) for this user.
        List<UserFriend> friendList;

        // The list of search results when searching users to add a friend.
        List<User> searchResults;

        // The current game that is being played and is displayed.
        ITurnGame currentGame;

        // The button command for Submit Move button.
        System.Windows.Input.ICommand submitMoveCommand;

        // The mobile service client object.
        TicTacToeMobileServiceClient mobileServiceClient;

        // The game board.
        ObservableCollection<Cell> m_cells;

        // Command object to create new game.
        System.Windows.Input.ICommand m_newGameCommand;

        // Command object to sign in
        System.Windows.Input.ICommand signInCommand;

        // Command object to search online for usernames
        System.Windows.Input.ICommand searchCommand;

        // Command object to add a friend (to play against) from the user search results list.
        System.Windows.Input.ICommand addFriendCommand;

        // True if the user is signed in.
        bool isSignedIn;

        // True if the user has registered.
        bool isRegistered;

        // Flags an active game.
        bool isGameInProgress;

        // Text that's displayed to let the user choose whether each player is human or computer-controlled.
        ObservableCollection<string> playerOptions;

        // Used to define cells' foreground colors.
        // This is for simplicity, but a real app might add a property to the Cell class (maybe IsOnWinningPath) and 
        // use a XAML converter to define the brushes.
        Windows.UI.Xaml.Media.SolidColorBrush defaultButtonForegroundBrush;
        Windows.UI.Xaml.Media.SolidColorBrush winningPathButtonForegroundBrush;

        // Text for last game's result.
        string gameResultString;

        // The event called when an opponent's turn occurs so that their move can be displayed.
        public event ProcessTurnEventHandler OnProcessTurn;



        // *********************************************** //
        // Properties that are bound to the user interface //
        // *********************************************** //

        /// <summary>
        /// The board spaces on the display.
        /// </summary>
        public ObservableCollection<Cell> Cells
        {
            get { return m_cells; }
        }

        /// <summary>
        /// The command for the Start Game button.
        /// </summary>
        public System.Windows.Input.ICommand NewGameCommand
        {
            get { return m_newGameCommand; }
        }

        /// <summary>
        ///  The command for the sign in button.
        /// </summary>
        public System.Windows.Input.ICommand SignInCommand
        {
            get { return signInCommand; }
        }

        /// <summary>
        /// The command for the add friend button.
        /// </summary>
        public System.Windows.Input.ICommand AddFriendCommand
        {
            get { return addFriendCommand; }
        }

        /// <summary>
        ///  The command for the search (for users) button.
        /// </summary>
        public System.Windows.Input.ICommand SearchCommand
        {
            get { return searchCommand; }
        }

        /// <summary>
        /// The command for the submit move button.
        /// </summary>
        public System.Windows.Input.ICommand SubmitMoveCommand
        {
            get { return submitMoveCommand; }
        }

        /// <summary>
        /// The list of possible opponents (friends).
        /// </summary>
        public ObservableCollection<string> PlayerOptions
        {
            get { return playerOptions; }
        }

        /// <summary>
        /// The currently selected opponent in the opponent dropdown.
        /// </summary>
        public int SelectedPlayerOption
        {
            get { return selectedPlayerOption; }
            set {
                // Selecting a player affects whether or not a new game can be started.
                SetProperty(ref selectedPlayerOption, value, "SelectedPlayerOption");
                ((DelegateCommand)NewGameCommand).RaiseCanExecuteChanged();
            }
        }

        /// <summary>
        /// The id of the local user.
        /// </summary>
        public string UserKey
        {
            get { return this.userKey; }
        }
        
        /// <summary>
        /// The local users tile/symbol (X or O)
        /// </summary>
        public char UserSymbol
        {
            get
            {
                if (currentGame != null && currentGame.UserPlayer != null)
                {
                    return currentGame.UserPlayer.Symbol.Symbol;
                }
                else
                {
                    return ' ';
                }
            }
        }

        /// <summary>
        /// The current user's username.
        /// </summary>
        public string UserNameText
        {
            get { return userNameText; }
            set
            {
                SetProperty(ref userNameText, value, "UserNameText");
                if (signInCommand != null)
                {
                    ((DelegateCommand)signInCommand).RaiseCanExecuteChanged();
                }
            }
        }

        /// <summary>
        /// The opponent's username.
        /// </summary>
        public string OpponentName
        {
            get
            {
                if (currentGame != null)
                {
                    return currentGame.NextPlayerName;
                }
                else
                    return "";
            }
        }

        /// <summary>
        /// The opponent's tile/symbol (X or O)
        /// </summary>
        public char OpponentSymbol
        {
            get
            {
                if (currentGame != null)
                {
                    return currentGame.OpponentPlayer.Symbol.Symbol;
                }
                else
                    return ' ';
            }
        }

        /// <summary>
        /// The string to display that indicates that the user is signed in with a particular username.
        /// </summary>
        public string SignedInAsText
        {
            get { return signedInAsText; }
            set { SetProperty(ref signedInAsText, value, "SignedInAsText"); }
        }

        /// <summary>
        /// The string to display on the Sign In/ Sign Out / Register button.
        /// </summary>
        public string SignInButtonText
        {
            get { return signInButtonText; }
            set { SetProperty(ref signInButtonText, value, "SignInButtonText"); }
        }

        /// <summary>
        /// True if an opponent is selected in the opponent drop-down box, meaning
        /// that a game can be started with that opponent.
        /// </summary>
        public bool CanCreateNewGame
        {
            get { return SelectedPlayerOption != -1; }
        }

        /// <summary>
        /// The string entered by the user to search for friends.
        /// </summary>
        public string UserNameSearchText
        {
            get { return userNameSearchText; }
            set { SetProperty(ref userNameSearchText, value, "UserNameSearchText");  }
        }

        /// <summary>
        /// The text to display for the game result (e.g. You win!)
        /// </summary>
        public string GameResultString
        {
            get { return gameResultString; }
        }

        /// <summary>
        /// The result of the current game.
        /// </summary>
        public GameResult GameResultEnum
        {
            get { return currentGame.Result; }
        }

        /// <summary>
        /// The current player in the current game.
        /// </summary>
        public IPlayer CurrentPlayer
        {
            get { return currentGame.CurrentPlayer; }
        }

        /// <summary>
        /// The list of currently active games for this user.
        /// </summary>
        public List<GameInfo> ActiveGames
        {
            get { return activeGamesForUser;  }
        }

        /// <summary>
        /// The list of games where it's the user's turn.
        /// </summary>
        public ObservableCollection<ITurnGame> MyTurnGames
        {
            get { return myTurnGamesForUser; }
        }

        /// <summary>
        /// The current game's game board as a string.
        /// </summary>
        public string CurrentBoard
        {
            get { return currentGame.GameBoard;  }
        }


        public bool IsGameInProgress
        {
            get { return isGameInProgress;  }
            set { SetProperty(ref isGameInProgress, value, "IsGameInProgress"); }
        }
        
        /// <summary>
        ///  True if the user is not signed in and is able to enter a username to sign in.
        /// </summary>
        public bool ShowSignInBox
        {
            get { return !isSignedIn; }
        }

        /// <summary>
        /// True if there are search results for user friends to show in the UI.
        /// </summary>
        public bool ShowSearchResults
        {
            get { return showSearchResults;  }
            set { SetProperty(ref showSearchResults, value, "ShowSearchResults"); }
        }

        /// <summary>
        /// The currently selected game in the My Turns game listbox.
        /// </summary>
        public int SelectedGameIndex
        {
            get { return selectedGameIndex; }
            set { SetProperty(ref selectedGameIndex, value, "SelectedGameIndex"); }
        }

        /// <summary>
        /// True if there is an opponent selected in the opponent dropdown box.
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public bool CanSelectNewGame(object parameter)
        {
            return CanCreateNewGame;
        }

        /// <summary>
        /// True if the user is signed in and entered some search text to search for.
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public bool CanSearch(object parameter)
        {
            return isSignedIn && UserNameSearchText != "";
        }

        /// <summary>
        /// True if the Sign In/ Sign Out/ Register button can be used.
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public bool CanRegisterOrSignInOrOut(object parameter)
        {
            //if (finalizedRegistration == false)
            //{
            //    return false;
            //}
            // If the user is signed in, they can sign out.
            if (isSignedIn)
            {
                return true;
            }
            // If a user is registered but not signed in, they can sign in.
            if (isRegistered)
            {
                return true;
            }
            // If a username is entered but the user is not registered, they can register.
            if (UserNameText != null && UserNameText != "")
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// True if it's the current local user's turn in the currently displayed game.
        /// </summary>
        /// <returns></returns>
        public bool IsUsersTurn()
        {
            return (currentGame != null && currentGame.CurrentPlayer == currentGame.UserPlayer);
        }

        /// <summary>
        /// The mobile service client object.
        /// </summary>
        public TicTacToeMobileServiceClient MobileServiceClient
        {
            get { return mobileServiceClient; }
        }

        /// <summary>
        /// Constructor, called just once per invocation of the app.
        /// </summary>
        /// <param name="mobileServiceClientIn"></param>
        public GameProcessor(TicTacToeMobileServiceClient mobileServiceClientIn)
        {
            mobileServiceClient = mobileServiceClientIn;

            m_cells = new ObservableCollection<Cell>();
            isGameInProgress = false;
            playerOptions = new ObservableCollection<String>();
            defaultButtonForegroundBrush = new SolidColorBrush(Colors.White);
            winningPathButtonForegroundBrush = new SolidColorBrush(Colors.Red);
        
            for (int i = 0; i < 9; i++)
            {
                m_cells.Add(new Cell(i, defaultButtonForegroundBrush, this));
            }

            foreach (Cell cell in m_cells)
            {
                cell.CellSelected += new CellSelectedHandler(this.CellSelected);
            }

            m_newGameCommand = new DelegateCommand(new ExecuteDelegate(this.CreateNewGame), new CanExecuteDelegate(this.CanSelectNewGame));
            signInCommand = new DelegateCommand(new ExecuteDelegate(this.RegisterOrSignInOrOut), new CanExecuteDelegate(this.CanRegisterOrSignInOrOut));
            searchCommand = new DelegateCommand(new ExecuteDelegate(this.SearchUsers), new CanExecuteDelegate(this.CanSearch));
            addFriendCommand = new DelegateCommand(new ExecuteDelegate(this.AddFriend), new CanExecuteDelegate(this.CanAddFriend));
            submitMoveCommand = new DelegateCommand(new ExecuteDelegate(this.SubmitMove), new CanExecuteDelegate(this.CanSubmitMoveCommand));
            // This sections helps the user log in if they have logged in previously by attempting to
            // retrieve credentials from the Password Vault.
            // If no cached credentials exist in the Password Vault, provide a Register button.

            //Task<bool> checkRegistration = mobileServiceClient.IsKnownUser();
            //checkRegistration.Wait();
            //isRegistered = checkRegistration.Result;
            //UserNameText = mobileServiceClient.GetUserName();

            // How do we set "isRegistered" ?
            // How do we set "isSignedIn" ?

            /*Task task = FinalizeRegistration();
            task.Wait();

            if (CanRegisterOrSignInOrOut(null))
            {
                RegisterOrSignInOrOut(null);
            }
            */

            FinalizeSignIn();
        }


        /// <summary>
        /// Create a list of playable TicTacToe games from the basic information from the service.
        /// </summary>
        /// <returns></returns>
        public async Task<List<GameInfo>> GetGamesForUser()
        {
            List<GameInfo> gameInfoList = await mobileServiceClient.GetAllMyGames(this.userKey);
            gameList = new List<ITurnGame>();
            foreach (GameInfo gameInfo in gameInfoList)
            {
                var users = new List<User>();
                User user1, user2;
                user1 = new User(gameInfo.User1Key, gameInfo.User1Name);
                users.Add(user1);
                user2 = new User(gameInfo.User2Key, gameInfo.User2Name);
                users.Add(user2);
                ITurnGame newGame = new TicTacToeGame(this, gameInfo, users, this.UserKey);
                gameList.Add(newGame);

            }

            return gameInfoList;
        }

        /// <summary>
        /// Sign in a user, sign out a user, or register a user.
        /// </summary>
        /// <param name="parameter"></param>
        async void RegisterOrSignInOrOut(object parameter)
        {
            if (isSignedIn)
            {
                // User was signed in. Sign Out.
                SignInButtonText = "Sign In";
                isSignedIn = false;
                SignedInAsText = "";
                UserNameText = "";
                activeGamesForUser.Clear();
                gameList.Clear();
                myTurnGamesForUser.Clear();
                OnPropertyChanged("MyTurnGames");
                friendList.Clear();
                PlayerOptions.Clear();
                OnPropertyChanged("PlayerOptions");
                isGameInProgress = false;
                OnPropertyChanged("IsGameInProgress");
                OnPropertyChanged("ShowSignInBox");
                ResetCells();
                return;
            }
            if (isRegistered)
            {
                // User was registered but not signed in.
                // Check to see if they entered the correct userName.
                if (mobileServiceClient.UserInfo.UserName == userNameText)
                {
                    // Sign them in.
                    isSignedIn = true;
                    SignInButtonText = "Sign Out";
                    SignedInAsText = "Signed in as " + userNameText;
                    PopulateFriendsList();
                    RefreshGamesLists();
                    ResetCells();
                    return;
                }
                else
                {
                    var dialog = new Windows.UI.Popups.MessageDialog("This app only supports one username per Microsoft Account, so you must use the name you originally registered with. To simulate multiplayer play, close the app, restart the app, sign in with a different Microsoft account and register with another username.", "You are already registered as " + mobileServiceClient.UserInfo.UserName + ".");
                    dialog.Commands.Add(new Windows.UI.Popups.UICommand("OK"));
                    await dialog.ShowAsync();
                    return;
                }
            }
            // User was not registered. Check for username in textbox.
            if (UserNameText == null || UserNameText == "")
            {
                SignInButtonText = "Register";
                return;
            }

            // User was not registered and a username was found. Try to register them.
            await mobileServiceClient.AddUser(UserNameText);
            FinalizeSignIn();
            return;
        }

        /// <summary>
        /// Sign in the user, either when they first log in or when they sign in using the
        /// Sign in button.
        /// </summary>
        async void FinalizeSignIn()
        {
 
            await mobileServiceClient.AwaitUserName();

            if (mobileServiceClient.UserInfo.UserName != null)
            {
                UserNameText = mobileServiceClient.UserInfo.UserName;
                userKey = mobileServiceClient.UserInfo.id;
                isSignedIn = true;
                isRegistered = true;
                SignInButtonText = "Sign Out";
                SignedInAsText = "Signed in as " + userNameText;
                OnPropertyChanged("ShowSignInBox");
                PopulateFriendsList();
                RefreshGamesLists();
            }
            else
            {

                UserNameText = null;
                userKey = "-1";
                isSignedIn = false;
                isRegistered = false;
                SignInButtonText = "Register";
            }
            
        }

        public void RefreshGamesLists()
        {
            var task = GetGamesForUser();
            task.ContinueWith(continuation =>
            {
                var games = continuation.Result;
                this.activeGamesForUser = games;

                this.myTurnGamesForUser = new ObservableCollection<ITurnGame>();
                foreach (ITurnGame game in gameList)
                {
                    if (String.Equals(game.CurrentPlayer.ID, userKey))
                    {
                        this.myTurnGamesForUser.Add(game);
                    }
                }

                // This method is useful when you need to invoke a method on the UI thread from
                // a callback or other context.
                Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal,
                    () =>
                    {
                        OnPropertyChanged("ActiveGames");
                        OnPropertyChanged("MyTurnGames");
                    });

            });
        }

        /// <summary>
        /// The results of a search for users.
        /// </summary>
        public List<string> SearchResults
        {
            get
            {
                // TODO: maybe we don't have to recreate this list every time...
                List<string> stringList = new List<string>();
                if (searchResults == null) return null;
                foreach (User user in searchResults)
                {
                    stringList.Add(user.UserName);
                }
                return stringList;
            }
        }

        /// <summary>
        /// Find users matching a particular search string by querying the mobile service.
        /// </summary>
        /// <param name="parameter"></param>
        public async void SearchUsers(object parameter)
        {
            string searchText = parameter as string;
            // Call the mobile service, performing a query on the users table to return a list of all users that
            // match the query.
            // This results in a call to the read method on the "users" table
            searchResults = await mobileServiceClient.SearchUsers(searchText);
            showSearchResults = true;
            OnPropertyChanged("SearchResults");
        }

        /// <summary>
        /// The currently selected user in the search results box.
        /// </summary>
        public int SelectedUser
        {
            get { return selectedUserIndex; }
            set
            {
                SetProperty(ref selectedUserIndex, value, "SelectedUser");
                DelegateCommand d = (DelegateCommand)addFriendCommand;
                d.RaiseCanExecuteChanged();
            }
        }

        /// <summary>
        /// True if a name has been selected in the search results box which the
        /// user could now add as a friend. Used to enable the Add Friend button.
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        bool CanAddFriend(object parameter)
        {
            if (SearchResults == null)
                return false;
            else
                return SearchResults.Count > 0;
        }

        /// <summary>
        /// Add the specified friend to the user's friend list.
        /// </summary>
        /// <param name="parameter"></param>
        async void AddFriend(object parameter)
        {
            // User has selected to add a userName (selected) to their friends list
            // Determine if the friend is already known
            User friend = searchResults[SelectedUser];
            if (friendList != null && friendList.Exists(fi => String.Equals(fi.User1Key, userKey) && String.Equals(fi.User2Key, friend.id)))
            {
                // Friend is already known. Do nothing.
                return;
            }

            // Add the user to the userFriends table
            await mobileServiceClient.AddUserFriend(userKey, friend);
            PopulateFriendsList();
        }

        /// <summary>
        /// Populate the possible opponent's list with the user's friends.
        /// </summary>
        async void PopulateFriendsList()
        {
            friendList = await mobileServiceClient.GetFriends(userKey);
            if (friendList == null)
            {
                friendList = new List<UserFriend>();
            }
            playerOptions.Clear();
            foreach (var friend in friendList)
            {
                playerOptions.Add(friend.User2Name);
            }
        }

        private void ResetCells()
        {
            m_cells.Clear();
            for (int i = 0; i < 9; i++)
            {
                m_cells.Add(new Cell(i, defaultButtonForegroundBrush, this));
            }

            foreach (Cell cell in m_cells)
            {
                cell.CellSelected += new CellSelectedHandler(this.CellSelected);
            }
        }

        private void RedrawCells()
        {
            m_cells.Clear();
            int index = 0;
            foreach (char cellValue in currentGame.GameBoard)
            {
                m_cells.Add(new Cell(index++, defaultButtonForegroundBrush, this, cellValue));
            }

            foreach (Cell cell in m_cells)
            {
                cell.CellSelected += new CellSelectedHandler(this.CellSelected);
            }
        }

        /// <summary>
        /// Create a new game with the chosen opponent.
        /// </summary>
        /// <param name="parameter"></param>
        async void CreateNewGame(object parameter)
        {
            // Update the friend list
            PopulateFriendsList();
            UserFriend opponent = null;
            bool isRemoteUserGame = false;

            currentGame = new TicTacToeGame();
            User user = new User(this.userKey, this.userNameText);
            currentGame.Players.Add(new LocalPlayer(currentGame, user, currentGame.Pieces[0]));


            isRemoteUserGame = true;
            opponent = friendList[SelectedPlayerOption];
            User opponentUserInfo = new User(opponent.User2Key, opponent.User2Name);
            currentGame.Players.Add(new RemotePlayer(currentGame, opponentUserInfo, currentGame.Pieces[1]));

            ResetCells();
           
            currentGame.InitializeBoard();

            // If this game involves a remote user, then create the game in the mobile service.
            if (isRemoteUserGame)
            {
                currentGame.ID = await mobileServiceClient.CreateGame(userKey, opponent.User2Key, currentGame);
            }

            // Hide the game setup area and previous game's result text.
            gameResultString = "";
            OnPropertyChanged("GameResult");
            isGameInProgress = true;
            OnPropertyChanged("IsGameInProgress");
            OnPropertyChanged("CanCreateNewGame");
            OnPropertyChanged("UserSymbol");
            OnPropertyChanged("OpponentName");
            OnPropertyChanged("OpponentSymbol");


            // Reset any selection in the list of games (in case some other game was selected)
            SelectedGameIndex = -1;

            // Player 1 starts.
            currentGame.Start();

            RefreshGamesLists();

        }

        /// <summary>
        /// A cell was selected by the user, so make sure it is filled in with
        /// the user's mark.
        /// </summary>
        /// <param name="sender"></param>
        void CellSelected(Cell sender)
        {
            if (selectedCellIndex != -1)
            {
                m_cells[selectedCellIndex].Clear();
            }
            // Is it the current player's turn?
            if (currentGame.CurrentPlayer == currentGame.UserPlayer)
            {
                selectedCellIndex = sender.Index;
                OnPropertyChanged("SubmitMoveCommand");
                OnPropertyChanged("CanSubmitMoveCommand");
                OnPropertyChanged("CanSubmitMove");
                ((DelegateCommand)submitMoveCommand).RaiseCanExecuteChanged();
            }

        }

        /// <summary>
        /// True if a user has made a valid move and can submit it.
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public bool CanSubmitMoveCommand(object o)
        {
            return selectedCellIndex != -1; 
        }


        /// <summary>
        /// True if the user has made a valid move and can submit it.
        /// </summary>
        public bool CanSubmitMove
        {
            get { return CanSubmitMoveCommand(null); }
        }

        /// <summary>
        /// Submit a move to the mobile service.
        /// </summary>
        /// <param name="o"></param>
        public void SubmitMove(object o)
        {
            ProcessTurn(selectedCellIndex);
        }

        /// <summary>
        /// Sets which game the user wants to play.
        /// </summary>
        /// <param name="selectedGame"></param>
        public void SetCurrentGame(object selectedGame)
        {
            currentGame = (ITurnGame)selectedGame;
            Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    OnPropertyChanged("UserSymbol");
                    OnPropertyChanged("OpponentName");
                    OnPropertyChanged("OpponentSymbol");
                    this.isGameInProgress = true;
                    RedrawCells();

                    selectedCellIndex = -1;
                    ((DelegateCommand)submitMoveCommand).RaiseCanExecuteChanged();
                    OnPropertyChanged("IsGameInProgress");
                });

            
        }

        
        /// <summary>
        /// The user made a valid move, now update everything as needed.
        /// </summary>
        /// <param name="index"></param>
        void ProcessTurn(int index)
        {
            currentGame.SetGameBoard(index, currentGame.CurrentPlayer.Symbol.Symbol);
            PlayerType currentPlayerType = currentGame.CurrentPlayer.Player;
            CheckEndOfGame();           

            bool submitMove = false;

            // If player is not remote (i.e. human interacting)
            // then submit move. 
            if (currentPlayerType == PlayerType.Local)
            {
                submitMove = true;
            }

            // Ensure that MakeMove executes after NextTurn has completed
            // by putting it in a ContinueWith statement.

            Task task = null;
            if (currentGame.Result == GameResult.None)
            {
                task = currentGame.NextTurn();
            }

            if (task != null)
            {
                task.ContinueWith((continuation) =>
                    {
                        if (submitMove)
                            mobileServiceClient.MakeMove(userKey, currentGame);
                    });
            }
            else
            {
                if (submitMove)
                    mobileServiceClient.MakeMove(userKey, currentGame);

            }

            // Now reset the selected move to nothing, and disable the SubmitMove button.
            selectedCellIndex = -1;
            ((DelegateCommand)submitMoveCommand).RaiseCanExecuteChanged();
            var gameToRemove = myTurnGamesForUser.FirstOrDefault(item => String.Equals(item.ID, currentGame.ID) );
            if (gameToRemove != null)
                myTurnGamesForUser.Remove(gameToRemove);
            OnPropertyChanged("MyTurnGames");

            // Update the badge with the number of games
            // or clear any badge notifications.
            OnProcessTurn.BeginInvoke(this, new ProcessTurnEventArgs() { MyTurnGamesCount = this.MyTurnGames.Count }, null, null);
        }

        /// <summary>
        /// A notification was received that an opponent made a play on a game with this user.
        /// It might be the currently active game, or it might not.
        /// </summary>
        public void HandleToastNotification()
        {
            // A push notification was received.
            // That means another player has made a move or finished a game.

            // Cache the old games list since any completed games will not appear in the new list.
            List<ITurnGame> oldGameList = gameList;

            // Refresh the games list (gameList).
            // gameList includes all games that are not completed, for a given user.

            var task = GetGamesForUser();
            task.ContinueWith(continuation =>
                {
                    List<GameInfo> newGames = continuation.Result;

                    // Identify any changed games among the active old games.
                    foreach (GameInfo oldGame in this.activeGamesForUser)
                    {
                        // Try to find this active game (oldGame) in the gameList of changed games.
                        // If so call it the new game. This is usually the same as the old game,
                        // but one where a play has been made will be different.
                        ITurnGame newGame = gameList.Find((item) => item.ID == oldGame.GameId);

                        // If any game is not found in the gameList, it's because it ended and the
                        // query that returns the game List ignores games that are ended.
                        if (newGame == null)
                        {
                            // If the game that ended is the currently displayed one, update all the details
                            // so the opponent's move can be displayed and show the winning path.
                            if (String.Equals(currentGame.ID, oldGame.GameId))
                            {
                                // Try to find this game in the old game List
                                ITurnGame oldCompletedGame = oldGameList.Find((item) => String.Equals(item.ID, oldGame.GameId));

                                if (oldCompletedGame != null)
                                {
                                    // This means a game was completed. It could be a win or a tie.
                                    // Get that specific game from the mobile service since it's not in the new game list,
                                    // and update the currentGame with the updated details.
                                    //Game newCompletedGame = await mobileServiceClient.GetGame(oldCompletedGame.ID);
                                    var task2 = mobileServiceClient.GetGame(oldCompletedGame.ID);
                                    task2.ContinueWith(continuation2 =>
                                        {
                                            Game newCompletedGame = continuation2.Result;
                                            currentGame.GameBoard = newCompletedGame.Board;
                                            Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                                            {
                                                // Reset the display to account for the new move.
                                                RedrawCells();

                                                currentGame.Result = (GameResult)newCompletedGame.GameResult;
                                                currentGame.CurrentPlayerIndex = newCompletedGame.CurrentPlayerIndex;
                                                if (currentGame.Result == GameResult.Winner)
                                                {
                                                    if (newCompletedGame.Winner == currentGame.UserPlayer.UserName)
                                                    {
                                                        currentGame.Winner = currentGame.UserPlayer;
                                                    }
                                                    else
                                                    {
                                                        currentGame.Winner = currentGame.OpponentPlayer;
                                                    }
                                                }

                                                // Locate the winning path in order to display it in red.
                                                ((TicTacToeGame)currentGame).FindWinningPath();

                                                // Trigger the data binding updates to actually cause the display to update.

                                                OnPropertyChanged("CurrentBoard");
                                                OnPropertyChanged("Cells");
                                                OnPropertyChanged("IsUsersTurn");
                                                ProcessEndOfGame();
                                            });
                                        });

                                }
                                else
                                {
                                    throw new InvalidOperationException("Could not find completed game.");
                                }
                            }
                            // Now update the active games list
                            activeGamesForUser = newGames;
                            return;
                        }

                        // Find out if it changed to the user's turn and if so add the game to the my turns list.
                        if (newGame.CurrentPlayerIndex != oldGame.CurrentPlayerIndex && newGame.IsMyTurn)
                        {
                            Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                            {
                                MyTurnGames.Add(newGame);
                                OnPropertyChanged("MyTurnGames");
                            });

                        }

                        // If the game currently being displayed (if any) was updated, make the updated game the current game
                        if (currentGame != null && String.Equals(currentGame.ID, newGame.ID))
                        {
                            SetCurrentGame(newGame);
                            ProcessOpponentTurn();
                        }
                    }
                    // Now update the active games list
                    activeGamesForUser = newGames;
                });

        }
        

        /// <summary>
        ///  This happens when a push notification is received that indicates that an opponent made a move.
        ///  This event triggers all the games to be reloaded from the server.  If the currently viewed game
        ///  is the one that got updated, update the UI.
        /// </summary>
        public void ProcessOpponentTurn()
        {
            // Before updating the ActiveGames list and the MyTurnGames list,
            // detect whether the game being updated is the one currently being viewed.
            // Also update the currentGame to the new currentGame since we 
            // recreated all our games.
            foreach (ITurnGame game in gameList)
            {
                if (String.Equals(game.CurrentPlayer.ID, userKey))
                {
                    // Check for this game in the myTurnGamesForUser list.
                    var foundGame = this.myTurnGamesForUser.FirstOrDefault((item) => String.Equals(item.ID, game.ID));

                    if (foundGame == null)
                    {
                        // The game was not found previously in the my Turns list.
                        // Check to see if it's a new game or one in the (old) activeGames list.
                        if (this.activeGamesForUser.Exists((item) => String.Equals(item.GameId, game.ID)))
                        {
                            // It's in the old list, so it's not a new game.
                            // Now find out if it's the current game and if so,
                            // update the UI.
                            if (String.Equals(game.ID, currentGame.ID))
                            {
                                currentGame = game;
                                Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal,
                                    () =>
                                    {
                                        OnPropertyChanged("CurrentBoard");
                                        OnPropertyChanged("Cells");
                                        OnPropertyChanged("IsUsersTurn");
                                    });
                            }
                        }
                        else
                        {
                            // It's a new game.
                            // It will be added to the activeGames list and the myturns list.
                        }
                    }
                    else
                    {
                        // The game was found in the myTurns List already,
                        // no action necessary.
                    }
                }
            }

            // Now update the active games

            this.myTurnGamesForUser = new ObservableCollection<ITurnGame>();
            foreach (ITurnGame game in gameList)
            {
                if (String.Equals(game.CurrentPlayer.ID, userKey))
                {
                    this.myTurnGamesForUser.Add(game);
                }
                // This seems a bit odd, but replace the current game with the new
                // game that represents the same game.
                if (String.Equals(currentGame.ID, game.ID))
                {
                    currentGame = game;
                }
            }

            // This method is useful when you need to invoke a method on the UI thread from
            // a callback or other context.
            Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal,
                () =>
                {
                    OnPropertyChanged("ActiveGames");
                    OnPropertyChanged("MyTurnGames");
                });

        }

        /// <summary>
        /// Check to see if the game has ended.
        /// </summary>
        void CheckEndOfGame()
        {
            currentGame.Result = GameResult.None;
            int[] path = new int[0];
            if (currentGame.CheckEndOfGame())
            {
                ProcessEndOfGame();

            }
        }

        /// <summary>
        /// If the game has ended, handle appropriate details.
        /// </summary>
        public void ProcessEndOfGame()
        {
            switch (currentGame.Result)
            {
                case GameResult.Winner:
                    if (currentGame.Winner == currentGame.UserPlayer)
                    {
                        gameResultString = "You win!";
                    }
                    else
                    {
                        gameResultString = currentGame.Winner.UserName + "wins!";
                    }
                    foreach (int index in ((TicTacToeGame)currentGame).WinningPath)
                    {
                        m_cells.ElementAt(index).ForegroundBrush = winningPathButtonForegroundBrush;
                    }
                    break;

                case GameResult.Tie:
                    gameResultString = "It's a tie!";
                    break;
            }

            isGameInProgress = false;
            OnPropertyChanged("IsGameInProgress");
            OnPropertyChanged("GameResultString");
            OnPropertyChanged("CanCreateNewGame");
        }
    }
}
