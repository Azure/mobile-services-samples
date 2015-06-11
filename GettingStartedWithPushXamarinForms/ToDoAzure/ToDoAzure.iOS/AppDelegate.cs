
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

using Foundation;
using UIKit;

using Xamarin.Forms;
using System.IO;
using Microsoft.WindowsAzure.MobileServices;

namespace ToDoAzure.iOS
{
    [Register("AppDelegate")]
    public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
    {
        public ToDoItemManager todoItemManager;
        static AppDelegate instance;


        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
             // registers for push for iOS8
            var settings = UIUserNotificationSettings.GetSettingsForTypes(
                UIUserNotificationType.Alert
                | UIUserNotificationType.Badge
                | UIUserNotificationType.Sound,
                new NSSet());

            global::Xamarin.Forms.Forms.Init();
            instance = this;
            CurrentPlatform.Init();
            
            todoItemManager = new ToDoItemManager();
            App.SetTodoItemManager(todoItemManager);


            UIApplication.SharedApplication.RegisterUserNotificationSettings(settings);
            UIApplication.SharedApplication.RegisterForRemoteNotifications();
            
            LoadApplication(new App());
            return base.FinishedLaunching(app, options);
        }

        public override void RegisteredForRemoteNotifications(UIApplication application, NSData deviceToken)
        {
            // Modify device token
            string _deviceToken = deviceToken.Description;
            _deviceToken = _deviceToken.Trim('<', '>').Replace(" ", "");

            // Get Mobile Services client
            MobileServiceClient client = todoItemManager.GetClient;

            // Register for push with Mobile Services
            IEnumerable<string> tag = new List<string>() { "uniqueTag" };

            const string template = "{\"aps\":{\"alert\":\"$(message)\"}}";

            var expiryDate = DateTime.Now.AddDays(90).ToString
                (System.Globalization.CultureInfo.CreateSpecificCulture("en-US"));

            var push = client.GetPush();

            push.RegisterTemplateAsync(_deviceToken, template, expiryDate, "myTemplate", tag)
        }
        public override void ReceivedRemoteNotification(UIApplication application, NSDictionary userInfo)
        {
            NSObject inAppMessage;

            bool success = userInfo.TryGetValue(new NSString("inAppMessage"), out inAppMessage);

            if (success)
            {
                var alert = new UIAlertView("Got push notification", inAppMessage.ToString(), null, "OK", null);
                alert.Show();
            }
        }
        public static AppDelegate DefaultService
        {
            get{return instance;}
        }
     }
 
  }
