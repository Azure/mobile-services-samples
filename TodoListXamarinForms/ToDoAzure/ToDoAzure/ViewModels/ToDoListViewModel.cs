using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace ToDoAzure
{
    public class ToDoListViewModel
    {
        private ObservableCollection<ToDoItem> _todoItems =
            new ObservableCollection<ToDoItem>();

        public ObservableCollection<ToDoItem> TodoItems
        {
            get { return this._todoItems; }
            set { _todoItems = value;}
        }

    }
}
