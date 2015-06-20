using System;
using Newtonsoft.Json;
using System.ComponentModel;

namespace ToDoAzure
{
    public class ToDoItem
    {
        string _id;
        string _name;
        string _notes;
        bool _done;

        [JsonProperty(PropertyName = "id")]
        public string ID
        {
            get { return _id; }
            set { _id = value;}
        }

        [JsonProperty(PropertyName = "text")]
        public string Name
        {
            get { return _name; }
            set { _name = value;}
        }

        [JsonProperty(PropertyName = "notes")]
        public string Notes
        {
            get { return _notes; }
            set { _notes = value;}
        }

        [JsonProperty(PropertyName = "complete")]
        public bool Done
        {
            get { return _done; }
            set { _done = value;}
        }

    }
}
