using Android.Widget;
using Android.OS;
using Android;
using ReceivingManagementSystem.Android.Interfaces;
using System.Threading.Tasks;
using Xamarin.Forms;
using ReceivingManagementSystem.Android.Droid;

[assembly: Dependency(typeof(ReceivingManagementSystem.Android.CommonUtilWrapper))]
namespace ReceivingManagementSystem.Android
{
    public class CommonUtilWrapper : ICommonUtilWrapper
    {
        public void CloseApplication()
        {
            Process.KillProcess(Process.MyPid());
        }

        public void ShowMessage(string msg)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                Toast.MakeText(MainActivity.AppContext, msg, ToastLength.Short).Show();
            });
        }

        public void ShowMessage2(string msg)
        {
            Task.Run(() =>
            {
                ShowMessage(msg);
            });
        }

        public void StartService()
        {
            //Do Nothing
        }
    }
}