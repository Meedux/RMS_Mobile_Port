using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReceivingManagementSystem.Android.Interfaces
{
    public interface ICommonUtilWrapper
    {
        // 以下を参考 See below
        //https://stackoverflow.com/questions/29257929/how-to-terminate-a-xamarin-application
        void CloseApplication();
        void ShowMessage(string msg);
        void ShowMessage2(string msg);
        void StartService();
    }
}
