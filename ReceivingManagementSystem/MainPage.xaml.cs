using ReceivingManagementSystem.Wrapper;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DensoScannerSDK.Common;
using DensoScannerSDK.Interface;
using DensoScannerSDK.RFID;
using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.Xaml;
using System.Threading;
using ReceivingManagementSystem.Orders;
using ReceivingManagementSystem.Custodies;
using ReceivingManagementSystem.Warehousing;
using ReceivingManagementSystem.Delivery;
using ReceivingManagementSystem.ReturnReception;
using ReceivingManagementSystem.Return;
using ReceivingManagementSystem.Inventory;
using ReceivingManagementSystem.Views.Settings;
using Serilog;
using ReceivingManagementSystem.ViewModels;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using ReceivingManagementSystem.ViewModels.Inventory;
using ReceivingManagementSystem.Views.PalletRegistration;
using ReceivingManagementSystem.Views.Receipt;
using ReceivingManagementSystem.Views.Shipping;

namespace ReceivingManagementSystem
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainPage : ContentPage, ScannerAcceptStatusListener, INotifyPropertyChanged
    {
        ICommManagerWrapper m_pCommManagerWrapper = DependencyService.Get<ICommManagerWrapper>();
        ICommonUtilWrapper m_pCommonUtilWrapper = DependencyService.Get<ICommonUtilWrapper>();
        ICommonEventWrapper m_pCommonEventWrapper = DependencyService.Get<ICommonEventWrapper>();
        CommonBase m_hCommonBase = CommonBase.GetInstance();
        IBeepAudioTracks m_pBeepAudioTracks = DependencyService.Get<IBeepAudioTracks>();

        // 本体に保存していて、アプリを終了後も保持するフィルタ設定値
        // SetFilterが成功したときのみに書き込みを行い、Loadボタン押下時に保持している値をテキストエリアに表示
        ISaveSettingsWrapper m_pSaveSettingsWrapper = DependencyService.Get<ISaveSettingsWrapper>();

        bool m_bPageTransition = false;

        #region Slave/Master connection switching variables for UWP
        CancellationTokenSource m_pTS = null;
        private DateTime m_dtConnectionStartTime;
        private long m_lConnectionElapsedTime = 0;

        private Semaphore m_pSemaphore_Connection = new Semaphore(1, 1);
        private const int SLAVE_MODE_TIMEOUT = 3000;
        #endregion

        #region Event Listeners
        public void OnScannerAppeared(ICommScanner scanner)
        {
            Debug.WriteLine("OnScannerAppeared()", typeof(MainPage).Name);

            bool successFlag = false;
            try
            {
                scanner.Claim();

                m_pCommManagerWrapper.RemoveAcceptStatusListener(this);
                CallEndAccept(false);

                System.Diagnostics.Debug.WriteLine("OnScannerAppeared", "claim");

                CommonBase.GetInstance().SetConnectedCommScanner(scanner);
                successFlag = true;
            }
            catch (CommException e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
            }

            // 画面に、接続したスキャナのBTLocalNameを表示
            // runOnUIThreadを使用してUI Threadで実行 
            // Display BTLocalName of connected scanner on screen
            // Run with UI Thread using runOnUIThread
            Device.BeginInvokeOnMainThread(() =>
            {
                bool finalSuccessFlag = successFlag;
                ICommScanner pCommScanner = m_hCommonBase.GetCommScanner();
                string btLocalName = pCommScanner.GetBTLocalName();

                if (finalSuccessFlag)
                {
                    m_pCommonUtilWrapper.ShowMessage(AppResources.connected + " " + btLocalName);

                    if (m_hCommonBase.IsCommScanner())
                    {
                        // RFID関連設定値を取得
                        RFIDScannerSettings rfidSettings = m_hCommonBase.GetCommScanner().GetRFIDScanner().GetSettings();
                        // 二度読み防止 SP1設定値送信
                        rfidSettings.scan.doubleReading = RFIDScannerSettings.Scan.DoubleReading.PREVENT1;
                        m_hCommonBase.GetCommScanner().GetRFIDScanner().SetSettings(rfidSettings);
                    }
                }
                else
                {
                    m_pCommonUtilWrapper.ShowMessage(AppResources.connection_error);
                }
            });

            Debug.WriteLine("ScannerAcceptStatusListener#onScannerAppeared", "LOG CONFIRM");
        }

        #endregion


        public MainPage()
        {
            InitializeComponent();

            this.BindingContext = new MainViewModel(this);

            //button_barcode.Text = AppResources.barcode;
            //button_inventory.Text = AppResources.inventory;
            //button_locate_tag.Text = AppResources.locate_tag;
            //button_pre_filters.Text = AppResources.pre_filters;
            //button_rapid_read.Text = AppResources.rapid_read;
            //button_settings.Text = AppResources.settings;

            // アプリバージョンを設定 
            // Set application version
            //tbiVersion.Text = string.Format(AppResources.app_version_format, "1.20");

            m_pCommonUtilWrapper.StartService();

            // セットアップには時間がかかるので、アプリ起動のタイミングでAudioTrackをセットアップする
            // それほどメモリは圧迫しないのでアプリ実行中はAudioTrackを常に保持する
            // Since setup takes time, set up AudioTrack at the timing of application startup
            // Since memory is not subject to much pressure, AudioTrack is always held during application execution
            m_pBeepAudioTracks.SetupAudioTracks();

            // SharedPreferencesを読み込み
            m_pSaveSettingsWrapper.InitSharedPreferences();

        }

        protected override void OnAppearing()
        {
            #region Xamarin.Forms Event
            m_hCommonBase.OnResume += OnResume;
            m_hCommonBase.OnSleep += OnPause;
            #endregion

            if (m_hCommonBase.GetIsAppForeground() == true)
            {
                StartConnection();
            }

            base.OnAppearing();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            #region Xamarin.Forms Event
            m_hCommonBase.OnResume -= OnResume;
            m_hCommonBase.OnSleep -= OnPause;
            #endregion

            if (m_bPageTransition == false)
            {
                // AudioTrackを解放する
                // Free AudioTrack
                m_pBeepAudioTracks.ReleaseAudioTracks();

                if (m_hCommonBase.IsCommScanner())
                {
                    m_hCommonBase.DisconnectCommScanner();
                }

                if (Device.RuntimePlatform == Device.UWP)
                {
                    if (m_pTS != null)
                    {
                        m_pTS.Cancel();
                    }
                }

                // 接続要求を中止
                // Cancel connection request
                CallEndAccept();
            }
        }

        public void StartConnection()
        {
            if (m_hCommonBase.IsCommScanner() == false)
            {
                m_pCommonUtilWrapper.ShowMessage(AppResources.waiting_for_connection);

                //in case of UWP, after Trying to slave mode with timeout 3000 secs, connects to master mode.
                if (Device.RuntimePlatform == Device.UWP)
                {
                    m_pSemaphore_Connection.WaitOne();

                    // 接続されてなかった場合、接続状態を描画
                    Log.Information("UWP Slave Mode Connection Start");

                    m_pTS = new CancellationTokenSource();
                    CancellationToken ct = m_pTS.Token;

                    try
                    {
                        Task.Run(async () =>
                        {
                            m_dtConnectionStartTime = DateTime.Now;
                            while (true)
                            {
                                //Slave Mode接続を回すとき、Thread Cancelをチェック
                                if (ct.IsCancellationRequested)
                                {
                                    Log.Information("Slave Mode Connection End (Cancelled)");
                                    m_pSemaphore_Connection.Release();
                                    return;
                                }

                                //スレーブモードの接続を行う。
                                ICommScanner pFoundScanner = m_pCommManagerWrapper.GetScanners();

                                if (pFoundScanner != null)
                                {
                                    try
                                    {
                                        Log.Information("Slave Mode Claim");
                                        pFoundScanner.Claim();
                                        Log.Information("Slave Mode Success");

                                        //エラーなくClaimに成功したらローブ終了。
                                        m_hCommonBase.SetConnectedCommScanner(pFoundScanner);

                                        ICommScanner pCommScanner = m_hCommonBase.GetCommScanner();
                                        string btLocalName = pCommScanner.GetBTLocalName();

                                        //Slave Mode成功 -> 関数動作終了
                                        m_pSemaphore_Connection.Release();

                                        return;
                                    }
                                    catch (CommException ex)
                                    {
                                        Log.Error(ex, ex.Message);
                                        m_lConnectionElapsedTime = (long)((DateTime.Now - m_dtConnectionStartTime).TotalMilliseconds);

                                        if (m_lConnectionElapsedTime >= SLAVE_MODE_TIMEOUT)
                                        {
                                            //Master Modeに入る
                                            break;
                                        }

                                        //Slave Modeを続ける
                                        await Task.Delay(10);
                                        continue;
                                    }
                                }
                                else
                                {
                                    Log.Information("pFoundScanner is null");
                                    m_lConnectionElapsedTime = (long)((DateTime.Now - m_dtConnectionStartTime).TotalMilliseconds);

                                    if (m_lConnectionElapsedTime >= SLAVE_MODE_TIMEOUT)
                                    {
                                        //Master Modeに入る
                                        break;
                                    }

                                    //Slave Modeを続ける
                                    await Task.Delay(10);
                                    continue;
                                }
                            }

                            Log.Information("Slave Mode Connection Timeout. ");

                            if (ct.IsCancellationRequested)
                            {
                                Log.Information("Connection Thread End. (Cancelled)");
                                m_pSemaphore_Connection.Release();
                                return;
                            }

                            Log.Information("Master Mode Connection Start");
                            m_pCommManagerWrapper.AddAcceptStatusListener(this);
                            Log.Information("AddAcceptStatusListener");
                            Log.Information("StartAccept Start");
                            m_pCommManagerWrapper.StartAccept();
                            Log.Information("StartAccept End");

                            m_pSemaphore_Connection.Release();
                        }, ct);
                    }
                    catch (TaskCanceledException ex)
                    {
                        Log.Error(ex, ex.Message);
                    }
                }
                else
                {
                    m_pCommManagerWrapper.AddAcceptStatusListener(this);
                    m_pCommManagerWrapper.StartAccept();
                }
            }
            else
            {
                ICommScanner pCommScanner = m_hCommonBase.GetCommScanner();

                if (pCommScanner != null)
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        m_pCommonUtilWrapper.ShowMessage(AppResources.connected + " " + pCommScanner.GetBTLocalName());

                        if (m_hCommonBase.IsCommScanner())
                        {
                            // RFID関連設定値を取得
                            RFIDScannerSettings rfidSettings = m_hCommonBase.GetCommScanner().GetRFIDScanner().GetSettings();
                            // 二度読み防止 SP1設定値送信
                            rfidSettings.scan.doubleReading = RFIDScannerSettings.Scan.DoubleReading.PREVENT1;
                            m_hCommonBase.GetCommScanner().GetRFIDScanner().SetSettings(rfidSettings);
                        }
                    });
                }
                else
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        //m_pCommonUtilWrapper.ShowMessage("");
                    });
                }
            }
        }

        #region Xamarin.Forms Events
        private void OnResume(object sender, EventArgs e)
        {
            StartConnection();
        }

        private void OnPause(object sender, EventArgs e)
        {
            if (!m_hCommonBase.IsCommScanner())
            {
                CallEndAccept();
            }
        }

        #endregion

        protected override bool OnBackButtonPressed()
        {
            return true;
        }

        private void Rapid_Read_Tapped(object sender, EventArgs e)
        {
            EnableButtons(false);
            //await NavigatePage(typeof(RapidReadPage));
            EnableButtons(true);
        }

        private async void Inventory_Tapped(object sender, EventArgs e)
        {
            EnableButtons(false);
            await NavigatePage(typeof(InventoryPage));
            EnableButtons(true);
        }

        private void Barcode_Tapped(object sender, EventArgs e)
        {
            EnableButtons(false);
            //await NavigatePage(typeof(BarcodePage));
            EnableButtons(true);
        }

        private async void Settings_Tapped(object sender, EventArgs e)
        {
            EnableButtons(false);
            await NavigatePage(typeof(SettingPage));
            EnableButtons(true);
        }

        private async void Locate_Tag_Tapped(object sender, EventArgs e)
        {
            EnableButtons(false);
            await NavigatePage(typeof(LocateTagPage));
            EnableButtons(true);
        }
        private void Pre_Filters_Tapped(object sender, EventArgs e)
        {
            EnableButtons(false);
            //await NavigatePage(typeof(PreFiltersPage));
            EnableButtons(true);
        }

        void CallEndAccept(bool bUseSemaphore = true)
        {
            if (Device.RuntimePlatform == Device.UWP)
            {
                if (bUseSemaphore == true)
                {
                    m_pSemaphore_Connection.WaitOne();
                }

                // 接続要求を中止
                // Cancel connection request
                System.Diagnostics.Debug.WriteLine("EndAccept Start");
            }

            // 接続要求を中止
            // Cancel connection request
            m_pCommManagerWrapper.EndAccept();

            if (Device.RuntimePlatform == Device.UWP)
            {
                System.Diagnostics.Debug.WriteLine("EndAccept End");
                if (bUseSemaphore == true)
                {
                    m_pSemaphore_Connection.Release();
                }
            }
        }

        async Task NavigatePage(Type IN_pPageType)
        {
            if (Device.RuntimePlatform == Device.UWP)
            {
                if (m_pTS != null)
                {
                    m_pTS.Cancel();
                }

                m_pSemaphore_Connection.WaitOne();
            }

            // 接続要求を中止
            // Cancel connection request
            if (!m_hCommonBase.IsCommScanner())
            {
                m_pCommManagerWrapper.RemoveAcceptStatusListener(this);     //
                System.Diagnostics.Debug.WriteLine("RemoveAcceptStatusListener");
            }

            CallEndAccept(false);

            m_bPageTransition = true;
            await Navigation.PushAsync((Page)Activator.CreateInstance(IN_pPageType), true);

            if (Device.RuntimePlatform == Device.UWP)
            {
                m_pSemaphore_Connection.Release();
            }
        }

        void EnableButtons(bool isEnabled)
        {
            //button_rapid_read.IsEnabled = isEnabled;
            //button_inventory.IsEnabled = isEnabled;
            //button_barcode.IsEnabled = isEnabled;
            //button_settings.IsEnabled = isEnabled;
            //button_locate_tag.IsEnabled = isEnabled;
            //button_pre_filters.IsEnabled = isEnabled;
        }

        private async void tbiSetting_Clicked(object sender, EventArgs e)
        {
            await NavigatePage(typeof(SettingPage));
        }

        private void tbiExist_Clicked(object sender, EventArgs e)
        {
            System.Diagnostics.Process.GetCurrentProcess().Kill();
        }

        private async void btnOrder_Clicked(object sender, EventArgs e)
        {
            await NavigatePage(typeof(OrderReceiptPage));
        }

        private async void btnReceiving_Clicked(object sender, EventArgs e)
        {
            await NavigatePage(typeof(CustodySearchPage));
        }

        private async void btnWarehousing_Clicked(object sender, EventArgs e)
        {
            await NavigatePage(typeof(WarehousingPage));
        }

        private async void btnDelivery_Clicked(object sender, EventArgs e)
        {
            await NavigatePage(typeof(DeliveryInventorySearchPage));
        }

        private async void btnReturnReception_Clicked(object sender, EventArgs e)
        {
            await NavigatePage(typeof(ReturnReceptionSearchPage));
        }

        private async void btnReturn_Clicked(object sender, EventArgs e)
        {
            await NavigatePage(typeof(ReturnPage));
        }

        private async void btnInventory_Clicked(object sender, EventArgs e)
        {
            await NavigatePage(typeof(InventorySearchPage));
        }

        private async void btnSearch_Clicked(object sender, EventArgs e)
        {
            await NavigatePage(typeof(SearchPage));
        }

        private async void tbiAbout_Clicked(object sender, EventArgs e)
        {
            await DisplayActionSheet("バージョン情報", "OK", null, "預り業務アプリケーション", "バージョン：1.0.0.0", "Copyright© 株式会社テスコ");
        }


        private void SfTabView_SelectionChanged(object sender, Syncfusion.XForms.TabView.SelectionChangedEventArgs e)
        {
            MainViewModel viewModel = (MainViewModel)this.BindingContext;

            viewModel.ChangeTabCommand.Execute(e.Name);
        }

        private async void btnPalletRegistration_Clicked(object sender, EventArgs e)
        {
            await NavigatePage(typeof(PalletRegistrationPage));
        }

        private async void btnReceipt_Clicked(object sender, EventArgs e)
        {
            await NavigatePage(typeof(ReceiptPage));
        }

        private async void btnShipping_Clicked(object sender, EventArgs e)
        {
            await NavigatePage(typeof(ShippingPage));
        }

        private async void btnProductInventory_Clicked(object sender, EventArgs e)
        {
            await NavigatePage(typeof(ItemInventorySearchResultPage));
        }

        private async void btnProductSearch_Clicked(object sender, EventArgs e)
        {
            await NavigatePage(typeof(ItemSearchResultPage));
        }
    }
}
