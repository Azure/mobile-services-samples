using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.WindowsAzure.Mobile.Service;
using System.ComponentModel.DataAnnotations;

namespace TicTacToeMobileService.DataObjects
{
    public class User : EntityData
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string Foo { get; set; }
    }
}