using System.Threading.Tasks;

using System.Windows.Controls;
using Xamarin.Forms;
using Xamarin.Forms.Platform.WinPhone;

[assembly: ExportRenderer(typeof(ToDoAzure.TextBox), typeof(ToDoAzure.WinPhone.TextBoxRenderer))]
namespace ToDoAzure.WinPhone
{
    public class TextBoxRenderer : EditorRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<Editor> e)
        {
            base.OnElementChanged(e);

            var nativeControl = (System.Windows.Controls.TextBox)Control;

            setTextBoxColor(nativeControl);

            nativeControl.GotFocus += nativeControl_GotFocus;
        }
        void nativeControl_GotFocus(object sender, System.Windows.RoutedEventArgs e)
        {
            // Make sure text box continues to be black when it is selected.
            setTextBoxColor(sender as System.Windows.Controls.TextBox);
        }

        private void setTextBoxColor(System.Windows.Controls.TextBox textBox)
        {
           textBox.Foreground = new 
               System.Windows.Media.SolidColorBrush
               (System.Windows.Media.Color.FromArgb(255, 255, 255, 255));

           textBox.Background = new 
               System.Windows.Media.SolidColorBrush
               (System.Windows.Media.Color.FromArgb(0, 0, 0, 0)); 
        }
    }
}

