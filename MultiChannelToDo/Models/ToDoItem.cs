using Newtonsoft.Json;
using System;

namespace MultiChannelToDo.Models
{
    [JsonObject]
    public class ToDoItem
    {
        public ToDoItem()
        {
            Complete = false;
            CreatedAt = DateTimeOffset.UtcNow;
            UpdatedAt = DateTimeOffset.UtcNow;
        }

        public string Id { get; set; }
        public string Text { get; set; }
        public bool Complete { get; set; }
        public DateTimeOffset? CreatedAt { get; set; }
        public DateTimeOffset? UpdatedAt { get; set; }
    }
}