using System;
using System.IO;

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;

using Microsoft.WindowsAzure.MobileServices;

namespace ToDoAzure.Droid
{
    [Activity(Label = "Xamarin.Forms and Azure Mobile Services", Icon = "@drawable/icon", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsApplicationActivity
    {
        public ToDoItemManager todoItemManager;
        static MainActivity instance;
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            
            global::Xamarin.Forms.Forms.Init(this, bundle);
            instance = this;          
            CurrentPlatform.Init();

            todoItemManager = new ToDoItemManager();
            App.SetTodoItemManager(todoItemManager);
            
            LoadApplication(new App());
        }
        public static MainActivity DefaultService
        {
            get{return instance;}
        }
    }
}

