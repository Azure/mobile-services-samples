using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Microsoft.WindowsAzure.Mobile.Service;
using Microsoft.WindowsAzure.Storage.Table;

namespace CustomAuthMobileService.DataObjects
{
    //// TODO: Use the below definition for Azure Table storage.    
    //public class Account : StorageData
    public class Account : EntityData
    {
        public string Username { get; set; }
        public byte[] Salt { get; set; }
        public byte[] SaltedAndHashedPassword { get; set; }
        public bool IsConfirmed { get; set; }
        public string Email { get; set; }
        public string FriendlyName { get; set; }
    }
}