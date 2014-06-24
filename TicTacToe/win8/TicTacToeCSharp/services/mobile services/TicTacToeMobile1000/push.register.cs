using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices;
using Newtonsoft.Json.Linq;
using Windows.Networking.PushNotifications;
using Windows.UI.Popups;

// http://go.microsoft.com/fwlink/?LinkId=290986&clcid=0x409

namespace TicTacToeCSharp
{
    /// <summary>
    /// Handles push notifications from mobile services.
    /// </summary>
    internal class TicTacToeMobile1000Push
    {

        private static PushNotificationChannel channel;

        public static PushNotificationChannel Channel
        {
            get { return channel; }
        }

        /// <summary>
        /// Registers for push notifications.
        /// </summary>

        public async static Task UploadChannel(MobileServiceUser user)
        {
            channel = await PushNotificationChannelManager.CreatePushNotificationChannelForApplicationAsync();
    
            await App.mobileClient.GetPush().RegisterNativeAsync(channel.Uri , new string[] { user.UserId });

            if (channel != null)
            {
                channel.PushNotificationReceived += OnPushNotificationReceived;
            }
        }

        /// <summary>
        /// Handles push notifications from mobile services.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private static async void OnPushNotificationReceived(PushNotificationChannel sender, PushNotificationReceivedEventArgs args)
        {
            switch(args.NotificationType)
            {
                case PushNotificationType.Toast:
                    // If there is a currently active game, get it and call ProcessRemoteTurn.
                    if (MainPage.GetMainPage().Processor != null)
                    {
                        await MainPage.GetMainPage().Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                            MainPage.GetMainPage().Processor.HandleToastNotification());
                       
                    }
                    break;
                case PushNotificationType.Badge:
                    if (MainPage.GetMainPage() != null)
                    {
                        await MainPage.GetMainPage().Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                            MainPage.GetMainPage().HandleBadgeNotification(args.BadgeNotification));
                        args.Cancel = true;
                    }
                    break;
                case PushNotificationType.Raw:
                    break;

            }
        }

        private static void HandleRegisterException(Exception exception)
        {

        }
    }
}
