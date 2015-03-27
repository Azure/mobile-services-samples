using System;
using Xamarin.Forms;

namespace ToDoAzure
{
    public partial class ToDoItemXaml : ContentPage
    {
        public ToDoItemXaml()
        {
            InitializeComponent();

            detailsStack.HeightRequest = App.TodoManager.Device.DeviceHeight / 2;
            
            saveButton.WidthRequest = App.TodoManager.Device.DeviceWidth / 3;
            deleteButton.WidthRequest = App.TodoManager.Device.DeviceWidth / 3;
            cancelButton.WidthRequest = App.TodoManager.Device.DeviceWidth / 3;

            if (Device.OS == TargetPlatform.WinPhone)
            {
                nameEntry.TextColor = Color.Black;
            }
        }
        async void OnSaveActivated(object sender, EventArgs e)
        {
            var todoItem = (ToDoItem)BindingContext;
            await App.TodoManager.SaveTaskAsync(todoItem);
            await this.Navigation.PopAsync();
        }
        async void OnDeleteActivated(object sender, EventArgs e)
        {
            var todoItem = (ToDoItem)BindingContext;
            await App.TodoManager.DeleteTaskAsync(todoItem);
            await this.Navigation.PopAsync();
        }

        void OnCancelActivated(object sender, EventArgs e)
        {
            this.Navigation.PopAsync();
        }

    }
}

