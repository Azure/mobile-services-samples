using Microsoft.WindowsAzure.Mobile.Service;

namespace blog20141010.DataObjects
{
    public class TodoItem : EntityData
    {
        public string Text { get; set; }

        public bool Complete { get; set; }
    }
}