using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Microsoft.WindowsAzure.Mobile.Service;
using Microsoft.WindowsAzure.Storage.Table;

namespace MobileServiceTableStorage.DataObjects
{
    public class Account : TableEntity
    {
        public string Username { get; set; }
        public byte[] Salt { get; set; }
        public byte[] SaltedAndHashedPassword { get; set; }
    }
}