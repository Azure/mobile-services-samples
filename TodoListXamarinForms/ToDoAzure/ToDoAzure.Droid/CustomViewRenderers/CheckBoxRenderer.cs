
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;


[assembly: ExportRenderer(typeof(ToDoAzure.CheckBox), typeof(ToDoAzure.Droid.CheckBoxRenderer))]

namespace ToDoAzure.Droid
{
    public class CheckBoxRenderer : ButtonRenderer
    {
        private ToDoItemManager todoItemManager;
        protected async override void OnElementChanged(ElementChangedEventArgs<Xamarin.Forms.Button> e)
        {
            base.OnElementChanged(e);

            string TaskID = e.NewElement.CommandParameter.ToString();
            todoItemManager = MainActivity.DefaultService.todoItemManager;

            Android.Widget.CheckBox checkbox = new Android.Widget.CheckBox(this.Context);

            ToDoItem item = todoItemManager.GetTaskFromList(TaskID);
            if (item == null)
                item = await todoItemManager.GetTaskAsync(TaskID);
            else
                checkbox.Checked = item.Done;

            checkbox.Click += (sender, eventArgs) => checkbox_Click(sender, eventArgs, TaskID);

            this.SetNativeControl(checkbox);
        }
        async void checkbox_Click(object sender, EventArgs e, string TaskID)
        {
            Android.Widget.CheckBox checkbox = sender as Android.Widget.CheckBox;

            ToDoItem item = todoItemManager.GetTaskFromList(TaskID);
            item.Done = checkbox.Checked;

            await todoItemManager.SaveTaskAsync(item);
        }

    }
}