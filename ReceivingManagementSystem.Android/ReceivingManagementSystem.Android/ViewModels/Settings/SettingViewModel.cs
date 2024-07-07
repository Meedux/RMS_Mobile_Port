using CsvHelper;
using ReceivingManagementSystem.Common.Resources;
using ReceivingManagementSystem.Android.Orders;
using ReceivingManagementSystem.Android.Services.Rfid;
using ReceivingManagementSystem.Android.Views;
using ReceivingManagementSystem.Views.Android.Settings;
using ReceivingManagementSystem.Android.Interfaces;
using RMS_Pleasanter;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.CommunityToolkit.Extensions;
using Xamarin.CommunityToolkit.UI.Views;
using Xamarin.Essentials;
using Xamarin.Forms;
using static PleasanterOperation.OperationData;
using static RMS_Pleasanter.Contents;
using static RMS_Pleasanter.Custody;
using System.Diagnostics;
using JP.CO.Toshibatec.Callback;

namespace ReceivingManagementSystem.Android.ViewModels.Orders
{
    public class SettingViewModel : BaseViewModel
    {
        #region Properties

        /// <summary>
        /// RFID
        /// </summary>
        private string _rfid;

        /// <summary>
        /// RFID
        /// </summary>
        public string Rfid
        {
            get { return _rfid; }

            set { this.SetProperty(ref this._rfid, value); }
        }

        /// <summary>
        /// Devices
        /// </summary>
        private List<ComboBoxItemViewModel> _devices;

        /// <summary>
        /// Devices
        /// </summary>
        public List<ComboBoxItemViewModel> Devices
        {
            get { return _devices; }

            set { this.SetProperty(ref this._devices, value); }
        }

        /// <summary>
        /// Device selected
        /// </summary>
        private ComboBoxItemViewModel _deviceSelected;

        /// <summary>
        /// Device selected
        /// </summary>
        public ComboBoxItemViewModel DeviceSelected
        {
            get { return _deviceSelected; }
            set
            {
                this.SetProperty(ref this._deviceSelected, value);
            }
        }
        #endregion

        #region Command

        public ICommand SP1SettingCommand { get; }
        public ICommand PleasanterSettingCommand { get; }
        public ICommand ReadRFIDCommand { get; }
        public ICommand SendSettingCommand { get; }
        public ICommand OkCommand { get; }
        public ICommand SoundSettingCommand { get; }

        #endregion

        private IReadRfidService _readRfidService;
        private ISaveSettingsWrapper _pSaveSettingsWrapper;
        private IToshibamanager toshibaManager;

        private CommonBase m_hCommonBase = CommonBase.GetInstance();

        public bool IsCommScanner 
        {
            get
            {
                return m_hCommonBase.IsCommScanner();
            }
        }

        public string BackgroundColor
        {
            get
            {
                return (IsCommScanner) ? "#9FBFBE" : "#CFCFCF";
            }
        }

        public SettingViewModel(ContentPage owner) : base(owner)
        {
            SP1SettingCommand = new Command(SP1Setting);
            PleasanterSettingCommand = new Command(PleasanterSetting);
            ReadRFIDCommand = new Command(ReadRFID);
            SendSettingCommand = new Command(SendSetting);
            OkCommand = new Command(Ok);
            SoundSettingCommand = new Command(SoundSetting);

            _readRfidService = DependencyService.Get<IReadRfidService>();
            _pSaveSettingsWrapper = DependencyService.Get<ISaveSettingsWrapper>();
            toshibaManager = DependencyService.Get<IToshibamanager>();

            GetDevices();
            GetSetting();
        }

        private void GetSetting()
        {
            Debug.WriteLine($"_pSaveSettingsWrapper: {_pSaveSettingsWrapper}");
            if (_pSaveSettingsWrapper != null){
                var device = _pSaveSettingsWrapper.GetString(ReceivingManagementSystem.Common.Constants.Setting_Device, "SP1");
                DeviceSelected = _devices.FirstOrDefault(r => r.Value.Equals(device));
            }

        }

        public void RfidInit()
        {
            Debug.WriteLine($"_readRfidServService: {_readRfidService}");
            //Fix the Null issue on this
            if (_readRfidService != null)
            {
                _readRfidService.OnReadRfid += OnReadRfid;
                _readRfidService.OnInit();
            }
        }

        public void RfidStop()
        {
            _readRfidService.OnReadRfid -= OnReadRfid;
            _readRfidService.Stop();
        }

        /// <summary>
        /// ShowConfirm Order
        /// </summary>
        private async void SP1Setting()
        {
            await this.Owner.Navigation.PushAsync(new SettingDevicePage(), true);
        }

        private async void PleasanterSetting()
        {
            await this.Owner.Navigation.PushAsync(new PleasanterSettingPage(), true);
        }

        private async void SendSetting()
        {
            await this.Owner.Navigation.PushAsync(new SendSettingPage(), true);
        }

        private async void Ok()
        {
            if (_pSaveSettingsWrapper == null)
            {
                _pSaveSettingsWrapper = DependencyService.Get<ISaveSettingsWrapper>();
            }
            var result = _pSaveSettingsWrapper.SaveSettings(new SaveSettingsParam(SaveSettingsParam.SaveTypes.STRING,ReceivingManagementSystem.Common.Constants.Setting_Device, _deviceSelected.Value));

            if (!result)
            {
                await ShowAlert(TextResourceKey.NotificationTitle, TextResourceKey.OK,
                     MessageResourceKey.E0003, TextResourceKey.Setting);
            }
            else
            {
                try
                {
                    ShowMessage(MessageResourceKey.E0004, TextResourceKey.Setting);
                    //Close();
                    await this.Owner.Navigation.PopAsync();
                }
                catch (NullReferenceException ex)
                {
                    Debug.WriteLine("ERROR: " + ex.Message);
                }
            }
            Debug.WriteLine($"_pSaveSettingsWrapper: {_pSaveSettingsWrapper}");
        }

        /// <summary>
        /// Read RFID
        /// </summary>
        private void ReadRFID()
        {
            if (DeviceSelected != null)
            {
                switch (DeviceSelected.Value.ToString())
                {
                    case "SP1":
                        _readRfidService.ReadRfid();
                        break;
                    case "Toshiba Tec":
                        toshibaManager.StartReadTags("0", "0", 0, toshibaManager.DataEventHandler, toshibaManager.ErrorEventHandler);
                        break;
                    default:
                        Rfid = string.Empty;
                        break;
                }
            }
            else
            {
                Rfid = string.Empty;
            }
        }

        private void GetDevices()
        {
            Devices = new List<ComboBoxItemViewModel>();
            Devices.Add(new ComboBoxItemViewModel()
            {
                DisplayValue = "SP1",
                Value = "SP1",
            });

            Devices.Add(new ComboBoxItemViewModel()
            {
                DisplayValue = "RP902",
                Value = "RP902",
            });

            Devices.Add(new ComboBoxItemViewModel()
            {
                DisplayValue = "Toshiba Tec",
                Value = "Toshiba Tec",
            });
        }

        private void OnReadRfid(object sender, RfidResultEventArgs args)
        {
            Rfid = args.Rfid;
        }

        private async void SoundSetting()
        {
            await this.Owner.Navigation.PushAsync(new SoundSettingPage(), true);
        }
    }
}
