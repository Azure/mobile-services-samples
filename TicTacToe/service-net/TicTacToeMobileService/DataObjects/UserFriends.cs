using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.WindowsAzure.Mobile.Service;
using System.ComponentModel.DataAnnotations;

namespace TicTacToeMobileService.DataObjects
{
    public class UserFriends : EntityData
    {
        public string User1Key { get; set; }
        public string User2Key { get; set; }
        public string User2Name { get; set; }
    }
}