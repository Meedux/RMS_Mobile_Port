using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

[assembly: XamlCompilation (XamlCompilationOptions.Compile)]
namespace ReceivingManagementSystem.Android
{
	public partial class App : Application
	{
        CommonBase m_hCommonBase = CommonBase.GetInstance();

        public App ()
		{
			Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("MjQxMjU0M0AzMjMxMmUzMDJlMzBPTm8wK1hHZDdKTC9xMTI4blZUVnRSa0F4T21VM2NZY0RXK1NIZDV5Sk1rPQ==");

			InitializeComponent();

			MainPage = new NavigationPage(new MainPage());

        }

		protected override void OnStart ()
		{
            // Handle when your app starts
            m_hCommonBase.SetIsForeground(true);
		}

		protected override void OnSleep ()
		{
            // Handle when your app sleeps
            m_hCommonBase.SetIsForeground(false);
            m_hCommonBase.RaiseOnSleep();
		}

		protected override void OnResume ()
		{
            m_hCommonBase.SetIsForeground(true);
            m_hCommonBase.RaiseOnResume();
        }
    }
}
