using ReceivingManagementSystem.Wrapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DensoScannerSDK.Barcode;
using DensoScannerSDK.Common;
using DensoScannerSDK.Dto;
using DensoScannerSDK.RFID;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.Reflection;

namespace ReceivingManagementSystem.Views.Settings
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SettingDevicePage : ContentPage
    {
        ICommManagerWrapper m_pCommManagerWrapper = DependencyService.Get<ICommManagerWrapper>();
        ICommonUtilWrapper m_pCommonUtilWrapper = DependencyService.Get<ICommonUtilWrapper>();
        CommonBase m_hCommonBase = CommonBase.GetInstance();

        private bool disposeFlg = true;

        private CommScannerParams commParams;
        private RFIDScannerSettings rfidSettings;
        private bool isAutoLinkProfileEnabled = false;
        private BarcodeScannerSettings barcodeSettings;

        // ブザー音量
        // Buzzer volume
        Dictionary<string, CommScannerParams.BuzzerVolume> buzzerVolumeMap = new Dictionary<string, CommScannerParams.BuzzerVolume>();
        // トリガモード
        // Trigger mode
        Dictionary<string, BarcodeScannerSettings.Scan.TriggerMode> triggerModeMap = new Dictionary<string, BarcodeScannerSettings.Scan.TriggerMode>();
        // 偏波設定
        // Polarization setting
        Dictionary<string, RFIDScannerSettings.Scan.Polarization> polarizationMap = new Dictionary<String, RFIDScannerSettings.Scan.Polarization>();

        // この画面が生成された時に、アプリがスキャナと接続されていたかどうか
        // この画面にいる間に接続が切断された場合でも、この画面の生成時にスキャナと接続されていた場合は通信エラーが出るようにする
        // Whether the application was connected to the scanner when this screen was generated.
        // Even if the connection is disconnected while on this screen, make sure that a communication error occurs when connected to the scanner when generating this screen.
        private bool scannerConnectedOnCreate = false;
        Assembly m_pAssembly = null;

        public SettingDevicePage()
        {
            InitializeComponent();

            m_pAssembly = this.GetType().Assembly;

            #region 文言系
            //title_settings.Text = AppResources.settings;
            text_scanner_version_head.Text = AppResources.scanner_version;
            text_scanner_version_value.Text = AppResources.number_0_00;

            text_read_power_level_head.Text = AppResources.read_power_level;
            for (int i = 4; i <= 30; i++)
            {
                text_read_power_level_value.Items.Add(i.ToString());
            }
            text_read_power_level_value.SelectedIndex = 0;

            if ( Device.RuntimePlatform == Device.UWP)
            {
                text_read_power_level_value.Title = "";
            }
            else
            {
                text_read_power_level_value.Title = AppResources.read_power_level;
            }

            text_read_power_level_unit.Text = AppResources.read_power_level_unit;

            text_session_head.Text = AppResources.session;

            if (Device.RuntimePlatform == Device.UWP)
            {
                text_session_value.Title = "";
            }
            else
            {
                text_session_value.Title = AppResources.session;
            }

            text_session_value.Items.Add(AppResources.session_s0);
            text_session_value.Items.Add(AppResources.session_s1);
            text_session_value.Items.Add(AppResources.session_s2);
            text_session_value.Items.Add(AppResources.session_s3);

            text_session_value.SelectedIndex = 0;

            text_report_unique_tags_head.Text = AppResources.report_unique_tags;

            text_channel_head.Text = AppResources.channel;

            text_channel5.Text = AppResources.channel5;
            text_channel11.Text = AppResources.channel11;
            text_channel17.Text = AppResources.channel17;
            text_channel23.Text = AppResources.channel23;
            text_channel24.Text = AppResources.channel24;
            text_channel25.Text = AppResources.channel25;

            text_q_factor_head.Text = AppResources.q_factor;

            if ( Device.RuntimePlatform == Device.UWP )
            {
                text_q_factor_value.Title = "";

            }
            else
            {
                text_q_factor_value.Title = AppResources.q_factor;
            }
            for (int i = 0; i <= 7; i++)
            {
                text_q_factor_value.Items.Add(i.ToString());
            }
            text_q_factor_value.SelectedIndex = 0;

            text_auto_link_profile_head.Text = AppResources.auto_link_profile;

            text_link_profile_head.Text = AppResources.link_profile;

            if ( Device.RuntimePlatform == Device.UWP )
            {
                text_link_profile_value.Title = "";

            }
            else
            {
                text_link_profile_value.Title = AppResources.link_profile;

            }

            text_link_profile_value.Items.Add(AppResources.link_profile_1);
            text_link_profile_value.Items.Add(AppResources.link_profile_4);
            text_link_profile_value.Items.Add(AppResources.link_profile_5);
            text_link_profile_value.SelectedIndex = 0;

            text_polarization_head.Text = AppResources.polarization;

            if ( Device.RuntimePlatform == Device.UWP )
            {
                text_polarization_value.Title = "";
            }
            else
            {
                text_polarization_value.Title = AppResources.polarization;
            }

            text_polarization_value.Items.Add(AppResources.polarization_vertical);
            text_polarization_value.Items.Add(AppResources.polarization_horizontal);
            text_polarization_value.Items.Add(AppResources.polarization_both);
            text_polarization_value.SelectedIndex = 2;

            text_power_save_head.Text = AppResources.power_save;
            text_buzzer_head.Text = AppResources.buzzer;
            text_buzzer_volume_head.Text = AppResources.buzzer_volume;

            if ( Device.RuntimePlatform == Device.UWP )
            {
                text_buzzer_volume_value.Title = "";
            }
            else
            {
                text_buzzer_volume_value.Title = AppResources.buzzer_volume;
            }

            text_buzzer_volume_value.Items.Add(AppResources.buzzer_volume_low);
            text_buzzer_volume_value.Items.Add(AppResources.buzzer_volume_middle);
            text_buzzer_volume_value.Items.Add(AppResources.buzzer_volume_loud);
            text_buzzer_volume_value.SelectedIndex = 2;

            text_barcode_head.Text = AppResources.barcode_uppercase;
            text_trigger_mode_head.Text = AppResources.trigger_mode;

            if ( Device.RuntimePlatform == Device.UWP )
            {
                text_trigger_mode_value.Title = "";
            }
            else
            {
                text_trigger_mode_value.Title = AppResources.trigger_mode;
            }

            text_trigger_mode_value.Items.Add(AppResources.trigger_mode_auto_off);
            text_trigger_mode_value.Items.Add(AppResources.trigger_mode_momentary);
            text_trigger_mode_value.Items.Add(AppResources.trigger_mode_alternate);
            text_trigger_mode_value.Items.Add(AppResources.trigger_mode_continuous);
            text_trigger_mode_value.Items.Add(AppResources.trigger_mode_trigger_release);

            text_trigger_mode_value.SelectedIndex = 0;

            text_enable_all_1d_codes_head.Text = AppResources.enable_all_1d_codes;
            text_enable_all_2d_codes_head.Text = AppResources.enable_all_2d_codes;

            image_scanner_battery.Source = ImageSource.FromResource("ReceivingManagementSystem.Resource.battery_1.png", m_pAssembly);
            
            #endregion

            #region Android限定イベント Android event
            ICommonEventWrapper pEvent = DependencyService.Get<ICommonEventWrapper>();
            pEvent.OnUserLeaveHint += OnUserLeaveHint;
            pEvent.OnRestart += OnRestart;
            #endregion
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            scannerConnectedOnCreate = m_hCommonBase.IsCommScanner();

            bool result = true;

            if (scannerConnectedOnCreate)
            {
                // SP1の情報を読み込む
                // Read information on SP1
                result = LoadScannerInfo();
            }
            else
            {
                // SP1が見つからなかったときはエラーメッセージ表示
                // Show error message if SP1 is not found.
                m_pCommonUtilWrapper.ShowMessage(AppResources.E_MSG_NO_CONNECTION);
                result = false; // 未接続 disconnected
            }

            // 設定値を読み込む
            // Read the setting value
            if (result == true)
            {
                LoadSettings();
            }

            // バックグラウンドでサービス起動
            // Service start up in the background
            m_pCommonUtilWrapper.StartService();
        }

        protected override void OnDisappearing()
        {
            if (scannerConnectedOnCreate && disposeFlg)
            {
                //ここで接続解除される
                m_hCommonBase.DisconnectCommScanner();
            }

            #region Android限定イベントの解除 Remove Android event listener
            ICommonEventWrapper pEvent = DependencyService.Get<ICommonEventWrapper>();
            pEvent.OnUserLeaveHint -= OnUserLeaveHint;
            pEvent.OnRestart -= OnRestart;
            #endregion

            base.OnDisappearing();
        }

        /// <summary>
        ///  Android限定のイベント Android event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnUserLeaveHint(object sender, EventArgs e)
        {
            if (disposeFlg)
            {
                // SP1設定値送信・保存
                // SP1 Send / save value
                Save();
                disposeFlg = false;
            }
        }

        /// <summary>
        /// Android限定のイベント
        /// </summary>
        private void OnRestart(object sender, EventArgs e)
        {
            disposeFlg = true;
        }

        protected override bool OnBackButtonPressed()
        {
            // SP1設定値送信・保存
            // SP1 Send / save value
            Save();
            disposeFlg = false;

            // 親のActivityに戻るため不要になるのでActivityを停止する
            // Stop Activity as it becomes unnecessary to return to parent's Activity.
            this.Navigation.PopAsync();

            return true;
        }

        private void TapGestureRecognizer_Tapped(object sender, EventArgs e)
        {
            NavigateUp();
        }

        /**
         * 画面遷移でいう上の階層に移動する
         * Move to the upper level in the screen transition.
         */
        private void NavigateUp()
        {
            // SP1設定値送信・保存
            // SP1 Send / save value
            Save();
            disposeFlg = false;

            // 親のActivityに戻るため不要になるのでActivityを停止する
            // Stop Activity as it becomes unnecessary to return to parent's Activity.
            this.Navigation.PopModalAsync();
        }

        /**
         * スキャナーの情報を読み込む
         * Read information on scanner.
         */
        private bool LoadScannerInfo()
        {
            // SP1のバージョン取得
            // Obtain version of SP1
            try
            {
                string ver = m_hCommonBase.GetCommScanner().GetVersion();
                text_scanner_version_value.Text = ver.Replace("Ver.", "");
            }
            catch (Exception /* e */)
            {
                return false;
            }

            // SP1のバッテリー残量を読み込む
            // Read the battery level of SP1
            DensoScannerSDK.CommConst.CommBattery? battery = null;
            try
            {
                battery = m_hCommonBase.GetCommScanner().GetRemainingBattery();
            }
            catch (CommException e)
            {
                System.Diagnostics.Debug.WriteLine(e.StackTrace);
            }
            if (battery != null)
            {
                string batteryResId = "";
                switch (battery)
                {
                    case DensoScannerSDK.CommConst.CommBattery.UNDER10:
                        batteryResId = "ReceivingManagementSystem.Resource.battery_1.png";
                        break;
                    case DensoScannerSDK.CommConst.CommBattery.UNDER40:
                        batteryResId = "ReceivingManagementSystem.Resource.battery_2.png";
                        break;
                    case DensoScannerSDK.CommConst.CommBattery.OVER40:
                        batteryResId = "ReceivingManagementSystem.Resource.battery_full.png";
                        break;
                }

                image_scanner_battery.Source = ImageSource.FromResource(batteryResId, m_pAssembly);
            }
            else
            {
                //showMessage("バッテリー残量取得失敗");
                return false;
            }

            return true;
        }

        /**
         * 設定値の読み込み
         * SharedPreferencesからアプリ内に保存している設定値を読み込み、各UIに適用する
         * Load setting
         * Load the setting values saved in the application from SharedPreferences and apply them to each UI
         */
        private void LoadSettings()
        {
            // Map作成
            // Create Map
            SetMap();

            try
            {
                // RFID関連設定値を取得
                // Acquire RFID related setting value
                rfidSettings = m_hCommonBase.GetCommScanner().GetRFIDScanner().GetSettings();

                // 共通パラメータの設定値を取得
                // Acquire common parameter setting value
                commParams = m_hCommonBase.GetCommScanner().GetParams();

                // バーコード関連設定値を取得
                // Acquire bar code related setting value
                barcodeSettings = m_hCommonBase.GetCommScanner().GetBarcodeScanner().GetSettings();

            }
            catch (Exception e)
            {
                if (e is CommException || e is RFIDException || e is BarcodeException)
                {
                    this.rfidSettings = null;
                    this.commParams = null;
                    this.barcodeSettings = null;
                    m_pCommonUtilWrapper.ShowMessage(AppResources.E_MSG_COMMUNICATION);
                    System.Diagnostics.Debug.WriteLine(e.StackTrace);
                    return;
                }
            }

            try
            {
                //Auto Link Profile
                isAutoLinkProfileEnabled = m_hCommonBase.GetCommScanner().GetRFIDScanner().GetAutoLinkProfile();
            }
            catch (Exception e)
            {
                if (e is RFIDException && !(((RFIDException)e).GetErrorCode() == DensoScannerSDK.Exception.ErrorCode.NOT_SUPPORT_COMMAND))
                {
                    m_pCommonUtilWrapper.ShowMessage(AppResources.E_MSG_COMMUNICATION);
                    System.Diagnostics.Debug.WriteLine(e.StackTrace);
                    return;
                }
            }

            // [RFID関連設定値] [RFID related setting value]
            // read_power_level
            text_read_power_level_value.SelectedItem = rfidSettings.scan.powerLevelRead.ToString();

            // session
            text_session_value.SelectedItem = rfidSettings.scan.sessionFlag.ToString();

            // report unigue tags
            bool unigue = (true == this.rfidSettings.scan.doubleReading.Equals(RFIDScannerSettings.Scan.DoubleReading.PREVENT1)) ? true : false;
            checkbox_report_unique_tags.IsToggled = unigue;

            // channel5 : 0x00000001
            bool onoff = ((this.rfidSettings.scan.channel & 0x00000001) > 0) ? true : false;
            checkbox_channel5.IsToggled = onoff;

            // channel11 : 0x00000002
            onoff = ((this.rfidSettings.scan.channel & 0x00000002) > 0) ? true : false;
            checkbox_channel11.IsToggled = onoff;

            // channel17 : 0x00000004
            onoff = ((this.rfidSettings.scan.channel & 0x00000004) > 0) ? true : false;
            checkbox_channel17.IsToggled = onoff;

            // channel23 : 0x00000008
            onoff = ((this.rfidSettings.scan.channel & 0x00000008) > 0) ? true : false;
            checkbox_channel23.IsToggled = onoff;

            // channel24 : 0x00000010
            onoff = ((this.rfidSettings.scan.channel & 0x00000010) > 0) ? true : false;
            checkbox_channel24.IsToggled = onoff;

            // channel25 : 0x00000020
            onoff = ((this.rfidSettings.scan.channel & 0x00000020) > 0) ? true : false;
            checkbox_channel25.IsToggled = onoff;

            // q_factor
            text_q_factor_value.SelectedItem = rfidSettings.scan.qParam.ToString();

            //auto_link_profile
            switch_auto_link_profile.IsToggled = isAutoLinkProfileEnabled;

            // link_profile
            // valueOf?
            text_link_profile_value.SelectedItem = rfidSettings.scan.linkProfile.ToString();

            // polarization
            string strPolarization = "";
            if (this.polarizationMap.ContainsValue(this.rfidSettings.scan.polarization) == true)
            {
                foreach (KeyValuePair<string, RFIDScannerSettings.Scan.Polarization> pEntry in polarizationMap)
                {
                    if (pEntry.Value.Equals(rfidSettings.scan.polarization))
                    {
                        strPolarization = pEntry.Key;
                        break;
                    }
                }
            }

            text_polarization_value.SelectedItem = strPolarization;

            // [共通パラメータの設定値] [Common parameter setting value]
            // power_save
            checkbox_power_save.IsToggled = commParams.powerSave;

            // buzzer
            onoff = (this.commParams.notification.sound.buzzer.Equals(CommScannerParams.Notification.Sound.Buzzer.ENABLE)) ? true : false;
            checkbox_buzzer.IsToggled = onoff;

            // buzzer_volumes
            string strBuzzerVolume = "";
            if (buzzerVolumeMap.ContainsValue(commParams.buzzerVolume) == true)
            {
                foreach (KeyValuePair<string, CommScannerParams.BuzzerVolume> pEntry in buzzerVolumeMap)
                {
                    if (pEntry.Value.Equals(commParams.buzzerVolume))
                    {
                        strBuzzerVolume = pEntry.Key;
                        break;
                    }
                }
            }

            text_buzzer_volume_value.SelectedItem = strBuzzerVolume;

            // [バーコード関連設定値] [Bar code related setting value]
            // trigger_mode
            String strTriggerMode = "";
            if (triggerModeMap.ContainsValue(this.barcodeSettings.scan.triggerMode) == true)
            {
                foreach (KeyValuePair<string, BarcodeScannerSettings.Scan.TriggerMode> pEntry in triggerModeMap)
                {
                    if (pEntry.Value.Equals(barcodeSettings.scan.triggerMode))
                    {
                        strTriggerMode = pEntry.Key;
                        break;
                    }
                }
            }

            text_trigger_mode_value.SelectedItem = strTriggerMode;

            // enable_all_1d_codes
            onoff = CheckEnable1dCodes(barcodeSettings);
            checkbox_enable_all_1d_codes.IsToggled = onoff;

            // enable_all_2d_codes
            onoff = CheckEnable2dCodes(barcodeSettings);
            checkbox_enable_all_2d_codes.IsToggled = onoff;

        }

        /**
         * Map作成
         * Create Map
         * @param
         */
        private void SetMap()
        {
            // ブザー音量Map
            // Buzzer volume map
            buzzerVolumeMap.Add(AppResources.buzzer_volume_low, CommScannerParams.BuzzerVolume.LOW);
            buzzerVolumeMap.Add(AppResources.buzzer_volume_middle, CommScannerParams.BuzzerVolume.MIDDLE);
            buzzerVolumeMap.Add(AppResources.buzzer_volume_loud, CommScannerParams.BuzzerVolume.LOUD);

            // バーコードトリガモードMap
            // bar code trigger mode map.
            triggerModeMap.Add(AppResources.trigger_mode_auto_off, BarcodeScannerSettings.Scan.TriggerMode.AUTO_OFF);
            triggerModeMap.Add(AppResources.trigger_mode_momentary, BarcodeScannerSettings.Scan.TriggerMode.MOMENTARY);
            triggerModeMap.Add(AppResources.trigger_mode_alternate, BarcodeScannerSettings.Scan.TriggerMode.ALTERNATE);
            triggerModeMap.Add(AppResources.trigger_mode_continuous, BarcodeScannerSettings.Scan.TriggerMode.CONTINUOUS);
            triggerModeMap.Add(AppResources.trigger_mode_trigger_release, BarcodeScannerSettings.Scan.TriggerMode.TRIGGER_RELEASE);

            // 偏波設定Map
            // Polarization setting map
            polarizationMap.Add(AppResources.polarization_vertical, RFIDScannerSettings.Scan.Polarization.V);
            polarizationMap.Add(AppResources.polarization_horizontal, RFIDScannerSettings.Scan.Polarization.H);
            polarizationMap.Add(AppResources.polarization_both, RFIDScannerSettings.Scan.Polarization.Both);
        }

        /**
         * 1次元バーコードの読取許可・禁止を設定する
         * Setting permission / prohibition of reading of one-dimensional barcode
         * @param settings　バーコード設定 Bar code setting
         */
        private bool CheckEnable1dCodes(BarcodeScannerSettings settings)
        {
            // EAN-13 UPC-A
            if (settings.decode.symbologies.ean13upcA.enabled == false)
            {
                return false;
            }
            // EAN-8
            if (settings.decode.symbologies.ean8.enabled == false)
            {
                return false;
            }
            // UPC-E
            if (settings.decode.symbologies.upcE.enabled == false)
            {
                return false;
            }
            // ITF
            if (settings.decode.symbologies.itf.enabled == false)
            {
                return false;
            }
            // STF
            if (settings.decode.symbologies.stf.enabled == false)
            {
                return false;
            }
            // Codabar
            if (settings.decode.symbologies.codabar.enabled == false)
            {
                return false;
            }
            // Code39
            if (settings.decode.symbologies.code39.enabled == false)
            {
                return false;
            }
            // Code93
            if (settings.decode.symbologies.code93.enabled == false)
            {
                return false;
            }
            // Code128
            if (settings.decode.symbologies.code128.enabled == false)
            {
                return false;
            }
            // GS1 Databar
            if (settings.decode.symbologies.gs1DataBar.enabled == false)
            {
                return false;
            }
            // GS1 Databar Limited
            if (settings.decode.symbologies.gs1DataBarLimited.enabled == false)
            {
                return false;
            }
            // GS1 Databar Expanded
            if (settings.decode.symbologies.gs1DataBarExpanded.enabled == false)
            {
                return false;
            }

            return true;
        }

        /**
         * 2次元バーコードの読取許可・禁止を設定する
         * Setting permission / prohibition of reading of two-dimensional barcode
         * @param settings　バーコード設定 Bar code setting
         */
        private bool CheckEnable2dCodes(BarcodeScannerSettings settings)
        {
            // QR Code
            if (settings.decode.symbologies.qrCode.enabled == false)
            {
                return false;
            }
            // QR Code.Model1
            if (settings.decode.symbologies.qrCode.model1.enabled == false)
            {
                return false;
            }
            // QR Code.Model2
            if (settings.decode.symbologies.qrCode.model2.enabled == false)
            {
                return false;
            }
            // QR Code.Micro QR
            if (settings.decode.symbologies.microQr.enabled == false)
            {
                return false;
            }
            // iQR Code
            if (settings.decode.symbologies.iqrCode.enabled == false)
            {
                return false;
            }
            // iQR Code.Square
            if (settings.decode.symbologies.iqrCode.square.enabled == false)
            {
                return false;
            }
            // iQR Code.Rectangle
            if (settings.decode.symbologies.iqrCode.rectangle.enabled == false)
            {
                return false;
            }
            // Data Matrix
            if (settings.decode.symbologies.dataMatrix.enabled == false)
            {
                return false;
            }
            // Data Matrix.Square
            if (settings.decode.symbologies.dataMatrix.square.enabled == false)
            {
                return false;
            }
            // Data Matrix.Rectangle
            if (settings.decode.symbologies.dataMatrix.rectangle.enabled == false)
            {
                return false;
            }
            // PDF417
            if (settings.decode.symbologies.pdf417.enabled == false)
            {
                return false;
            }
            // Micro PDF417
            if (settings.decode.symbologies.microPdf417.enabled == false)
            {
                return false;
            }
            // Maxi Code
            if (settings.decode.symbologies.maxiCode.enabled == false)
            {
                return false;
            }
            // GS1 Composite
            if (settings.decode.symbologies.gs1Composite.enabled == false)
            {
                return false;
            }

            return true;
        }

        /**
         *　RFIDScannerSettingsの設定値をコマンド送信する(コマンド：setSettings)
         *　Send command set value of RFIDS scanner settings (command: setSettings)
         */
        private void SendRFIDScannerSettings()
        {
            try
            {
                // powerLevelRead
                rfidSettings.scan.powerLevelRead = int.Parse((string)(text_read_power_level_value.SelectedItem));

                // session
                string strSelectedItem = "";
                strSelectedItem = (string)(text_session_value.SelectedItem);
                if (strSelectedItem == AppResources.session_s0)
                {
                    rfidSettings.scan.sessionFlag = RFIDScannerSettings.Scan.SessionFlag.S0;
                }
                else if (strSelectedItem == AppResources.session_s1)
                {
                    rfidSettings.scan.sessionFlag = RFIDScannerSettings.Scan.SessionFlag.S1;
                }
                else if (strSelectedItem == AppResources.session_s2)
                {
                    rfidSettings.scan.sessionFlag = RFIDScannerSettings.Scan.SessionFlag.S2;
                }
                else if (strSelectedItem == AppResources.session_s3)
                {
                    rfidSettings.scan.sessionFlag = RFIDScannerSettings.Scan.SessionFlag.S3;
                }

                // report unigue tags
                RFIDScannerSettings.Scan.DoubleReading doubleReading;
                if (checkbox_report_unique_tags.IsToggled == true)
                {
                    doubleReading = RFIDScannerSettings.Scan.DoubleReading.PREVENT1;
                }
                else
                {
                    doubleReading = RFIDScannerSettings.Scan.DoubleReading.Free;
                }
                rfidSettings.scan.doubleReading = doubleReading;

                // channel(channel5～25)
                long channel = 0L;
                // channel5
                if (checkbox_channel5.IsToggled == true)
                {
                    channel += 1L;
                }
                // channel11
                if (checkbox_channel11.IsToggled == true)
                {
                    channel += 2L;
                }
                // channel17
                if (checkbox_channel17.IsToggled == true)
                {
                    channel += 4L;
                }
                // channel23
                if (checkbox_channel23.IsToggled == true)
                {
                    channel += 8L;
                }
                // channel24
                if (checkbox_channel24.IsToggled == true)
                {
                    channel += 16L;
                }
                // channel25
                if (checkbox_channel25.IsToggled == true)
                {
                    channel += 32L;
                }
                this.rfidSettings.scan.channel = channel;

                // q factor
                rfidSettings.scan.qParam = short.Parse((string)(text_q_factor_value.SelectedItem));

                // link profile
                rfidSettings.scan.linkProfile = short.Parse((string)(text_link_profile_value.SelectedItem));

                // polarization
                string strSelectedPolarization = (string)text_polarization_value.SelectedItem;
                if (polarizationMap.ContainsKey(strSelectedPolarization) == true)
                {
                    rfidSettings.scan.polarization = polarizationMap[strSelectedPolarization];
                }

                // SP1設定値送信
                // Send setting value to SP1
                m_hCommonBase.GetCommScanner().GetRFIDScanner().SetSettings(this.rfidSettings);
            }
            catch (Exception e)
            {
                if (e is CommException || e is RFIDException)
                {
                    throw;
                }
            }
        }

        /**
         *　AutoLinkProfileの設定値をコマンド送信する(コマンド：setAutoLinkProfile)
         *　Send command set value of Auto Link Profile (command: setAutoLinkProfile)
         */
        private void SendAutoLinkProfile()
        {
            try
            {
                isAutoLinkProfileEnabled = switch_auto_link_profile.IsToggled;

                // SP1設定値送信
                // Send setting value to SP1
                m_hCommonBase.GetCommScanner().GetRFIDScanner().SetAutoLinkProfile(isAutoLinkProfileEnabled);
            }
            catch (Exception e)
            {
                if (e is CommException || e is RFIDException)
                {
                    throw;
                }
            }
        }

        /**
         * CommScannerParamsの設定値をコマンド送信する(コマンド：setParams, saveParams)
         * Send command of CommScannerParams setting value (command：setParams, saveParams)
         */
        private void SendCommScannerParams()
        {
            try
            {
                // powerSave
                commParams.powerSave = checkbox_power_save.IsToggled;

                // settings(オートパワーオフ)の設定
                // powerSaveとリンクさせる
                // auto power off setting
                // Link with powerSave

                // buzzer
                CommScannerParams.Notification.Sound.Buzzer buzzer;
                if (checkbox_buzzer.IsToggled == true)
                {
                    buzzer = CommScannerParams.Notification.Sound.Buzzer.ENABLE;
                }
                else
                {
                    buzzer = CommScannerParams.Notification.Sound.Buzzer.DISABLE;
                }

                commParams.notification.sound.buzzer = buzzer;

                // buzzer Volume
                string strSelectedItem = (string)(text_buzzer_volume_value.SelectedItem);

                if (buzzerVolumeMap.ContainsKey(strSelectedItem) == true)
                {
                    commParams.buzzerVolume = buzzerVolumeMap[strSelectedItem];
                }

                // SP1設定値送信
                // Send setting value to SP1
                m_hCommonBase.GetCommScanner().SetParams(this.commParams);

                // SP1設定値保存
                // Save SP1 setting value
                m_hCommonBase.GetCommScanner().SaveParams();
            }
            catch (CommException /* e */)
            {
                throw;
            }
        }

        /**
         * BarcodeScannerSettingsの設定値をコマンド送信する(コマンド：setSettings)
         * Send the setting value of BarcodeScannerSettings command (command: setSettings)
         */
        private void SendBarcodeScannerSettings()
        {
            try
            {
                // Trigger mode
                string strSelectedItem = (string)(text_trigger_mode_value.SelectedItem);
                if (triggerModeMap.ContainsKey(strSelectedItem) == true)
                {
                    this.barcodeSettings.scan.triggerMode = this.triggerModeMap[strSelectedItem];
                }

                // enable_all_1d_codes
                bool enable1dFlg = checkbox_enable_all_1d_codes.IsToggled;

                // enable_all_2d_codes
                bool enable2dFlg = checkbox_enable_all_2d_codes.IsToggled;

                // SP1設定値送信
                // Send setting value to SP1
                SetEnable1dCodes(this.barcodeSettings, enable1dFlg);
                SetEnable2dCodes(this.barcodeSettings, enable2dFlg);
                m_hCommonBase.GetCommScanner().GetBarcodeScanner().SetSettings(this.barcodeSettings);
            }
            catch (Exception e)
            {
                if (e is CommException || e is BarcodeException)
                {
                    throw;
                }
            }
        }

        /**
         * 1次元バーコードの読取許可・禁止を設定する
         * Setting permission / prohibition of reading of one-dimensional barcode
         * @param settings　バーコード設定 Bar code setting
         * @param enable1dFlg　許可・禁止  permission / prohibition
         */
        private void SetEnable1dCodes(BarcodeScannerSettings settings, bool enable1dFlg)
        {
            // EANコードはチェックの有無によらず常に許可する
            // Always allow EAN code regardless of checking
            settings.decode.symbologies.ean13upcA.enabled = true; // EAN-13 UPC-A
            settings.decode.symbologies.ean8.enabled = true; // EAN-8
            settings.decode.symbologies.upcE.enabled = enable1dFlg; // UPC-E
            settings.decode.symbologies.itf.enabled = enable1dFlg; // ITF
            settings.decode.symbologies.stf.enabled = enable1dFlg; // STF
            settings.decode.symbologies.codabar.enabled = enable1dFlg; // Codabar
            settings.decode.symbologies.code39.enabled = enable1dFlg; // Code39
            settings.decode.symbologies.code93.enabled = enable1dFlg; // Code93
            settings.decode.symbologies.code128.enabled = enable1dFlg; // Code128
            settings.decode.symbologies.msi.enabled = enable1dFlg; // MSI
            settings.decode.symbologies.gs1DataBar.enabled = enable1dFlg; // GS1 Databar
            settings.decode.symbologies.gs1DataBarLimited.enabled = enable1dFlg; // GS1 Databar Limited
            settings.decode.symbologies.gs1DataBarExpanded.enabled = enable1dFlg; // GS1 Databar Expanded
        }

        /**
         * 2次元バーコードの読取許可・禁止を設定する
         * Setting permission / prohibition of reading of two-dimensional barcode
         * @param settings　バーコード設定 Bar code setting
         * @param enable1dFlg　許可・禁止  permission / prohibition
         */
        private void SetEnable2dCodes(BarcodeScannerSettings settings, bool enable2dFlg)
        {
            settings.decode.symbologies.qrCode.enabled = enable2dFlg;   // QR Code
            settings.decode.symbologies.qrCode.model1.enabled = enable2dFlg;    // QR Code.Model1
            settings.decode.symbologies.qrCode.model2.enabled = enable2dFlg;    // QR Code.Model2
            settings.decode.symbologies.microQr.enabled = enable2dFlg; // QR Code.Micro QR
            settings.decode.symbologies.iqrCode.enabled = enable2dFlg; // iQR Code
            settings.decode.symbologies.iqrCode.square.enabled = enable2dFlg; // iQR Code.Square
            settings.decode.symbologies.iqrCode.rectangle.enabled = enable2dFlg; // iQR Code.Rectangle
            settings.decode.symbologies.dataMatrix.enabled = enable2dFlg; // Data Matrix
            settings.decode.symbologies.dataMatrix.square.enabled = enable2dFlg; // Data Matrix.Square
            settings.decode.symbologies.dataMatrix.rectangle.enabled = enable2dFlg; // Data Matrix.Rectangle
            settings.decode.symbologies.pdf417.enabled = enable2dFlg; // PDF417
            settings.decode.symbologies.microPdf417.enabled = enable2dFlg; // Micro PDF417
            settings.decode.symbologies.maxiCode.enabled = enable2dFlg; // Maxi Code
            settings.decode.symbologies.gs1Composite.enabled = enable2dFlg; // GS1 Composite
            settings.decode.symbologies.plessey.enabled = enable2dFlg;  // Plessey
            settings.decode.symbologies.aztec.enabled = enable2dFlg; // Aztec
        }

        /**
         * SP1設定値送信・保存
         * SP1 Send / save value
         */
        private void Save()
        {
            //bool isOK = false;
            // Setting画面遷移時にSP1との接続が無かった場合は処理を中断
            if (!scannerConnectedOnCreate)
            {
                return;
            }

            // SP1設定値送信・保存
            bool failedSettings = this.rfidSettings == null || this.commParams == null || this.barcodeSettings == null;
            Exception exception = null;
            if (!failedSettings)
            {
                try
                {
                    // スキャナと接続されているときに限り、設定中を示すtoastを表示
                    if (m_hCommonBase.IsCommScanner())
                    {
                        m_pCommonUtilWrapper.ShowMessage2(AppResources.I_MSG_PROGRESS_SETTING);
                    }

                    // RFIDScannerSettingsの設定値送信
                    SendRFIDScannerSettings();

                    // CommScannerParamsの設定値送信
                    SendCommScannerParams();

                    // BarcodeScannerSettingsの設定値送信
                    SendBarcodeScannerSettings();

                    //isOK = true;
                }
                catch (Exception e)
                {
                    if (e is CommException || e is RFIDException || e is BarcodeException)
                    {
                        failedSettings = true;
                        exception = e;
                    }
                }

                try
                {
                    // Auto Link Profileの設定値送信
                    SendAutoLinkProfile();
                    //isOK = true;
                }
                catch (Exception e)
                {
                    if (e is RFIDException && !(((RFIDException)e).GetErrorCode() == DensoScannerSDK.Exception.ErrorCode.NOT_SUPPORT_COMMAND))
                    {
                        failedSettings = true;
                        exception = e;
                    }
                }
            }

            if (failedSettings)
            {
                m_pCommonUtilWrapper.ShowMessage(AppResources.E_MSG_SAVE_SETTINGS);
                if (exception != null)
                {
                    System.Diagnostics.Debug.WriteLine(exception.StackTrace);
                }
                return;
            }

            // SP1のコマンド送受信に時間が掛かるため、待ってから画面を抜ける
            try
            {
                Thread.Sleep(2000);
            }
            catch (Exception /* e */)
            {

            }

            // 保存完了メッセージ
            if (this.commParams != null)
            {
                m_pCommonUtilWrapper.ShowMessage(AppResources.I_MSG_SAVE_SETTINGS);
                this.Navigation.PopAsync();
            }
        }

        private void btnSave_Clicked(object sender, EventArgs e)
        {
            Save();
        }

        private void btnCancel_Clicked(object sender, EventArgs e)
        {
            disposeFlg = false;
            this.Navigation.PopAsync();
        }
    }
}