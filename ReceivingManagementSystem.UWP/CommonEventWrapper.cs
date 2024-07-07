using System;

[assembly: Xamarin.Forms.Dependency(typeof(ReceivingManagementSystem.UWP.CommonEventWrapper))]
namespace ReceivingManagementSystem.UWP
{
    class CommonEventWrapper : ReceivingManagementSystem.Wrapper.ICommonEventWrapper
    {
        #region Android���� Android event
        public event EventHandler OnUserLeaveHint;
        public event EventHandler OnRestart;

        public void RaiseOnUserLeaveHint()
        {
            //do nothing
        }

        public void RaiseOnRestart()
        {
            //do nothing
        }
        #endregion

    }
}