using CsvHelper;
using ReceivingManagementSystem.Common.Resources;
using ReceivingManagementSystem.Android.Orders;
using ReceivingManagementSystem.Android.Services.Pleasanter;
using ReceivingManagementSystem.Android.Services.Rfid;
using ReceivingManagementSystem.Views.Android.Settings;
using ReceivingManagementSystem.Android.Interfaces;
using ReceivingManagementSystem.Android.Interfaces.Models;
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
    public class SoundSettingViewModel : BaseViewModel
    {
        #region Properties

        /// <summary>
        /// Param
        /// </summary>
        private SoundSettingParamViewModel _soundSetting;

        /// <summary>
        /// Param
        /// </summary>
        public SoundSettingParamViewModel SoundSetting
        {
            get { return _soundSetting; }

            set { this.SetProperty(ref this._soundSetting, value); }
        }
        #endregion

        #region Command

        public ICommand OkCommand { get; }
        public ICommand ChooseFileCommand { get; }

        #endregion

        private ISaveSettingsWrapper _pSaveSettingsWrapper;
        private IFileWrapper _fileWrapper;
        private IBeepAudioTracks _beepAudioTracks;

        public SoundSettingViewModel(ContentPage owner) : base(owner)
        {
            OkCommand = new Command(Ok);
            ChooseFileCommand = new Command<string>(ChooseFile);

            _pSaveSettingsWrapper = DependencyService.Get<ISaveSettingsWrapper>();
            _fileWrapper = DependencyService.Get<IFileWrapper>();
            _beepAudioTracks = DependencyService.Get<IBeepAudioTracks>();

            GetSetting();
        }

        private void GetSetting()
        {
            SoundSetting = new SoundSettingParamViewModel();

            SoundSetting.Track1 = Path.GetFileName(_pSaveSettingsWrapper.GetString(ReceivingManagementSystem.Common.Constants.Setting_Sound_Track1, "track1.wav"));
            SoundSetting.Track2 = Path.GetFileName(_pSaveSettingsWrapper.GetString(ReceivingManagementSystem.Common.Constants.Setting_Sound_Track2, "track2.wav"));
            SoundSetting.Track3 = Path.GetFileName(_pSaveSettingsWrapper.GetString(ReceivingManagementSystem.Common.Constants.Setting_Sound_Track3, "track3.wav"));
            SoundSetting.Track4 = Path.GetFileName(_pSaveSettingsWrapper.GetString(ReceivingManagementSystem.Common.Constants.Setting_Sound_Track4, "track4.wav"));
            SoundSetting.Track5 = Path.GetFileName(_pSaveSettingsWrapper.GetString(ReceivingManagementSystem.Common.Constants.Setting_Sound_Track5, "track5.wav"));
        }

        private async void ChooseFile(string type)
        {
            FileModel file = await _fileWrapper.ChooseFile(".wav");

            if (file == null)
            {
                return;
            }

            switch (type)
            {
                case "1":
                    {
                        SoundSetting.Track1 = file.FileName;
                        SoundSetting.TrackFile1 = file;
                        break;
                    }
                case "2":
                    {
                        SoundSetting.Track2 = file.FileName;
                        SoundSetting.TrackFile2 = file;
                        break;
                    }
                case "3":
                    {
                        SoundSetting.Track3 = file.FileName;
                        SoundSetting.TrackFile3 = file;
                        break;
                    }
                case "4":
                    {
                        SoundSetting.Track4 = file.FileName;
                        SoundSetting.TrackFile4 = file;
                        break;
                    }
                case "5":
                    {
                        SoundSetting.Track5 = file.FileName;
                        SoundSetting.TrackFile5 = file;
                        break;
                    }
            }
        }

        private async void Ok()
        {
            List<SaveSettingsParam> settingsParams = new List<SaveSettingsParam>();

            if (_soundSetting.TrackFile1 != null)
            {
                string filePath1 = await _fileWrapper.SaveFile(_soundSetting.TrackFile1);
                settingsParams.Add(new SaveSettingsParam(SaveSettingsParam.SaveTypes.STRING,
                    ReceivingManagementSystem.Common.Constants.Setting_Sound_Track1, filePath1));
            }
            if (_soundSetting.TrackFile2 != null)
            {
                string filePath2 = await _fileWrapper.SaveFile(_soundSetting.TrackFile2);
                settingsParams.Add(new SaveSettingsParam(SaveSettingsParam.SaveTypes.STRING,
                    ReceivingManagementSystem.Common.Constants.Setting_Sound_Track2, filePath2));
            }

            if (_soundSetting.TrackFile3 != null)
            {
                string filePath3 = await _fileWrapper.SaveFile(_soundSetting.TrackFile3);
                settingsParams.Add(new SaveSettingsParam(SaveSettingsParam.SaveTypes.STRING,
                   ReceivingManagementSystem.Common.Constants.Setting_Sound_Track3, filePath3));
            }

            if (_soundSetting.TrackFile4 != null)
            {
                string filePath4 = await _fileWrapper.SaveFile(_soundSetting.TrackFile4);
                settingsParams.Add(new SaveSettingsParam(SaveSettingsParam.SaveTypes.STRING,
                   ReceivingManagementSystem.Common.Constants.Setting_Sound_Track4, filePath4));
            }

            if (_soundSetting.TrackFile5 != null)
            {
                string filePath5 = await _fileWrapper.SaveFile(_soundSetting.TrackFile5);
                settingsParams.Add(new SaveSettingsParam(SaveSettingsParam.SaveTypes.STRING,
                   ReceivingManagementSystem.Common.Constants.Setting_Sound_Track5, filePath5));
            }

            var result = _pSaveSettingsWrapper.SaveSettings(settingsParams.ToArray());

            if (!result)
            {
                await ShowAlert(TextResourceKey.NotificationTitle, TextResourceKey.OK,
                     MessageResourceKey.E0003, TextResourceKey.Setting);
            }
            else
            {
                _beepAudioTracks.SetupAudioTracks();
                ShowMessage(MessageResourceKey.E0004, TextResourceKey.Setting);
                Close(true);
            }
        }
    }
}
