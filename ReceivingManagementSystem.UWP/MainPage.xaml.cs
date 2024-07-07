using ReceivingManagementSystem.Common.Resources;
using ReceivingManagementSystem.Services.Email;
using ReceivingManagementSystem.Services.Pleasanter;
using ReceivingManagementSystem.Services.Rfid;
using ReceivingManagementSystem.Wrapper;
using RMS_Pleasanter;
using Serilog;
using Serilog.Formatting.Compact;
using Serilog.Sinks.File;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Xamarin.Forms;

namespace ReceivingManagementSystem.UWP
{
    public sealed partial class MainPage
    {
        public MainPage()
        {
            this.InitializeComponent();

            //Shift-JIS 対応
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            ConfigLog();

            LoadApplication(new ReceivingManagementSystem.App());

            RegisterProvider();
            RegisterService();
        }

        private void RegisterProvider()
        {
            DependencyService.RegisterSingleton<IResourceProvider>(new ResourceProvider());
        }

        private void RegisterService()
        {
            DependencyService.Register<IReadRfidService, ReadRfidService>();
            DependencyService.RegisterSingleton<IPleasanterService>(new PleasanterService());
            DependencyService.RegisterSingleton<IEmailService>(new EmailService());
        }

        private void ConfigLog()
        {
            Log.Logger = new LoggerConfiguration()
                    .WriteTo.File(Windows.Storage.ApplicationData.Current.LocalCacheFolder.Path + @"\Logs\log.txt", rollingInterval: RollingInterval.Day)
                    .CreateLogger();

            Log.Information("Start app");
        }

    }
}
