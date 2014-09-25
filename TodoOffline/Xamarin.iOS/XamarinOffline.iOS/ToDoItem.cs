using System;
using Newtonsoft.Json;
using Microsoft.WindowsAzure.MobileServices;

namespace XamarinOffline
{
    public class ToDoItem
    {
        public string Id { get; set; }

        [JsonProperty(PropertyName = "text")]
        public string Text { get; set; }

        [JsonProperty(PropertyName = "complete")]
        public bool Complete { get; set; }

        [Version]
        public string Version { get; set; }

        public override string ToString()
        {
            return "Text: " + Text + "\nComplete: " + Complete + "\n";
        }
    }
}

