using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace TicTacToeLibraryCSharp.Common
{
    /// <summary>
    /// Implementation of <see cref="INotifyPropertyChanged"/> to simplify models.
    /// </summary>
    [Windows.Foundation.Metadata.WebHostHidden]
    public class BindableBase : DependencyObject, INotifyPropertyChanged, ICustomPropertyProvider
    {
        public event PropertyChangedEventHandler PropertyChanged;

        // ICustomPropertyProvider
        public ICustomProperty GetCustomProperty(string name) { return null; }
        public ICustomProperty GetIndexedProperty(string name, Type type) { return null; }
        public string GetStringRepresentation() { return this.ToString();  }

        public System.Type Type
        {
            get { return this.GetType(); }
        }

        protected bool SetProperty<T>(ref T backingStore, T newValue, String propertyName)
        {
            if (object.Equals(backingStore, newValue))
            {
                return false;
            }

            backingStore = newValue;
            this.OnPropertyChanged(propertyName);
            return true;
        }


        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    };
}
