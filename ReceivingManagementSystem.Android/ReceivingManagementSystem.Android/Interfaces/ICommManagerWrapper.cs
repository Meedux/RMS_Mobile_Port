using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DensoScannerSDK.Interface;

namespace ReceivingManagementSystem.Android.Interfaces
{
    public interface ICommManagerWrapper
    {
        ICommScanner GetScanners();

        void AddAcceptStatusListener(ScannerAcceptStatusListener listener);

        void RemoveAcceptStatusListener(ScannerAcceptStatusListener listener);

        void StartAccept();

        void EndAccept();
    }
}
