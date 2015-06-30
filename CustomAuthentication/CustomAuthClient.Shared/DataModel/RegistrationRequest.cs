using System;
using System.Collections.Generic;
using System.Linq;

using Newtonsoft.Json;

namespace CustomAuthClient
{
    public class RegistrationRequest 
    {
        [JsonProperty(PropertyName="username")]
        public String Username { get; set; }
        [JsonProperty(PropertyName = "password")]
        public String Password { get; set; }
        [JsonProperty(PropertyName = "email")]
        public String Email { get; set; }
        [JsonProperty(PropertyName = "friendlyName")]
        public String FriendlyName { get; set; }
    }
}