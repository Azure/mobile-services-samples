using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(ToDoAzure.CheckBox), typeof(ToDoAzure.iOS.CheckBoxRenderer))]

namespace ToDoAzure.iOS
{
    public class CheckBoxRenderer : ButtonRenderer
    {
        private ToDoItemManager todoItemManager;
        protected async override void OnElementChanged(ElementChangedEventArgs<Button> e)
        {
            base.OnElementChanged(e);

            string TaskID = e.NewElement.CommandParameter.ToString();
            todoItemManager = AppDelegate.DefaultService.todoItemManager;

            UIKit.UIButton checkBox = new UIKit.UIButton();
            checkBox.SetImage(new UIImage("checked.png"), UIControlState.Selected);
            checkBox.SetImage(new UIImage("unchecked.png"), UIControlState.Normal);

            ToDoItem item = todoItemManager.GetTaskFromList(TaskID);
            if (item == null)
                item = await todoItemManager.GetTaskAsync(TaskID);

            checkBox.Selected = item.Done;

            checkBox.TouchUpInside += (sender, eventArgs) => 
                checkBox_TouchUpInside(sender, eventArgs, TaskID);

            this.SetNativeControl(checkBox);
        }

        async void checkBox_TouchUpInside(object sender, EventArgs e, string TaskID)
        {
            Control.Selected = !Control.Selected;

            ToDoItem item = todoItemManager.GetTaskFromList(TaskID);
            item.Done = Control.Selected;
            
            await todoItemManager.SaveTaskAsync(item);
        }
    }
}