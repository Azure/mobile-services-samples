using Newtonsoft.Json;
using System;

namespace MultiChannelToDo.Models
{
    [JsonObject]
    public class TodoItem
    {
        public TodoItem()
        {
            Id = Guid.NewGuid().ToString();
            CreatedAt = DateTimeOffset.UtcNow;
            UpdatedAt = DateTimeOffset.UtcNow;
        }

        public string Id { get; set; }
        public string Text { get; set; }
        public bool Complete { get; set; }
        public DateTimeOffset? CreatedAt { get; set; }
        public DateTimeOffset? UpdatedAt { get; set; }
        public bool Deleted { get; set; }
    }
}