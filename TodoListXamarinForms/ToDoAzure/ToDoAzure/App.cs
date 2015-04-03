using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Xamarin.Forms;

namespace ToDoAzure
{
    public class App : Application
    {
        public App()
        {
            // The root page of your application
            var tdlx = new ToDoListXaml();
            MainPage = new NavigationPage(tdlx) {};
            
        }

        #region Azure stuff
        static ToDoItemManager todoItemManager;

        public static ToDoItemManager TodoManager
        {
            get { return todoItemManager; }
            set { todoItemManager = value; }
        }

        public static void SetTodoItemManager(ToDoItemManager todoItemManager)
        {
            TodoManager = todoItemManager;
        }
        #endregion

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
