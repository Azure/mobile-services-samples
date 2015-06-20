using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace CustomAuthClient
{
    public class LoginRequest
    {
        [JsonProperty(PropertyName = "username")]
        public String UserName { get; set; }
        [JsonProperty(PropertyName = "password")]
        public String Password { get; set; }
    }
}