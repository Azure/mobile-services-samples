using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.WindowsAzure.Mobile.Service;

namespace blog20141010.DataObjects
{
    public class Person : StorageData
    {
        public string Name { get; set; }

        public int Age { get; set; }
    }
}