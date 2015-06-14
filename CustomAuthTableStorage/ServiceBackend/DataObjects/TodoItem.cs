using Microsoft.WindowsAzure.Mobile.Service;

namespace MobileServiceTableStorage.DataObjects
{
    public class TodoItem : StorageData
    {
        public string Text { get; set; }

        public bool Complete { get; set; }
    }
}