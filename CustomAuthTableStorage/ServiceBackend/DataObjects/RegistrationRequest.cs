using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MobileServiceTableStorage.DataObjects
{
    public class RegistrationRequest 
    {
        public String username { get; set; }
        public String password { get; set; }
        public String email { get; set; }
        public String friendlyName { get; set; }
    }
}