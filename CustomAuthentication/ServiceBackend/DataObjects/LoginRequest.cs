using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CustomAuthMobileService.DataObjects
{
    public class LoginRequest
    {
        public String username { get; set; }
        public String password { get; set; }
    }
}