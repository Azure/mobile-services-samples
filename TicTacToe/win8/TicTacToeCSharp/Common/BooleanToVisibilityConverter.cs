using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Interop;

namespace TicTacToeCSharp.Common
{
    [Windows.Foundation.Metadata.WebHostHidden]
    public class BooleanToVisibilityConverter : Windows.UI.Xaml.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, 
            object parameter, string language)
        {
            bool boolValue = (bool)value;
            return boolValue ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, 
            object parameter, string language)
        {
            if (value == null)
            {
                return false;
            }
            else
            {
                Visibility visibility = (Visibility)value;
                return visibility == Visibility.Visible;
            }
        }
    }
}
