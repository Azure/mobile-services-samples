using Newtonsoft.Json;
using System;

namespace MultiChannelToDo.Models
{
    [JsonObject]
    public class ToDoItem
    {
        public string Id { get; set; }
        public string Text { get; set; }
        public bool Complete { get; set; }
        [JsonProperty("__createdAt")]
        public DateTimeOffset? CreatedAt { get; set; }
        [JsonProperty("__updatedAt")]
        public DateTimeOffset? UpdatedAt { get; set; }
    }
}