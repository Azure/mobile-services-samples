using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToDoAzure
{
    public class DeviceDetails
    {
        private double _deviceWidth;
        private double _deviceHeight;

        public double DeviceWidth
        {
            get { return this._deviceWidth; }
            set { _deviceWidth = value; }
        }
        public double DeviceHeight
        {
            get { return this._deviceHeight; }
            set { _deviceHeight = value; }
        }
    }
}
