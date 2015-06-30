using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

using System.Net.Http;

// To add offline sync support, add the NuGet package Microsoft.WindowsAzure.MobileServices.SQLiteStore
// to your project. Then, uncomment the lines marked // offline sync
// For more information, see: http://aka.ms/addofflinesync
//using Microsoft.WindowsAzure.MobileServices.SQLiteStore;  // offline sync
//using Microsoft.WindowsAzure.MobileServices.Sync;         // offline sync

namespace CustomAuthClient
{
    sealed partial class MainPage: Page
    {
        private MobileServiceCollection<TodoItem, TodoItem> items;
        private IMobileServiceTable<TodoItem> todoTable = App.MobileService.GetTable<TodoItem>();
        //private IMobileServiceSyncTable<TodoItem> todoTable = App.MobileService.GetSyncTable<TodoItem>(); // offline sync

        #region custom_auth_fields
        // ******************Custom auth registration inputs***********************
        // 
        // These inputs are hard-coded for sample convienence. In a real-world app
        // you must get these from the user entered in the UI.
        //
        // ************************************************************************
        private const string testUsername = "KarlJB";
        private const string testPassword = "Secure@Password1";
        private const string testEmail = "karl.jablonski@nwtraders.com";
        private const string testFriendlyName = "Karl Jablonski";

        #endregion

        public MainPage()
        {
            this.InitializeComponent();

            // Set the dummy credentials--remove this for a real app.
            txtUsername.Text = testUsername;
            txtPassword.Password = testPassword; // Never hard-code passwords!
        }

        private async Task InsertTodoItem(TodoItem todoItem)
        {
            // This code inserts a new TodoItem into the database. When the operation completes
            // and Mobile Services has assigned an Id, the item is added to the CollectionView
            await todoTable.InsertAsync(todoItem);
            items.Add(todoItem);

            //await SyncAsync(); // offline sync
        }

        private async Task RefreshTodoItems()
        {
            MobileServiceInvalidOperationException exception = null;
            try
            {
                // This code refreshes the entries in the list view by querying the TodoItems table.
                // The query excludes completed TodoItems
                items = await todoTable
                    .Where(todoItem => todoItem.Complete == false)
                    .ToCollectionAsync();
            }
            catch (MobileServiceInvalidOperationException e)
            {
                exception = e;
            }

            if (exception != null)
            {
                await new MessageDialog(exception.Message, "Error loading items").ShowAsync();
            }
            else
            {
                ListItems.ItemsSource = items;
                this.ButtonSave.IsEnabled = true;
            }
        }

        private async Task UpdateCheckedTodoItem(TodoItem item)
        {
            // This code takes a freshly completed TodoItem and updates the database. When the MobileService 
            // responds, the item is removed from the list 
            await todoTable.UpdateAsync(item);
            items.Remove(item);
            ListItems.Focus(Windows.UI.Xaml.FocusState.Unfocused);

            //await SyncAsync(); // offline sync
        }

        private async void ButtonRefresh_Click(object sender, RoutedEventArgs e)
        {
            ButtonRefresh.IsEnabled = false;

            //await SyncAsync(); // offline sync
            await RefreshTodoItems();

            ButtonRefresh.IsEnabled = true;
        }

        private async void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            var todoItem = new TodoItem { Text = TextInput.Text };
            await InsertTodoItem(todoItem);
        }

        private async void CheckBoxComplete_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox cb = (CheckBox)sender;
            TodoItem item = cb.DataContext as TodoItem;
            await UpdateCheckedTodoItem(item);
        }

        private async void ButtonCustomLogin_Click(object sender, RoutedEventArgs e)
        {

            if (string.IsNullOrEmpty(txtUsername.Text)
                || string.IsNullOrEmpty(txtPassword.Password))
            {
                return;
            }

            this.LoginProgress.IsActive = true;
            this.ButtonCancelLogin.IsEnabled = false;
            this.ButtonCustomLogin.IsEnabled = false;

            try
            {
                // Sign-in and set the returned user on the context,
                // then load data from the mobile service.
                App.MobileService.CurrentUser = 
                    await AuthenticateAsync(this.txtUsername.Text, txtPassword.Password);
                await RefreshTodoItems();
                this.GridLoginDialog.Visibility = Visibility.Collapsed;
                this.ButtonRefresh.IsEnabled = true;
            }
            catch (MobileServiceInvalidOperationException ex)
            {
                loginMessage.Text = ex.Message;
            }
            finally
            {
                this.LoginProgress.IsActive = false;
                this.ButtonCancelLogin.IsEnabled = true;
                this.ButtonCustomLogin.IsEnabled = true;
            }
        }

        private void ButtonCancelLogin_Click(object sender, RoutedEventArgs e)
        {
            // Hide the sign-in grid and reshow the login button.
            this.GridLoginDialog.Visibility = Visibility.Collapsed;
            this.ButtonLogin.Visibility = Visibility.Visible;
        }

        private async Task<MobileServiceUser> AuthenticateAsync(string username,
    string password)
        {

            // Call the CustomLogin API and set the returned MobileServiceUser
            // as the current user.
            var user = await App.MobileService
                .InvokeApiAsync<LoginRequest, MobileServiceUser>(
                "CustomLogin", new LoginRequest()
                {
                    UserName = testUsername,
                    Password = testPassword
                });

            return user;
        }

        private void ButtonLogin_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {         
            // Show the sign-in grid and hide the login button.
            this.GridLoginDialog.Visibility = Visibility.Visible;
            this.ButtonLogin.Visibility = Visibility.Collapsed;
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            //await InitLocalStoreAsync(); // offline sync
            //await RefreshTodoItems();

            //// Registration is only required once; you will need to use local 
            //// to rember when a give user has already been authenticated.
            await RegisterUser();

        }

        private async Task RegisterUser()
        {
            try
            {
                // Make sure that the user is registered, using the hard-coded
                // dummy registration credentials. In a real app, you must get these at runtime.
                var response = await App.MobileService
                    .InvokeApiAsync<RegistrationRequest, string>(
                        "customregistration", new RegistrationRequest()
                        {
                            Username = testUsername,
                            Password = testPassword,
                            Email = testEmail,
                            FriendlyName = testFriendlyName
                        });
            }
            catch(MobileServiceInvalidOperationException ex)
            {
                // We expect an error when the registration alreay exists.
                if (ex.Message.Contains("That username already exists.")) { return; }

                throw ex;
            }
        }

        #region Offline sync

        //private async Task InitLocalStoreAsync()
        //{
        //    if (!App.MobileService.SyncContext.IsInitialized)
        //    {
        //        var store = new MobileServiceSQLiteStore("localstore.db");
        //        store.DefineTable<TodoItem>();
        //        await App.MobileService.SyncContext.InitializeAsync(store);
        //    }
        //
        //    await SyncAsync();
        //}

        //private async Task SyncAsync()
        //{
        //    await App.MobileService.SyncContext.PushAsync();
        //    await todoTable.PullAsync("todoItems", todoTable.CreateQuery());
        //}

        #endregion 
    }
}
