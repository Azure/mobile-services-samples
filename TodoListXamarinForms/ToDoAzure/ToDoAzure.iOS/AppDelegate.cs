
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
            global::Xamarin.Forms.Forms.Init();
            instance = this;
            CurrentPlatform.Init();
            
            todoItemManager = new ToDoItemManager();
            App.SetTodoItemManager(todoItemManager);
            
            LoadApplication(new App());
            return base.FinishedLaunching(app, options);
        }
        public static AppDelegate DefaultService
        {
            get{return instance;}
        }
     }
 
  }
