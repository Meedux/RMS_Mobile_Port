using System;
using Android.Content;
using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.OS;
using Xamarin.Forms;
using ReceivingManagementSystem.Android.Services.Rfid;
using ReceivingManagementSystem.Android.Services.Pleasanter;
using ReceivingManagementSystem.Android.Services.Email;
using ReceivingManagementSystem.Android.Services;
using ReceivingManagementSystem.Android.Interfaces;
using ReceivingManagementSystem.Common.Resources;

namespace ReceivingManagementSystem.Android.Droid
{
    [Activity(Label = "ReceivingManagementSystem.Android", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize )]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        public static Context AppContext { get; private set; }
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            AppContext = this;
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);

            DependencyService.Register<IPleasanterService, PleasanterService>();
            DependencyService.Register<IEmailService, EmailService>();
            RegisterProvider();
            RegisterService();
            LoadApplication(new App());
        }
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum]Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
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
    }
}