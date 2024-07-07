using CsvHelper;
using ReceivingManagementSystem.Common.Resources;
using ReceivingManagementSystem.Android.Orders;
using ReceivingManagementSystem.Android.Services.Pleasanter;
using ReceivingManagementSystem.Android.Services.Rfid;
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

namespace ReceivingManagementSystem.Android.ViewModels.Orders
{
    public class SendSettingViewModel : BaseViewModel
    {
        #region Properties

        /// <summary>
        /// Param
        /// </summary>
        private SendSettingParamViewModel _sendSetting;

        /// <summary>
        /// Param
        /// </summary>
        public SendSettingParamViewModel SendSetting
        {
            get { return _sendSetting; }

            set { this.SetProperty(ref this._sendSetting, value); }
        }

        /// <summary>
        /// Devices
        /// </summary>
        private List<ComboBoxItemViewModel> _encryptionMethods;

        /// <summary>
        /// Devices
        /// </summary>
        public List<ComboBoxItemViewModel> EncryptionMethods
        {
            get { return _encryptionMethods; }

            set { this.SetProperty(ref this._encryptionMethods, value); }
        }

        /// <summary>
        /// Device selected
        /// </summary>
        private ComboBoxItemViewModel _encryptionMethodSelected;

        /// <summary>
        /// Device selected
        /// </summary>
        public ComboBoxItemViewModel EncryptionMethodSelected
        {
            get { return _encryptionMethodSelected; }
            set
            {
                if (value != null)
                {
                    SendSetting.EncryptionMethod = value.Value.ToString();
                }
                this.SetProperty(ref this._encryptionMethodSelected, value);
            }
        }
        #endregion

        #region Command

        public ICommand OkCommand { get; }

        #endregion

        private ISaveSettingsWrapper _pSaveSettingsWrapper;

        public SendSettingViewModel(ContentPage owner) : base(owner)
        {
            OkCommand = new Command(Ok);

            _pSaveSettingsWrapper = DependencyService.Get<ISaveSettingsWrapper>();

            GetEncryptionMethods();
            GetSetting();
        }

        private void GetSetting()
        {
            SendSetting = new SendSettingParamViewModel();

            SendSetting.Port = _pSaveSettingsWrapper.GetString(ReceivingManagementSystem.Common.Constants.Setting_Send_Port, null);
            SendSetting.EncryptionMethod = _pSaveSettingsWrapper.GetString(ReceivingManagementSystem.Common.Constants.Setting_Send_Encryption_Method, string.Empty);
            SendSetting.RequiresAuthentication = _pSaveSettingsWrapper.GetBool(ReceivingManagementSystem.Common.Constants.Setting_Send_Requires_Authentication,false);
            SendSetting.Password = _pSaveSettingsWrapper.GetString(ReceivingManagementSystem.Common.Constants.Setting_Send_Password, string.Empty);
            SendSetting.UserName = _pSaveSettingsWrapper.GetString(ReceivingManagementSystem.Common.Constants.Setting_Send_User_Name, string.Empty);
            SendSetting.Server = _pSaveSettingsWrapper.GetString(ReceivingManagementSystem.Common.Constants.Setting_Send_Server, string.Empty);
            SendSetting.MailTo = _pSaveSettingsWrapper.GetString(ReceivingManagementSystem.Common.Constants.Setting_Send_Mail_To, string.Empty);

            if (SendSetting.EncryptionMethod != null)
            {
                EncryptionMethodSelected = _encryptionMethods.FirstOrDefault(r => r.Value.ToString().Equals(SendSetting.EncryptionMethod));
            }
        }

        private void GetEncryptionMethods()
        {
            EncryptionMethods = new List<ComboBoxItemViewModel>();
            EncryptionMethods.Add(new ComboBoxItemViewModel()
            {
                DisplayValue = "SSL",
                Value = "SSL",
            });

            EncryptionMethods.Add(new ComboBoxItemViewModel()
            {
                DisplayValue = "TLS",
                Value = "TLS",
            });
        }

        private async void Ok()
        {
            List<SaveSettingsParam> settingsParams = new List<SaveSettingsParam>();

            settingsParams.Add(new SaveSettingsParam(SaveSettingsParam.SaveTypes.STRING, 
                ReceivingManagementSystem.Common.Constants.Setting_Send_User_Name, _sendSetting.UserName));
            settingsParams.Add(new SaveSettingsParam(SaveSettingsParam.SaveTypes.STRING, 
                ReceivingManagementSystem.Common.Constants.Setting_Send_Password, _sendSetting.Password));
            settingsParams.Add(new SaveSettingsParam(SaveSettingsParam.SaveTypes.STRING, 
                ReceivingManagementSystem.Common.Constants.Setting_Send_Server, _sendSetting.Server));
            settingsParams.Add(new SaveSettingsParam(SaveSettingsParam.SaveTypes.STRING,
                ReceivingManagementSystem.Common.Constants.Setting_Send_Port, _sendSetting.Port));
            settingsParams.Add(new SaveSettingsParam(SaveSettingsParam.SaveTypes.STRING, 
                ReceivingManagementSystem.Common.Constants.Setting_Send_Encryption_Method, _sendSetting.EncryptionMethod));
            settingsParams.Add(new SaveSettingsParam(SaveSettingsParam.SaveTypes.BOOL, 
                ReceivingManagementSystem.Common.Constants.Setting_Send_Requires_Authentication, _sendSetting.RequiresAuthentication));
            settingsParams.Add(new SaveSettingsParam(SaveSettingsParam.SaveTypes.STRING,
                ReceivingManagementSystem.Common.Constants.Setting_Send_Mail_To, _sendSetting.MailTo));

            var result = _pSaveSettingsWrapper.SaveSettings(settingsParams.ToArray());

            if (!result)
            {
                await ShowAlert(TextResourceKey.NotificationTitle, TextResourceKey.OK,
                     MessageResourceKey.E0003, TextResourceKey.Setting);
            }
            else
            {
                ShowMessage(MessageResourceKey.E0004, TextResourceKey.Setting);
                Close();
            }
        }
    }
}
