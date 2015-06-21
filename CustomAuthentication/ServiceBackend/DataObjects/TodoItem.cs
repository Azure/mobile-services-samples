using Microsoft.WindowsAzure.Mobile.Service;

namespace CustomAuthMobileService.DataObjects
{
    //// TODO: Use the below definition for Azure Table storage.
    //public class TodoItem : StorageData
    public class TodoItem : EntityData
    {
        public string Text { get; set; }

        public bool Complete { get; set; }
    }
}