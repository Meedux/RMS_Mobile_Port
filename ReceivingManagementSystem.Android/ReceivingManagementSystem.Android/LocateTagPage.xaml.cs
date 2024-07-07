using DensoScannerSDK.Common;
using DensoScannerSDK.Interface;
using DensoScannerSDK.RFID;
using DensoScannerSDK.Util;
using ReceivingManagementSystem.Android.Interfaces;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ReceivingManagementSystem.Android
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LocateTagPage : ContentPage, RFIDDataDelegate
    {
        #region Property

        // 検索用タグUII
        // Search tag UII
        private static TagUII _searchTagUII = null;

        private int? readPowerLevelOnReadSearchTag = null;

        private float? readPowerLevelOnSearch = null;
        private ReadPowerStage? readPowerStageOnSearch = null;

        private MatchingMethod matchingMethod = MatchingMethod.FORWARD;

        private LocateTagState locateTagState = LocateTagState.STANDBY;

        // この画面が生成された時に、アプリがスキャナと接続されていたかどうか
        // この画面にいる間に接続が切断された場合でも、この画面の生成時にスキャナと接続されていた場合は通信エラーが出るようにする
        // Whether the application was connected to the scanner when this screen was generated.
        // Even if the connection is disconnected while on this screen, make sure that a communication error occurs when connected to the scanner when generating this screen.
        private bool scannerConnectedOnCreate = false;

        private bool disposeFlg = true;

        private bool m_bTransitionToSettingScreen = false;

        #endregion

        ICommManagerWrapper m_pCommManagerWrapper = DependencyService.Get<ICommManagerWrapper>();
        ICommonUtilWrapper m_pCommonUtilWrapper = DependencyService.Get<ICommonUtilWrapper>();
        CommonBase m_hCommonBase = CommonBase.GetInstance();

        IBeepAudioTracks m_pBeepAudioTracks = DependencyService.Get<IBeepAudioTracks>();

        CancellationTokenSource m_pCancellationSource = null;

        // 本体に保存していて、アプリを終了後も保持するフィルタ設定値
        // SetFilterが成功したときのみに書き込みを行い、Loadボタン押下時に保持している値をテキストエリアに表示
        ISaveSettingsWrapper m_pSaveSettingsWrapper = DependencyService.Get<ISaveSettingsWrapper>();

        Assembly m_pAssembly = null;

        public LocateTagPage()
        {
            InitializeComponent();

            m_pAssembly = this.GetType().Assembly;

            #region 文言系 text
            image_search_radar.Source = ImageSource.FromResource("ReceivingManagementSystem.Resource.locate_tag_radar_background.png", m_pAssembly);
            image_search_circle.Source = ImageSource.FromResource("ReceivingManagementSystem.Resource.locate_tag_circle_under_75.png", m_pAssembly);

            text_title_locate_tag.Text = AppResources.locate_tag;

            button_read_search_tag.Text = AppResources.read;
            text_search_tag_uii_head.Text = AppResources.uii;

            picker_match_direction.Items.Add(AppResources.forward_match);
            picker_match_direction.Items.Add(AppResources.backward_match);
            picker_match_direction.SelectedIndex = 0;

            text_read_power_value_on_search.Text = AppResources.number_0_0;
            text_read_power_unit_on_search.Text = AppResources.read_power_level_unit;
            button_search_tag_toggle.Text = AppResources.search;

            button_setting.Text = AppResources.range_setting;

            #endregion

            #region Android限定イベント Android event
            ICommonEventWrapper pEvent = DependencyService.Get<ICommonEventWrapper>();
            pEvent.OnUserLeaveHint += OnUserLeaveHint;
            pEvent.OnRestart += OnRestart;
            #endregion

            #region 設定系
            m_hCommonBase.fStage2_Max_Read_Power_Level = m_pSaveSettingsWrapper.GetFloat
                                                (Preferences.pref_stage2_max_read_power_level_on_search, float.Parse(Constants.stage2_max_read_power_level_on_search));
            m_hCommonBase.fStage3_Max_Read_Power_Level = m_pSaveSettingsWrapper.GetFloat
                                                (Preferences.pref_stage3_max_read_power_level_on_search, float.Parse(Constants.stage3_max_read_power_level_on_search));
            m_hCommonBase.fStage4_Max_Read_Power_Level = m_pSaveSettingsWrapper.GetFloat
                                                (Preferences.pref_stage4_max_read_power_level_on_search, float.Parse(Constants.stage4_max_read_power_level_on_search));
            m_hCommonBase.fStage5_Max_Read_Power_Level = m_pSaveSettingsWrapper.GetFloat
                                                (Preferences.pref_stage5_max_read_power_level_on_search, float.Parse(Constants.stage5_max_read_power_level_on_search));
            #endregion
        }

        #region Activity Relation
        protected override void OnAppearing()
        {
            base.OnAppearing();

            scannerConnectedOnCreate = m_hCommonBase.IsCommScanner();

            if (scannerConnectedOnCreate)
            {
                try
                {
                    // リスナー登録
                    m_hCommonBase.GetCommScanner().GetRFIDScanner().SetDataDelegate(this);
                }
                catch (Exception /* ex */)
                {
                    // データリスナーの登録に失敗
                    m_pCommonUtilWrapper.ShowMessage(AppResources.E_MSG_COMMUNICATION);
                }
            }
            else
            {
                // SP1が見つからなかったときはエラーメッセージ表示
                m_pCommonUtilWrapper.ShowMessage(AppResources.E_MSG_COMMUNICATION);
                SetEnableSearchTag(false);
            }

            if (m_bTransitionToSettingScreen == false)
            {
                // UIをセットアップする
                SetupReadPowerLevelSpinner();

                // 前回保存した検索用タグUIIを反映する
                LoadSearchTagUII();

                if (text_search_tag_uii_value.Text == "" || text_search_tag_uii_value.Text == null)
                {
                    SetSearchTagUII(null);
                    SetEnableSearchTag(false);
                    return;
                }
            }

            // バックグラウンドでサービス起動
            m_pCommonUtilWrapper.StartService();

            if ( m_bTransitionToSettingScreen == true )
            {
                #region Android限定イベントの解除 Remove Android event listener
                ICommonEventWrapper pEvent = DependencyService.Get<ICommonEventWrapper>();
                pEvent.OnUserLeaveHint += OnUserLeaveHint;
                pEvent.OnRestart += OnRestart;
                #endregion
            }

            m_bTransitionToSettingScreen = false;

        }

        protected override void OnDisappearing()
        {
            m_pBeepAudioTracks.StopAudioTracks();

            if (scannerConnectedOnCreate && disposeFlg)
            {
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
        /// Android限定のイベント Android event
        /// </summary>
        private void OnRestart(object sender, EventArgs e)
        {
            disposeFlg = true;
        }

        /// <summary>
        ///  Android限定のイベント Android event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnUserLeaveHint(object sender, EventArgs e)
        {
            // バックグラウンド中はタグの読み込みや検索を停止する
            // Stop loading and searching tags during background.
            RunLocateTagAction(new LocateTagAction(LocateTagAction.LocateTagActionType.STOP));

            if (scannerConnectedOnCreate && locateTagState != LocateTagState.STANDBY)
            {
                disposeFlg = false;
            }
        }

        protected override bool OnBackButtonPressed()
        {
            // タグの読み込みや検索を停止してから遷移する
            RunLocateTagAction(new LocateTagAction(LocateTagAction.LocateTagActionType.STOP));

            // リスナーを登録解除してから遷移する
            if (scannerConnectedOnCreate)
            {
                m_hCommonBase.GetCommScanner().GetRFIDScanner().SetDataDelegate(null);
            }

            disposeFlg = false;
            this.Navigation.PopAsync();

            return true;
        }

        /**
         * 画面遷移でいう上の階層に移動する
         */
        private void NavigateUp()
        {
            // タグの読み込みや検索を停止してから遷移する
            RunLocateTagAction(new LocateTagAction(LocateTagAction.LocateTagActionType.STOP));

            // リスナーを登録解除してから遷移する
            if (scannerConnectedOnCreate)
            {
                m_hCommonBase.GetCommScanner().GetRFIDScanner().SetDataDelegate(null);
            }

            disposeFlg = false;

            // Androidには組み込みのナビゲーション機能"Up Button"があるが、UI上の制約により要件を満たせないためボタンイベントで画面遷移している
            this.Navigation.PopModalAsync();
        }

        #endregion

        #region Handle click event
        private void TapGestureRecognizer_Tapped(object sender, EventArgs e)
        {
            NavigateUp();
        }

        private void button_read_search_tag_Clicked(object sender, EventArgs e)
        {
            RunLocateTagAction(new LocateTagAction(LocateTagAction.LocateTagActionType.READ_SEARCH_TAG));
        }

        private void button_search_tag_toggle_Clicked(object sender, EventArgs e)
        {
            LocateTagAction action = locateTagState == LocateTagState.STANDBY ?
            new LocateTagAction(LocateTagAction.LocateTagActionType.SEARCH_TAG) : new LocateTagAction(LocateTagAction.LocateTagActionType.STOP);
            RunLocateTagAction(action);
        }
        #endregion

        #region Handle received RFID event
        /**
         * データ受信時の処理
         * @param rfidDataReceivedEvent 受信イベント Receive event
         */
        public void OnRFIDDataReceived(ICommScanner scanner, RFIDDataReceivedEvent rfidDataReceivedEvent)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                if (locateTagState == LocateTagState.READING_SEARCH_TAG)
                {
                    ReadDataOnReadSearchTag(rfidDataReceivedEvent);
                }
                else if (locateTagState == LocateTagState.SEARCHING_TAG)
                {
                    ReadDataOnSearch(rfidDataReceivedEvent);
                }
            });
        }

        /**
         * 検索用のデータを読み込むときのデータ受信処理
         * @param event RFID受信イベント RFID receive event
         */
        private void ReadDataOnReadSearchTag(RFIDDataReceivedEvent IN_pEvent)
        {
            // 複数のタグが読み込まれた場合、最初のタグのUIIを設定する
            List<RFIDData> dataList = new List<RFIDData>(IN_pEvent.RFIDData);
            RFIDData firstData = dataList.ElementAt(0);

            try
            {
                SetSearchTagUII(TagUII.ValueOf((firstData.GetUII())));
            }
            catch (OverflowBitException e)
            {
                System.Diagnostics.Debug.WriteLine(e.StackTrace);
            }

            // 1回読み込んだらすぐに読み込みを終える
            StopReadSearchTag();
        }

        /**
         * 検索しているときのデータ受信処理
         * @param event RFID受信イベント RFID receive event
         */
        private void ReadDataOnSearch(RFIDDataReceivedEvent IN_pEvent)
        {
            // 検索用タグのUIIに該当するデータを検出する
            // 複数のデータが読み込まれた場合、最後のデータのUIIを設定する
            // If more than one data is loaded, set the UII of the last data.
            List<RFIDData> dataList = new List<RFIDData>(IN_pEvent.RFIDData);
            RFIDData matchedData = null;
            for (int i = dataList.Count - 1; i >= 0; i--)
            {
                RFIDData data = dataList.ElementAt(i);
                if (CheckMatchSearchTagUIIHexString(data.GetUII()))
                {
                    matchedData = data;
                    break;
                }
            }

            // 検索用タグのUIIに該当するデータがなければ何もしない
            if (matchedData == null)
            {
                return;
            }

            // RSSIからRead出力値を計算して表示と音に反映する
            // (Read出力値[dBm]) = (RSSI) / 10
            // RSSIはShortに入れ込み0x8000以上の値が負の値となるようにする
            short rssi = (short)matchedData.GetRSSI();
            float readPowerLevel = rssi / 10.0f;

            // 読み込みから1.5秒経ったとき、表示を初期状態にする
            // 1.5秒以内に次の読み込みが行われるとremoveCallbacksAndMessagesが呼び出されキャンセルされる
            if (m_pCancellationSource != null)
            {
                m_pCancellationSource.Cancel();
            }

            SetReadPowerLevelOnSearch(readPowerLevel);

            m_pCancellationSource = new CancellationTokenSource();
            Task.Delay(int.Parse(Constants.locate_tag_initialize_time)).ContinueWith(messageHandler =>
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    SetReadPowerLevelOnSearch(null);
                });
            }, m_pCancellationSource.Token);
        }

        /**
         * 検索用タグのUIIの16進数文字列が検索対象タグのものに合致しているか
         * バイト配列として合致していなくても、文字列として合致していれば合致しているものとする
         * 
         * 例: 検索用タグのUII:"A12" 検索対象タグのUII:"A123"の場合
         * マッチング方法:先頭からのマッチング の場合、バイト配列でマッチングすると検索用タグのUIIは[0A, 12]、検索対象タグのUIIは[A1, 23]で合致しない。
         * しかし文字列によるマッチングなので先頭の"A12"の部分が合致していることから合致しているものとする。
         * 
         * @param targetTagUII 検索対象タグのUII Search tag UII
         * @return 検索用タグのUIIの16進数文字列が検索対象タグのものに合致していればtrue、合致していなければfalseを返す True if the UII hexadecimal character string of the search tag matches that of the search target tag, false if it does not match
         */
        private bool CheckMatchSearchTagUIIHexString(byte[] targetTagUII)
        {
            // 検索用タグのUIIがなければ、マッチング自体が不要なので常に該当しているものとする
            if (GetSearchTagUII() == null)
            {
                return true;
            }

            // 検索用タグのUIIが検索対象のUIIより長い場合、常に該当しない
            TagUII searchTagUII = GetSearchTagUII();
            if (searchTagUII.GetBytes().Length > targetTagUII.Length)
            {
                return false;
            }

            // 検索文字列が検索対象文字列の先頭もしくは末尾にあれば該当しているものとする
            String searchString = searchTagUII.GetHexString();
            String targetString = BytesToHexString(targetTagUII);
            switch (matchingMethod)
            {
                case MatchingMethod.FORWARD:
                    int searchStringFirstIndex = targetString.IndexOf(searchString);
                    return searchStringFirstIndex == 0;
                case MatchingMethod.BACKWARD:      // 検索文字列が検索対象文字列の末尾 // Search string at the end of search string
                    int searchStringLastIndex = targetString.LastIndexOf(searchString);
                    return searchStringLastIndex == targetString.Length - searchString.Length;
                default:
                    return false;
            }
        }
        #endregion

        #region Interactive(UI and sound) action

        /**
         * Read出力値を選択するスピナーをセットアップする
         */
        private void SetupReadPowerLevelSpinner()
        {
            spinner_power_level_value_on_read_search_tag.Items.Add(Constants.locate_tag_read_power_levels_item0);
            spinner_power_level_value_on_read_search_tag.Items.Add(Constants.locate_tag_read_power_levels_item1);
            spinner_power_level_value_on_read_search_tag.Items.Add(Constants.locate_tag_read_power_levels_item2);
            spinner_power_level_value_on_read_search_tag.Items.Add(Constants.locate_tag_read_power_levels_item3);

            // 初期状態は最小値
            readPowerLevelOnReadSearchTag = int.Parse(((string)(spinner_power_level_value_on_read_search_tag.Items[0])));
            spinner_power_level_value_on_read_search_tag.SelectedIndex = 0;
        }

        private void spinner_power_level_value_on_read_search_tag_SelectedIndexChanged(object sender, EventArgs e)
        {
            // 選択時にRead出力値を設定
            readPowerLevelOnReadSearchTag = int.Parse(((string)(spinner_power_level_value_on_read_search_tag.SelectedItem)));
        }

        /**
         * UIIを編集する
         */
        private void EditTagUII()
        {
            //未実装 Unimplemented

            //StringInputContents contents = new StringInputContents();
            //contents.title = getString(R.string.uii);
            //contents.startString = getSearchTagUII() != null ? getSearchTagUII().getHexString() : "";
            //contents.inputListener = new StringInputFragment.InputListener() {

            //OnInput
            // ...
            // ...

            //
            // TODO
            //ShowUpperAlphaInput(contents);
        }

        /**
         * タグを見つける上でのアクションを実行する
         * @param action タグを見つける上でのアクション Action on finding tags
         */
        private void RunLocateTagAction(LocateTagAction action)
        {
            switch (action.m_ReadActionType)
            {
                case LocateTagAction.LocateTagActionType.READ_SEARCH_TAG:
                    if (locateTagState == LocateTagState.STANDBY)
                    {
                        StartReadSearchTag();
                    }
                    break;
                case LocateTagAction.LocateTagActionType.SEARCH_TAG:
                    if (locateTagState == LocateTagState.STANDBY)
                    {
                        StartSearchTag();
                    }
                    break;
                case LocateTagAction.LocateTagActionType.STOP:
                    if (locateTagState == LocateTagState.READING_SEARCH_TAG)
                    {
                        StopReadSearchTag();
                    }
                    else if (locateTagState == LocateTagState.SEARCHING_TAG)
                    {
                        StopSearchTag();
                    }
                    break;
            }
        }

        /**
         * 検索用タグの読み込みを開始する
         * Start loading search tags
         */
        private void StartReadSearchTag()
        {
            locateTagState = LocateTagState.READING_SEARCH_TAG;

            // 検索用タグ読み込み中はナビゲーター以外のUI操作を無効にする
            SetEnableInteractiveUIWithoutNavigatorAndSearchTag(false);
            SetEnableSearchTag(false);

            // Read出力値に設定してインベントリを開く
            try
            {
                SetScannerSettings((int)readPowerLevelOnReadSearchTag, null);
                OpenScannerInventory();
            }
            catch (Exception e)
            {
                if (e is CommException || e is RFIDException)
                {
                    m_pCommonUtilWrapper.ShowMessage(AppResources.E_MSG_COMMUNICATION);
                    System.Diagnostics.Debug.WriteLine(e.StackTrace);
                }

            }
        }

        /**
         * 検索用タグの読み込みを停止する
         * Stop loading search tags
         */
        private void StopReadSearchTag()
        {
            // インベントリを閉じる
            // Close inventory
            try
            {
                CloseScannerInventory();
            }
            catch (Exception e)
            {
                if (e is CommException || e is RFIDException)
                {
                    m_pCommonUtilWrapper.ShowMessage(AppResources.E_MSG_COMMUNICATION);
                    System.Diagnostics.Debug.WriteLine(e.StackTrace);
                }
            }

            // 待機中はUI操作を有効にする
            SetEnableInteractiveUIWithoutNavigatorAndSearchTag(true);
            SetEnableSearchTag(true);

            locateTagState = LocateTagState.STANDBY;

        }

        /**
         * タグ検索を開始する
         */
        private void StartSearchTag()
        {
            locateTagState = LocateTagState.SEARCHING_TAG;

            // タグ検索中はナビゲーターと停止ボタン（タグ検索切り替えボタン）以外のUI操作を無効にする
            SetEnableInteractiveUIWithoutNavigatorAndSearchTag(false);

            // タグ検索切り替えボタンのテキストに停止のアクション名を設定する
            button_search_tag_toggle.Text = new LocateTagAction(LocateTagAction.LocateTagActionType.STOP).ToResourceString();

            // 規定のRead出力値とセッションS0を設定しインベントリを開く
            int searchReadPowerLevel = int.Parse(Constants.read_power_level_on_search_tag);

            RFIDScannerSettings.Scan.SessionFlag searchSessionFlag = RFIDScannerSettings.Scan.SessionFlag.S0;   // session_on_search = "S0"
            try
            {
                SetScannerSettings(searchReadPowerLevel, searchSessionFlag);
                OpenScannerInventory();
            }
            catch (Exception e)
            {
                if (e is CommException || e is RFIDException)
                {
                    m_pCommonUtilWrapper.ShowMessage(AppResources.E_MSG_COMMUNICATION);
                    System.Diagnostics.Debug.WriteLine(e.StackTrace);
                }
            }
        }

        /**
         * タグ検索を停止する
         */
        private void StopSearchTag()
        {
            // インベントリを閉じる
            // Close inventory
            try
            {
                CloseScannerInventory();
            }
            catch (Exception e)
            {
                if (e is CommException || e is RFIDException)
                {
                    m_pCommonUtilWrapper.ShowMessage(AppResources.E_MSG_COMMUNICATION);
                    System.Diagnostics.Debug.WriteLine(e.StackTrace);
                }
            }

            // 待機中はUI操作を有効にする
            SetEnableInteractiveUIWithoutNavigatorAndSearchTag(true);

            // タグ検索切り替えボタンのテキストにタグ検索のアクション名を設定する
            button_search_tag_toggle.Text = new LocateTagAction(LocateTagAction.LocateTagActionType.SEARCH_TAG).ToResourceString();

            // Read出力値の表示と音をなくす
            SetReadPowerLevelOnSearch(null);

            locateTagState = LocateTagState.STANDBY;
        }

        /**
         * スキャナのインベントリを開く
         * @throws CommException CommScannerに関する例外 Exception on CommScanner
         * @throws RFIDException RFIDScannerに関する例外 Exception on RFIDScanner
         */
        private void OpenScannerInventory()
        {
            if (!scannerConnectedOnCreate)
            {
                return;
            }

            try
            {
                m_hCommonBase.GetCommScanner().GetRFIDScanner().OpenInventory();
            }
            catch (Exception e)
            {
                if (e is CommException || e is RFIDException)
                {
                    throw e;
                }
            }
        }

        /**
         * スキャナのインベントリを閉じる
         * @throws CommException CommScannerに関する例外 Exception on CommScanner
         * @throws RFIDException RFIDScannerに関する例外 Exception on RFIDScanner
         */
        private void CloseScannerInventory()
        {
            if (!scannerConnectedOnCreate)
            {
                return;
            }

            try
            {
                m_hCommonBase.GetCommScanner().GetRFIDScanner().Close();
            }
            catch (Exception e)
            {
                if (e is CommException || e is RFIDException)
                {
                    throw e;
                }
            }
        }

        /**
         * スキャナを設定する
         * Set up the scanner
         * @param readPowerLevel Read出力値
         * @param sessionFlag nullを指定した場合はセッションフラグを設定しない
         * @throws CommException CommScannerに関する例外
         * @throws RFIDException RFIDScannerに関する例外
         */
        private void SetScannerSettings(int readPowerLevel, RFIDScannerSettings.Scan.SessionFlag? sessionFlag)
        {
            if (!scannerConnectedOnCreate)
            {
                return;
            }

            // 設定する値以外の設定値を既存のものから取得する
            // Acquire setting values ​​other than the value to be set from the existing one
            RFIDScannerSettings settings = m_hCommonBase.GetCommScanner().GetRFIDScanner().GetSettings();

            // 値を設定する
            // Set value
            settings.scan.powerLevelRead = readPowerLevel;
            if (sessionFlag != null)
            {
                settings.scan.sessionFlag = (RFIDScannerSettings.Scan.SessionFlag)sessionFlag;
            }

            m_hCommonBase.GetCommScanner().GetRFIDScanner().SetSettings(settings);
        }

        /**
         * ナビゲーターとタグ検索切り替えボタン以外のUI操作に関して有効/無効を設定する
         * Enable / disable UI operations other than navigator and tag search switching button
         * @param enabled 有効ならtrue、無効ならfalseを指定する Specify true if it is enabled, false if it is invalid
         */
        private void SetEnableInteractiveUIWithoutNavigatorAndSearchTag(bool enabled)
        {
            // 該当するUIを 有効化/無効化 する
            // 基本の色がデフォルトのテキストを 有効/無効 な見た目になるよう表示する            

            spinner_power_level_value_on_read_search_tag.IsEnabled = enabled;
            button_read_search_tag.IsEnabled = enabled;
            text_search_tag_uii_value.IsEnabled = enabled;
            picker_match_direction.IsEnabled = enabled;
            button_setting.IsEnabled = enabled;

            // 基本の色が強調色のテキストを 有効/無効 な見た目になるよう表示する
            text_search_tag_uii_head.IsEnabled = enabled;

        }

        /**
         * タグ検索切り替えボタンの有効/無効を設定する
         * @param enabled 有効ならtrue、無効ならfalseを指定する
         */
        private void SetEnableSearchTag(bool enabled)
        {
            button_search_tag_toggle.IsEnabled = enabled;
        }

        /**
         * 検索時におけるRead出力値の表示と音を設定する
         * @param readPowerLevel 検索時におけるRead出力値
         */
        private void SetReadPowerLevelOnSearch(float? readPowerLevel)
        {
            // Read出力値に変更がなければ何もしない
            if (readPowerLevelOnSearch == null && readPowerLevel == null ||
                    readPowerLevelOnSearch != null && readPowerLevel != null && readPowerLevelOnSearch.Equals(readPowerLevel))
            {
                return;
            }
            readPowerLevelOnSearch = readPowerLevel;

            // 変更後のRead出力値を表示に反映する
            string text = readPowerLevel != null ? readPowerLevel.ToString() : "0.0";
            text_read_power_value_on_search.Text = text;

            // Read出力値の段階に変更がなければこれ以降何もしない
            ReadPowerStage? readPowerStage = GetReadPowerStage(readPowerLevel);

            if (readPowerStageOnSearch == readPowerStage)
            {
                return;
            }
            readPowerStageOnSearch = readPowerStage;

            // 変更後のRead出力値の段階を円と音に反映する
            SetSearchCircle(readPowerStage);
            SetSearchSound(readPowerStage);
        }

        /**
         * Read出力値の段階に応じて円の表示を設定する
         * @param readPowerStage 円の表示のもとになるRead出力値の段階 nullを指定した場合は表示しない
         */
        private void SetSearchCircle(ReadPowerStage? readPowerStage)
        {
            // Read出力値の段階がnullであれば表示しない
            if (readPowerStage != null)
            {
                image_search_circle.IsVisible = true;
            }
            else
            {
                image_search_circle.IsVisible = false;
                return;
            }

            // Read出力値の段階に応じて円の表示を設定する
            string circleResId = "";
            switch (readPowerStage)
            {
                case ReadPowerStage.STAGE_1:
                    circleResId = "ReceivingManagementSystem.Resource.locate_tag_circle_over_35.png";
                    break;
                case ReadPowerStage.STAGE_2:
                    circleResId = "ReceivingManagementSystem.Resource.locate_tag_circle_48_to_36.png";
                    break;
                case ReadPowerStage.STAGE_3:
                    circleResId = "ReceivingManagementSystem.Resource.locate_tag_circle_62_to_49.png";
                    break;
                case ReadPowerStage.STAGE_4:
                    circleResId = "ReceivingManagementSystem.Resource.locate_tag_circle_74_to_63.png";
                    break;
                case ReadPowerStage.STAGE_5:
                    circleResId = "ReceivingManagementSystem.Resource.locate_tag_circle_under_75.png";
                    break;
                default:
                    return;
            }

            image_search_circle.Source = ImageSource.FromResource(circleResId, m_pAssembly);
        }

        /**
         * Read出力値の段階に応じて音を設定する
         * @param readPowerStage Read出力値の段階 nullを指定した場合は再生しない
         */
        private void SetSearchSound(ReadPowerStage? readPowerStage)
        {
            // Read出力値の段階がnullであればオーディオトラックを停止する
            if (readPowerStage == null)
            {
                m_pBeepAudioTracks.StopAudioTracks();
                return;
            }

            // 再生するオーディオトラック名を求める
            AudioTrackName playAudioTrackName;
            switch (readPowerStage)
            {
                case ReadPowerStage.STAGE_1:
                    playAudioTrackName = AudioTrackName.TRACK_1;
                    break;
                case ReadPowerStage.STAGE_2:
                    playAudioTrackName = AudioTrackName.TRACK_2;
                    break;
                case ReadPowerStage.STAGE_3:
                    playAudioTrackName = AudioTrackName.TRACK_3;
                    break;
                case ReadPowerStage.STAGE_4:
                    playAudioTrackName = AudioTrackName.TRACK_4;
                    break;
                case ReadPowerStage.STAGE_5:
                    playAudioTrackName = AudioTrackName.TRACK_5;
                    break;
                default:
                    return;
            }

            // 再生しないオーディオトラックは停止する
            if (playAudioTrackName != AudioTrackName.TRACK_1)
            {
                m_pBeepAudioTracks.Stop(AudioTrackName.TRACK_1);
            }

            if (playAudioTrackName != AudioTrackName.TRACK_2)
            {
                m_pBeepAudioTracks.Stop(AudioTrackName.TRACK_2);
            }

            if (playAudioTrackName != AudioTrackName.TRACK_3)
            {
                m_pBeepAudioTracks.Stop(AudioTrackName.TRACK_3);
            }

            if (playAudioTrackName != AudioTrackName.TRACK_4)
            {
                m_pBeepAudioTracks.Stop(AudioTrackName.TRACK_4);
            }

            if (playAudioTrackName != AudioTrackName.TRACK_5)
            {
                m_pBeepAudioTracks.Stop(AudioTrackName.TRACK_5);
            }

            // 該当するオーディオトラックを再生する
            m_pBeepAudioTracks.Play(playAudioTrackName);
        }

        #endregion

        #region Set/Get/Load search tag UII with UI

        /**
         * 検索用タグのUIIを設定する
         * 該当するTextView及びButtonに反映する
         * @param searchTagUII 検索用タグのUII nullを指定した場合空として保持する
         */
        private void SetSearchTagUII(TagUII searchTagUII)
        {
            _searchTagUII = searchTagUII;
            text_search_tag_uii_value.Text = searchTagUII != null ? searchTagUII.GetHexString() : null;
        }

        /**
         * 検索用タグのUIIを取得する
         * @return 検索用タグのUII 値がない場合はnullを返す
         */
        private TagUII GetSearchTagUII()
        {
            return _searchTagUII;
        }

        /**
         * アプリ実行中の間常に保持する検索用タグUIIを読みこむ
         */
        private void LoadSearchTagUII()
        {
            SetSearchTagUII(_searchTagUII);
        }

        #endregion

        #region Convert hex string and bytes

        /**
         * 16進数の文字列からバイトのリストに変換する
         * @param hexString 16進数の文字列
         * @return 16進数の文字列をもとにしたバイトのリスト
         */
        private static byte[] HexStringToBytes(String hexString)
        {
            // 空文字の場合は要素 0
            // In case of null character, element 0
            if (hexString.Length == 0)
            {
                return new byte[0];
            }

            // 16進数の文字列をバイト単位で切り出してリストに格納する
            // 1バイトは16進数2文字分なので2文字ずつ切り出す
            // 文字列長にかかわらず2文字ずつ切り出せるようにするため、文字列が奇数長の場合は0を先頭に追加する
            String workHexString = hexString.Length % 2 == 0 ? hexString : "0" + hexString;
            byte[] bytes = new byte[workHexString.Length / 2];
            for (int i = 0; i < bytes.Length; i++)
            {
                // そのままByte.parseByteすると0x80以上のときにオーバーフローしてしまう
                // 一旦大きい型にパースしてからbyteにキャストすることで、0x80以上の値を負の値として入れ込む
                string hex2Characters = CommandBuilder.JavaStyleSubstring(workHexString, i * 2, i * 2 + 2);
                short number = short.Parse(hex2Characters, NumberStyles.HexNumber);
                bytes[i] = (byte)number;
            }
            return bytes;
        }

        /**
         * バイトのリストから16進数の文字列に変換する
         * @param bytes バイトのリスト
         * @return バイトのリストをもとにした16進数の文字列
         */
        private static string BytesToHexString(byte[] bytes)
        {
            StringBuilder hexStringBuilder = new StringBuilder();
            foreach (byte byteNumber in bytes)
            {
                string hex2Characters = byteNumber.ToString("X2");
                hexStringBuilder.Append(hex2Characters);
            }

            return hexStringBuilder.ToString();
        }

        #endregion

        #region Dialog relation

        /**
         * 大文字の英語と数字のみ入力できるStringInputダイアログを表示する
         * @param contents ダイアログに表示する内容
         */
        //private void showUpperAlphaInput(StringInputContents contents)
        //{
        //    StringInputFragment fragment = new StringInputFragment();
        //    fragment.context = this;
        //    fragment.inputType = StringInputFragment.InputType.UPPER_ALPHA_NUMERIC;
        //    fragment.title = contents.title;
        //    fragment.startString = contents.startString;
        //    fragment.listener = contents.inputListener;
        //    fragment.show(getFragmentManager(), getString(R.string.fragment_anonymous));
        //}

        /**
         * StringInputダイアログを生成するための内容
         */
        //private class StringInputContents
        //{
        //    string title;
        //    string startString;
        //    StringInputFragment.InputListener inputListener;
        //}

        #endregion

        #region Tag UII class

        /**
         * タグのUIIを表すクラス
         * タグのUIIには範囲および16進数表記の制約があるため、ラップして表現する
         */
        private class TagUII
        {

            private static char[] hexCharacters =
                    {'0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F'};

            // 16進数のUII値
            // UIIは256ビット分( (0or1) * 256 )に及ぶため、文字列で指定する
            private string hexString;

            // バイトリストのUII値
            // 最高位バイトからリストに入れていく
            // 例えば文字列が"ABC"の場合、{0xA, 0xBC} になる
            private byte[] bytes;

            /**
             * 16進数の文字列をもとにタグのUIIを返す
             * @param hexString 16進数の文字列
             * @return 指定した16進数の文字列をもとにしたタグのUII
             * @throws NotCapitalException 指定した文字列の形式に小文字が含まれている場合
             * @throws NotHexException 指定した文字列の形式が16進数でない場合
             * @throws OverflowBitException 指定した文字列のビット数が規定のビット数を超えている場合
             */
            public static TagUII ValueOf(String hexString)
            {
                if ( hexString.Any ( char.IsLower ) )
                {
                    throw new NotCapitalException();    
                }
                if (!CheckHexString(hexString))
                {
                    throw new NotHexException();
                }
                if (hexString.Length > 64)
                {
                    // 256ビットまで入力可能、16進数の64桁分は256ビット分にあたる 
                    throw new OverflowBitException(256);
                }

                return new TagUII(hexString);
            }

            /**
             * バイトリストをもとにタグのUIIを返す
             * @param bytes バイトリスト
             * @return 指定したバイトリストをもとにしたタグのUII
             * @throws OverflowBitException 指定したバイトリストのビット数が規定のビット数を超えている場合
             */
            public static TagUII ValueOf(byte[] bytes)
            {
                if (bytes.Length > 32)
                {
                    // 256ビットまで入力可能。1バイトは8ビットの為、32バイトは256ビットにあたる
                    throw new OverflowBitException(256);
                }

                return new TagUII(bytes);
            }

            /**
             * 16進数の文字列から初期化する
             * @param hexString 16進数の文字列
             */
            private TagUII(String hexString)
            {
                this.hexString = hexString;

                // バイトのリストを求める
                bytes = HexStringToBytes(hexString);
            }

            /**
             * バイトリストから初期化する
             * @param bytes バイトリスト
             */
            private TagUII(byte[] bytes)
            {
                this.bytes = bytes;

                // 16進数の文字列を求める
                hexString = BytesToHexString(bytes);
            }

            /**
             * バイトのリストとして返す
             * @return バイトのリスト
             */
            public byte[] GetBytes()
            {
                return bytes;
            }

            /**
             * 16進数の文字列として返す
             * @return 16進数の文字列
             */
            public String GetHexString()
            {
                return hexString;
            }

            /**
             * 16進数の文字列であるか検証する
             * @param string 対象となる文字列
             * @return 16進数の文字列であればtrue、そうでなければfalseを返す
             */
            private static bool CheckHexString(string IN_string)
            {
                for (int i = 0; i < IN_string.Length; i++)
                {
                    char character = IN_string.ElementAt(i);

                    if (!CheckHexCharacter(character))
                    {
                        return false;
                    }
                }

                return true;
            }

            /**
             * 16進数の文字であるか検証する
             * @param character 対象となる文字
             * @return 16進数の文字であればtrue、そうでなければfalseを返す
             */
            private static bool CheckHexCharacter(char character)
            {
                foreach (char hexCharacter in hexCharacters)
                {
                    if (character == hexCharacter)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        /**
         * ビット数がオーバーフローしている場合にこの例外がスローされる
         */
        private class OverflowBitException : Exception
        {

            /**
             * ビット数から初期化する
             * @param bitNumber ビット数
             */
            public OverflowBitException(int bitNumber) : base(string.Format(CultureInfo.CurrentCulture, "指定できる値は %d bitまでです。", bitNumber))
            {

            }
        }

        private class NotCapitalException : Exception
        {
            /**
             * 初期化する
             */
            public NotCapitalException() : base("指定できる値は大文字でなければいけません。")
            {

            }
        }

        /**
         * 数値が16進数でない場合にこの例外がスローされる
         */
        private class NotHexException : Exception
        {
            /**
             * 初期化する
             */
            public NotHexException() : base("指定できる値は16進数でなければいけません。")
            {

            }
        }

        #endregion

        #region Enums

        /**
         * Read出力値の段階(1～5段階)
         */
        private enum ReadPowerStage
        {
            STAGE_1
            , STAGE_2
            , STAGE_3
            , STAGE_4
            , STAGE_5
        }

        /**
         * Read出力値に対応する段階を返す
         * @param readPowerLevel Read出力値 
         * @return Read出力値に対応する段階 引数にnullを指定した場合はnullを返す
         */
        private ReadPowerStage? GetReadPowerStage(float? readPowerLevel)
        {
            if (readPowerLevel == null)
            {
                return null;
            }

            if (readPowerLevel <= m_hCommonBase.fStage5_Max_Read_Power_Level )
            {
                return ReadPowerStage.STAGE_5;
            }
            else if (readPowerLevel <= m_hCommonBase.fStage4_Max_Read_Power_Level )
            {
                return ReadPowerStage.STAGE_4;
            }
            else if (readPowerLevel <= m_hCommonBase.fStage3_Max_Read_Power_Level)
            {
                return ReadPowerStage.STAGE_3;
            }
            else if (readPowerLevel <= m_hCommonBase.fStage2_Max_Read_Power_Level )
            {
                return ReadPowerStage.STAGE_2;
            }
            else
            {
                return ReadPowerStage.STAGE_1;
            }
        }

        /**
         * タグのマッチング方法
         */
        private enum MatchingMethod
        {
            FORWARD
            , BACKWARD
        }

        /**
         * タグを見つける上での状態
         */
        private enum LocateTagState
        {
            STANDBY                 // 待機中
            , READING_SEARCH_TAG    // 検索用のタグを読み込み中
            , SEARCHING_TAG         // タグを検索中
        }

        /**
         * タグを見つける上でのアクション
         */
        private class LocateTagAction
        {
            public enum LocateTagActionType
            {
                READ_SEARCH_TAG      // 検索用のタグを読み込む
                , SEARCH_TAG         // タグを検索する
                , STOP               // アクションを停止する
            }

            public LocateTagAction(LocateTagActionType IN_ReadActionType)
            {
                m_ReadActionType = IN_ReadActionType;
            }

            public LocateTagActionType m_ReadActionType { get; set; }

            /**
             * リソース に変換する
             * 
             * @return リソース
             */
            public string ToResourceString()
            {
                switch (m_ReadActionType)
                {
                    case LocateTagActionType.READ_SEARCH_TAG:
                        return AppResources.read;
                    case LocateTagActionType.SEARCH_TAG:
                        return AppResources.search;
                    case LocateTagActionType.STOP:
                        return AppResources.stop;
                    default:
                        throw new ArgumentException();
                }
            }
        }
        #endregion

        private void text_search_tag_uii_value_Completed(object sender, EventArgs e)
        {
            string strInput = text_search_tag_uii_value.Text;

            // 空文字列の場合はnullを設定する
            if (strInput == "" || strInput == null)
            {
                SetSearchTagUII(null);
                SetEnableSearchTag(false);
                return;
            }

            // 数値を検証する
            TagUII uii;
            try
            {
                uii = TagUII.ValueOf(strInput);
            }
            catch (NotCapitalException)
            {
                m_pCommonUtilWrapper.ShowMessage(AppResources.E_MSG_FILTER_NOT_CAPITAL);
                SetEnableSearchTag(false);
                return;
            }
            catch (NotHexException)
            {
                m_pCommonUtilWrapper.ShowMessage(AppResources.E_MSG_FILTER_INVALID_PATTERN);
                SetEnableSearchTag(false);
                return;
            }
            catch (OverflowBitException)
            {
                m_pCommonUtilWrapper.ShowMessage(AppResources.E_MSG_FILTER_OUT_OF_RANGE_PATTERN);
                SetEnableSearchTag(false);
                return;
            }

            // 数値が正常値なので設定する
            SetSearchTagUII(uii);

            if (scannerConnectedOnCreate)
            {
                SetEnableSearchTag(true);
            }
        }

        private void button_setting_Clicked(object sender, EventArgs e)
        {
            // リスナーを登録解除してから遷移する
            if (scannerConnectedOnCreate)
            {
                m_hCommonBase.GetCommScanner().GetRFIDScanner().SetDataDelegate(null);
            }

            disposeFlg = false;
            m_bTransitionToSettingScreen = true;

            Navigation.PushModalAsync(new LocateTagSettingPage(), true);
        }

        private void Picker_match_direction_SelectedIndexChanged(object sender, EventArgs e)
        {
            Picker pPicker = ((Picker)sender);

            string strSelectedItem = (string)(pPicker.SelectedItem);

            if ( strSelectedItem == AppResources.forward_match)
            {
                matchingMethod = MatchingMethod.FORWARD;
            }
            else if ( strSelectedItem == AppResources.backward_match)
            {
                matchingMethod = MatchingMethod.BACKWARD;
            }
        }
    }
}