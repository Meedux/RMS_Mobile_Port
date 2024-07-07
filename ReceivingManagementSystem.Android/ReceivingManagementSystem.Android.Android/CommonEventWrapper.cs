using System;

[assembly: Xamarin.Forms.Dependency(typeof(ReceivingManagementSystem.UWP.CommonEventWrapper))]
namespace ReceivingManagementSystem.UWP
{
    class CommonEventWrapper : ReceivingManagementSystem.Android.Interfaces.ICommonEventWrapper
    {
        #region AndroidŒÀ’è Android event
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