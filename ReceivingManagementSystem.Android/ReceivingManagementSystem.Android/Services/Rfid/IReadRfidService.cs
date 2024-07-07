using System;
using System.Collections.Generic;
using System.Text;

namespace ReceivingManagementSystem.Android.Services.Rfid
{
    public interface IReadRfidService
    {
        event EventHandler<RfidResultEventArgs> OnReadRfid;
        event EventHandler<RfidResultEventArgs> OnReadPowerLevel;

        void OnInit();
        void Stop();
        void StopRead();
        void ReadRfid();
        void ReadMultiRfid();
        void SearchRfid(string refidSearch);
    }
}
