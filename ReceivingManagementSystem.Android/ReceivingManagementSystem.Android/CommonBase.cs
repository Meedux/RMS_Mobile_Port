using ReceivingManagementSystem.Android.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using DensoScannerSDK;
using DensoScannerSDK.Common;
using DensoScannerSDK.Interface;
using DensoScannerSDKDroid.RFID;
using Xamarin.Forms;
using JP.CO.Toshibatec;
using JP.CO.Toshibatec.Callback;
using JP.CO.Toshibatec.Model;

namespace ReceivingManagementSystem.Android
{
    public class CommonBase : ScannerStatusListener
    {
        static CommonBase m_hCommonBase = null;

        //Denso SDK Scanner
        public static ICommScanner commScanner;
        public static bool scannerConnected = false;

        //Toshiba Tec's Scanner

        ICommonUtilWrapper m_pCommonUtilWrapper = DependencyService.Get<ICommonUtilWrapper>();
        ICommonEventWrapper m_pCommonEventWrapper = DependencyService.Get<ICommonEventWrapper>();

        public event EventHandler OnResume;     //Android OnResume 相当
        public event EventHandler OnSleep;        //Android OnSleep 相当

        public static CommonBase GetInstance()
        {
            if (m_hCommonBase == null)
            {
                m_hCommonBase = new CommonBase();
            }
            return m_hCommonBase;
        }

        /**
         * 接続済みのCommScannerを設定
         * Set up connected CommScanner
         * @param connectedCommScanner 接続済みのCommScanner nullの場合、保持しているCommScannerをnullにする if argument is null, make the holding CommScanner null.
         */
        public void SetConnectedCommScanner(ICommScanner IN_pCommScanner)
        {
            if (IN_pCommScanner != null)
            {
                scannerConnected = true;
                IN_pCommScanner.AddStatusListener(this);
            }
            else
            {
                scannerConnected = false;
                if (commScanner != null)
                {
                    IN_pCommScanner.RemoveStatusListener(this);
                }
            }

            commScanner = IN_pCommScanner;
        }

        /**
         * CommScanner取得
         * 返されるCommScannerがnullでなくてもCommScannerが接続している状態とは限らないので
         * スキャナが接続されていることを確認するならば,isCommScannerを使用すること
         * Get CommScanner
         * Even if the returned CommScanner is not null,CommScanner is not always connected.
         * If you want to confirm that the scanner is connected, use isCommScanner.
         * @return
         */
        public ICommScanner GetCommScanner()
        {
            return commScanner;
        }

        /**
         * CommScannerが接続されているか
         * Whether the CommScanner is connected or not
         * @return CommScannerが接続されていたらtrue、切断されていたらfalseを返す true:connected false:not connected
         */
        public bool IsCommScanner()
        {
            return scannerConnected;
        }

        /**
         * SP1切断
         * disconnect SP1 
         */
        public void DisconnectCommScanner()
        {
            if (commScanner != null)
            {
                try
                {
                    commScanner.Close();
                    commScanner.RemoveStatusListener(this);
                    scannerConnected = false;
                    commScanner = null;
                }
                catch (CommException e)
                {
                    ICommonUtilWrapper pUtil = DependencyService.Get<ICommonUtilWrapper>();
                    pUtil.ShowMessage(e.Message);
                }
            }
        }

        /**
         * スキャナの接続状態が変更されたときのイベント処理
         * Event handling when the connection status of the scanner is changed
         * @param scanner スキャナ scanner
         * @param state 状態 status
         */
        public void OnScannerStatusChanged(ICommScanner scanner, CommStatusChangedEvent state)
        {
            CommConst.ScannerStatus scannerStatus = state.GetStatus();
            if (scanner == commScanner && scannerStatus.Equals(CommConst.ScannerStatus.CLOSE_WAIT))
            {
                // 切断検知時にTOP画面以外のActivityを終了する
                // finish all activity except top  if a disconnect is detencted.
                Device.BeginInvokeOnMainThread(async () =>
                {
                    if (scannerConnected)
                    {
                        // 未接続メッセージ表示
                        // show no connect message
                        m_pCommonUtilWrapper.ShowMessage(AppResources.E_MSG_NO_CONNECTION);

                        scannerConnected = false;

                        // TOP画面にもどる
                        // return to top screen
                        var pArrList = App.Current.MainPage.Navigation.ModalStack;

                        if ( pArrList.Count == 0 )
                        {
                            ((MainPage)App.Current.MainPage).StartConnection();
                        }
                        else
                        {
                            while (true)
                            {
                                pArrList = App.Current.MainPage.Navigation.ModalStack;

                                if (pArrList.Count == 0)
                                {
                                    break;
                                }

                                await App.Current.MainPage.Navigation.PopAsync();
                            }
                        }
                    }
                });
            }
        }

        bool m_bIsForeground = true;
        object mObj_isForeground = new object();

        public bool GetIsAppForeground()
        {
            bool result;
            lock (mObj_isForeground)
            {
                result = m_bIsForeground;
            }

            return result;
        }

        public void SetIsForeground(bool in_isForeground)
        {
            lock (mObj_isForeground)
            {
                m_bIsForeground = in_isForeground;
            }
        }

        #region Xamarin.Forms Lifecyles
        public void RaiseOnResume()
        {
            if (OnResume != null)
            {
                OnResume(this, new EventArgs());
            }
        }

        public void RaiseOnSleep()
        {
            if (OnSleep != null)
            {
                OnSleep(this, new EventArgs());
            }
        }
        #endregion

        #region PreFilter Temp Values
        // アプリ実行中の間のみ保持するフィルタ
        // フィルタを設定するときにこのアクティビティが持つフィルタと変わりなければフィルタの保存を行わない
        // フィルタの設定が成功した時に値を保存し、画面遷移後に表示する値を保持
        // this filter is used while this app is executing.
        // When setting the filter, do not save the filter unless it is the same as the filter of this activity
        // Save the value when the filter setting is successful
        //public PreFiltersPage.FilterBank tempFilterBank = null;
        //public PreFiltersPage.FilterOffset tempFilterOffset = null;
        //public PreFiltersPage.FilterPattern tempFilterPattern = null;
        #endregion

        #region LocationTag Setting Values
        //LocationTag 画面での設定
        public float fStage2_Max_Read_Power_Level = 0.0f;
        public float fStage3_Max_Read_Power_Level = 0.0f;
        public float fStage4_Max_Read_Power_Level = 0.0f;
        public float fStage5_Max_Read_Power_Level = 0.0f;
        #endregion

    }
}
