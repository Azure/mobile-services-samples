using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Xamarin.Forms;
using Xamarin.Forms.Platform.WinPhone;

[assembly: ExportRenderer(typeof(ToDoAzure.CheckBox), typeof(ToDoAzure.WinPhone.CheckBoxRenderer))]

namespace ToDoAzure.WinPhone
{
    public class CheckBoxRenderer : ViewRenderer<ToDoAzure.CheckBox, System.Windows.Controls.CheckBox>
    {
        private ToDoItemManager todoItemManager;
        protected async override void OnElementChanged(ElementChangedEventArgs<ToDoAzure.CheckBox> e)
        {
            base.OnElementChanged(e);
          
            todoItemManager = MainPage.DefaultService.todoItemManager;

            var checkBoxNew = e.NewElement;
            
            if (checkBoxNew != null)
            {
                System.Windows.Controls.CheckBox checkbox = new System.Windows.Controls.CheckBox();
                checkbox.IsChecked = true; // IsChecked has to be initialized to true.
              
                string TaskID = e.NewElement.CommandParameter.ToString();
                ToDoItem item = todoItemManager.GetTaskFromList(TaskID);      
                
                if (item == null)
                    item = await todoItemManager.GetTaskAsync(TaskID);
                
                Device.BeginInvokeOnMainThread(() => { Control.IsChecked = item.Done; });

                checkbox.Click += (sender, routedEventArgs) => 
                    checkbox_Click(sender, routedEventArgs, item);

                this.SetNativeControl(checkbox);
            }
        }
        async void checkbox_Click(object sender, System.Windows.RoutedEventArgs e, ToDoItem item)
        {
            todoItemManager.CheckBoxClicked = true;
            item.Done = Convert.ToBoolean(Control.IsChecked);
            
            await todoItemManager.SaveTaskAsync(item);
        }
    }
}
