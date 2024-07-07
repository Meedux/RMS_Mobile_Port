using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace ReceivingManagementSystem.Android.Services.Rfid
{
    public class RfidResultEventArgs: EventArgs
    {
        public string Rfid { get; }
        public float PowerLevel { get; }
        public ImageSource PowerImage { get; }
        public bool IsExist { get; }

        public RfidResultEventArgs(string rfid)
        {
            Rfid = rfid;
        }

        public RfidResultEventArgs(float powerLevel, ImageSource powerImage, bool isExist)
        {
            PowerLevel = powerLevel;
            PowerImage = powerImage;
            IsExist = isExist;
        }
    }
}
