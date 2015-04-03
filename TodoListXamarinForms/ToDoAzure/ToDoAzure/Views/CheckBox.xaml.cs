using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;

namespace ToDoAzure
{
    // Example adapted from Charles Petzolds "Creating Mobile Apps with Xamarin.Forms Book Preview 2
    // http://developer.xamarin.com/guides/cross-platform/xamarin-forms/creating-mobile-apps-xamarin-forms/
    public partial class CheckBox : ContentView
    {

        public static readonly BindableProperty FontSizeProperty =
            BindableProperty.Create<CheckBox, double>(
                checkbox => checkbox.FontSize,
                Device.GetNamedSize(NamedSize.Default, typeof(Label)),
                propertyChanged: (bindable, oldValue, newValue) =>
                {
                    CheckBox checkbox = (CheckBox)bindable;
                    checkbox.checkBoxLabel.FontSize = newValue;
                });

        public static readonly BindableProperty IsCheckedProperty =
            BindableProperty.Create<CheckBox, bool>(
                checkbox => checkbox.IsChecked,
                false,
                propertyChanged: (bindable, oldValue, newValue) =>
                {
                    // Set the graphic.
                    CheckBox checkbox = (CheckBox)bindable;
                    checkbox.checkBoxLabel.Text = newValue ? "\u2611" : "\u25FB";
                    // Fire the event.
                    EventHandler<bool> eventHandler = checkbox.CheckedChanged;
                    if (eventHandler != null)
                    {
                        eventHandler(checkbox, newValue);
                    }
                });

        public event EventHandler<bool> CheckedChanged;

        public CheckBox()
        {
            InitializeComponent();
        }

        [TypeConverter(typeof(FontSizeConverter))]
        public double FontSize
        {
            set { SetValue(FontSizeProperty, value); }
            get { return (double)GetValue(FontSizeProperty); }
        }

        public bool IsChecked
        {
            set { SetValue(IsCheckedProperty, value); }
            get { return (bool)GetValue(IsCheckedProperty); }
        }

        // TapGestureRecognizer handler.
        void OnCheckBoxTapped(object sender, EventArgs args)
        {
            IsChecked = !IsChecked;
        }
    }
}
