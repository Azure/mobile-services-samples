using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Popups;
using Windows.UI.Notifications;
using Microsoft.WindowsAzure.MobileServices;
using Windows.Data.Xml.Dom;
using TicTacToeLibraryCSharp;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace TicTacToeCSharp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        TicTacToeLibraryCSharp.GameProcessor processor;
        TicTacToeLibraryCSharp.TicTacToeMobileServiceClient mobileServiceClient;
        MobileServiceUser mobileServiceUser;
        TileUpdater tileUpdater;
        BadgeUpdater badgeUpdater;
        //private LiveConnectSession session;
        private static MainPage mainPage;

        public MainPage()
        {
            mainPage = this;
            this.InitializeComponent();
            this.InitializeTiles();
        }

        public static MainPage GetMainPage()
        {
            return mainPage;
        }

        // Initialize the app's tiles on the Start page. There are three different sizes of tiles users can
        // choose from.
        private void InitializeTiles()
        {
            tileUpdater = TileUpdateManager.CreateTileUpdaterForApplication();
            var mediumSquareTile = TileUpdateManager.GetTemplateContent(TileTemplateType.TileSquare150x150PeekImageAndText01);
            var largeSquareTile = TileUpdateManager.GetTemplateContent(TileTemplateType.TileSquare310x310ImageAndText02);
            var wideTile = TileUpdateManager.GetTemplateContent(TileTemplateType.TileWide310x150PeekImage02);

            tileUpdater.EnableNotificationQueue(true);

            mediumSquareTile.GetElementsByTagName("text")[0].InnerText = "TicTacToe";
            largeSquareTile.GetElementsByTagName("text")[0].InnerText = "TicTacToe";
            wideTile.GetElementsByTagName("text")[0].InnerText = "TicTacToe";

            var node1 = wideTile.ImportNode(mediumSquareTile.GetElementsByTagName("binding")[0], true);
            wideTile.GetElementsByTagName("visual")[0].AppendChild(node1);

            var node2 = wideTile.ImportNode(largeSquareTile.GetElementsByTagName("binding")[0], true);
            wideTile.GetElementsByTagName("visual")[0].AppendChild(node2);

            var testNotification = new TileNotification(wideTile);
            testNotification.Tag = "Test";
            tileUpdater.Update(testNotification);

            badgeUpdater = BadgeUpdateManager.CreateBadgeUpdaterForApplication();

        }

        internal void HandleTileNotification(TileNotification tileNotification)
        {
            if (tileUpdater != null)
                tileUpdater.Update(tileNotification);
        }

        internal bool HandleBadgeNotification(BadgeNotification badgeNotification)
        {
            // When this happens, a game has just been played and so the myTurnGames is to be
            // updated by one.
            if (badgeUpdater != null)
            {
                if (processor != null)
                {
                    // Overwrite the badgeNotification
                    var badgeXml = BadgeUpdateManager.GetTemplateContent(BadgeTemplateType.BadgeNumber);
                    var badgeElement = (XmlElement)badgeXml.SelectSingleNode("/badge");
                    badgeElement.SetAttribute("value", (processor.MyTurnGames.Count + 1).ToString());
                    badgeNotification = new BadgeNotification(badgeXml);
                    
                }
                else
                    badgeUpdater.Update(badgeNotification);
                return true;
            }
            return false;
        }


        protected override async void OnNavigatedTo(NavigationEventArgs args)
        {
            await this.Authenticate();
            mobileServiceClient = new TicTacToeLibraryCSharp.TicTacToeMobileServiceClient(App.mobileClient, 
                TicTacToeMobile1000Push.Channel, mobileServiceUser);
            processor = new TicTacToeLibraryCSharp.GameProcessor(mobileServiceClient);
            DataContext = processor;
            processor.OnProcessTurn += this.UpdateBadge;
        }

        public void UpdateBadge(object sender, ProcessTurnEventArgs args)
        {
            if (badgeUpdater != null && args.MyTurnGamesCount == 0)
            {
                badgeUpdater.Clear();
            }
            else
            {
                var badgeXml = BadgeUpdateManager.GetTemplateContent(BadgeTemplateType.BadgeNumber);
                var badgeElement = (XmlElement)badgeXml.SelectSingleNode("/badge");
                badgeElement.SetAttribute("value", args.MyTurnGamesCount.ToString());
                badgeUpdater.Update(new BadgeNotification(badgeXml));
            }
        }

        public TicTacToeLibraryCSharp.GameProcessor Processor
        {
            get { return processor; }
        }

        // Add the following code from
        // http://www.windowsazure.com/en-us/develop/mobile/tutorials/single-sign-on-windows-8-dotnet/



        /// <summary>
        /// Set up Microsoft account (Live ID) authentication.
        /// </summary>
        /// <returns></returns>
        private async System.Threading.Tasks.Task Authenticate()
        {
            /*
            // Enable this code if you want to use client-side authentication.
            // You'll need Live SDK 5.2 or later.
            // Remove the existing call to LoginAsync.
            
            LiveAuthClient liveIdClient = new LiveAuthClient("http://tictactoemobile1000.azure-mobile.net");


            while (session == null)
            {
                // Force a logout to make it easier to test with multiple Microsoft Accounts
                if (liveIdClient.CanLogout)
                    liveIdClient.Logout();


                LiveLoginResult result = await liveIdClient.LoginAsync(new[] { "wl.basic" });
                if (result.Status == LiveConnectSessionStatus.Connected)
                {
                    session = result.Session;
                    LiveConnectClient client = new LiveConnectClient(result.Session);
                    LiveOperationResult meResult = await client.GetAsync("me");
                    mobileServiceUser = await App.TicTacToeMobile1000Client
                        .LoginWithMicrosoftAccountAsync(result.Session.AuthenticationToken);
                    

                    string title = string.Format("Welcome {0}!", meResult.Result["first_name"]);
                    var dialog = new MessageDialog("You are now logged in.", title);
                    dialog.Commands.Add(new UICommand("OK"));
                    await dialog.ShowAsync();
                }
                else
                {
                    session = null;
                    var dialog = new MessageDialog("You must log in.", "Login Required");
                    dialog.Commands.Add(new UICommand("OK"));
                    await dialog.ShowAsync();
                }
            }*/

            // If you want Single-Sign On, enable this version of the call.
            //mobileServiceUser = await App.mobileClient
            //            .LoginAsync(MobileServiceAuthenticationProvider.MicrosoftAccount, true);
            
            System.Threading.Tasks.Task<MobileServiceUser> t1 = App.mobileClient
                        .LoginAsync(MobileServiceAuthenticationProvider.MicrosoftAccount);
            await t1.ContinueWith(cont =>
                {
                    mobileServiceUser = t1.Result;
                    System.Threading.Tasks.Task t = TicTacToeCSharp.TicTacToeMobile1000Push.UploadChannel(mobileServiceUser);
                    t.Wait();
                });
            

        }

        /// <summary>
        /// Changes the currently active game when the user selects from the list of active games.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void myTurnGames_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            IList<object> list = e.AddedItems;
            if (list.Count > 0)
                processor.SetCurrentGame(list.First<object>());

        }



    }
}
