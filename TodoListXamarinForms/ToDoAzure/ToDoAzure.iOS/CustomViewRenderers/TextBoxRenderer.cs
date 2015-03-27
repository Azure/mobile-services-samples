using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;

using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(ToDoAzure.TextBox), typeof(ToDoAzure.iOS.TextBoxRenderer))]

namespace ToDoAzure.iOS
{
    public class TextBoxRenderer : EditorRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<Editor> e)
        {
            base.OnElementChanged(e);

            Control.TextColor = UIColor.White;
            Control.BackgroundColor = UIColor.Black;
        }
    }
}