using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReceivingManagementSystem.Android.Interfaces
{
    // <summary>
    // Android限定、またはiOS限定のイベント両側をXamarin.Formsで受け取るために作ったWrapper Classです。
    // It is a Wrapper Class created to receive both sides of Android limited or iOS limited event on Xamarin.Forms.

    public interface ICommonEventWrapper
    {
        #region Android限定 Android event
        event EventHandler OnUserLeaveHint;
        event EventHandler OnRestart;

        void RaiseOnUserLeaveHint();
        void RaiseOnRestart();
        #endregion

        #region iOS限定 iOS event

        #endregion
    }
}
