using System;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Linq;

namespace ToDoAzure
{
    /// <summary>
    /// Manager classes are an abstraction on the data access layers
    /// </summary>
    public class ToDoItemManager
    {
        // Azure
        private IMobileServiceTable<ToDoItem> _todoTable;
        private MobileServiceClient _client;

        private ToDoListViewModel _todoViewModel = new ToDoListViewModel();
        private DeviceDetails _device = new DeviceDetails();
        private bool _checkBoxClicked = false;

        public ToDoListViewModel TodoViewModel
        {
            get { return this._todoViewModel; }
        }

        public DeviceDetails Device
        {
            get { return this._device; }
            set { _device = value; }
        }
        public bool CheckBoxClicked
        {
            get { return this._checkBoxClicked; }
            set { _checkBoxClicked = value; }
        }
        public MobileServiceClient GetClient
        {
            get
            {
                return _client;
            }
        }
        public ToDoItemManager()
        {
            _client = new MobileServiceClient(
                Constants.ApplicationURL,
                Constants.ApplicationKey);
                
            this._todoTable = _client.GetTable<ToDoItem>();
            App.SetTodoItemManager(this);
        }
        public ToDoItem GetTaskFromList(string id)
        {
            return TodoViewModel.TodoItems.FirstOrDefault(o => o.ID == id);   
        }
        public async Task<ToDoItem> GetTaskAsync(string id)
        {
            try
            {
                return await _todoTable.LookupAsync(id);
            }
            catch (MobileServiceInvalidOperationException msioe)
            {
                Debug.WriteLine(@"INVALID {0}", msioe.Message);
            }
            catch (Exception e)
            {
                Debug.WriteLine(@"ERROR {0}", e.Message);
            }
            return null;
        }
        public async Task<ObservableCollection<ToDoItem>> GetTasksAsync()
        {
            try
            {
                return new ObservableCollection<ToDoItem>(await _todoTable.ReadAsync());
            }
            catch (MobileServiceInvalidOperationException msioe)
            {
                Debug.WriteLine(@"INVALID {0}", msioe.Message);
            }
            catch (Exception e)
            {
                Debug.WriteLine(@"ERROR {0}", e.Message);
            }
            return null;
        }
        public async Task SaveTaskAsync(ToDoItem item)
        {
            if (item.ID == null)
            {
                await _todoTable.InsertAsync(item);
                TodoViewModel.TodoItems.Add(item);
            }
            else
                await _todoTable.UpdateAsync(item);
        }
        public async Task DeleteTaskAsync(ToDoItem item)
        {
            try
            {
                TodoViewModel.TodoItems.Remove(item);
                await _todoTable.DeleteAsync(item);
            }
            catch (MobileServiceInvalidOperationException msioe)
            {
                Debug.WriteLine(@"INVALID {0}", msioe.Message);
            }
            catch (Exception e)
            {
                Debug.WriteLine(@"ERROR {0}", e.Message);
            }
        }
    }
}