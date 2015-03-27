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

[assembly: ExportRenderer(typeof(ToDoAzure.TextBox), typeof(ToDoAzure.Droid.TextBoxRenderer))]

namespace ToDoAzure.Droid
{
    public class TextBoxRenderer : EditorRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<Editor> e)
        {
            base.OnElementChanged(e);

            var nativeControl = (EditText)Control;

            nativeControl.SetTextColor(Android.Graphics.Color.White);
            nativeControl.SetBackgroundColor(Android.Graphics.Color.Black);
        }
    }
}