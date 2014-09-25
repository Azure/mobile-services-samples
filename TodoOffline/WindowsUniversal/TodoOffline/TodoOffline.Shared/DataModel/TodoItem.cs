using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Microsoft.WindowsAzure.MobileServices;

namespace TodoOffline
{
    public class TodoItem
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
            return "    Title: " + Text + "\n    Complete: " + Complete;
        }
    }
}
