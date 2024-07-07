using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using DensoScannerSDK.Interface;
using ReceivingManagementSystem.Wrapper;
using DensoScannerSDKWindows;

[assembly: Xamarin.Forms.Dependency(typeof(ReceivingManagementSystem.UWP.CommManagerWrapper))]

namespace ReceivingManagementSystem.UWP
{
    /**
     * CommManagerのWrapperクラス
     * Wrapper class of CommManager
     */
    public class CommManagerWrapper : ReceivingManagementSystem.Wrapper.ICommManagerWrapper
    {
        public CommManagerWrapper()
        {
        }

        public void AddAcceptStatusListener(ScannerAcceptStatusListener listener)
        {
            CommManager.AddAcceptStatusListener(listener);
        }

        public void EndAccept()
        {
            CommManager.EndAccept();
        }

        #region Slave用 旧コード previous code for slave mode
        ICommScanner ICommManagerWrapper.GetScanners()
        {
            List<ICommScanner> ScannerList = CommManager.GetScanners();
            if ( ScannerList != null )
            {
                foreach (ICommScanner Scanner in ScannerList)
                {
                    if (Scanner.GetBTLocalName().StartsWith("SP1"))
                    {
                        return Scanner;
                    }
                }
            }
            
            return null;
        }

        public void RemoveAcceptStatusListener(ScannerAcceptStatusListener listener)
        {
            CommManager.RemoveAcceptStatusListener(listener);
        }

        public void StartAccept()
        {
            CommManager.StartAccept();
        }

        #endregion

    }
}