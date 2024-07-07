using DensoScannerSDK.Common;
using DensoScannerSDK.Interface;
using DensoScannerSDK.RFID;
using DensoScannerSDK.Util;
using ReceivingManagementSystem.Common;
using ReceivingManagementSystem.Android.Interfaces;
using Serilog;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ReceivingManagementSystem.Android.Services.Rfid
{
    public class ReadRfidService : IReadRfidService, RFIDDataDelegate
    {
        public event EventHandler<RfidResultEventArgs> OnReadRfid;
        public event EventHandler<RfidResultEventArgs> OnReadPowerLevel;

        /**
        * タグを見つける上での状態
        * Status on finding tags
        */
        private enum LocateTagState
        {
            STANDBY                 // 待機中 waiting 
            , READING_SEARCH_TAG    // 検索用のタグを読み込み中 Loading search tags
            , READING_SEARCH_MULTI_TAG    // 検索用のタグを読み込み中 Loading search multi tags
            , SEARCHING_TAG         // タグを検索中 Searching for tags
        }

        /**
         * Read出力値の段階(1～5段階)
         * Read output stage (1 to 5 steps)
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
        * タグのマッチング方法
        * Tag matching method
        */
        private enum MatchingMethod
        {
            FORWARD
            , BACKWARD
        }


        ICommManagerWrapper m_pCommManagerWrapper = DependencyService.Get<ICommManagerWrapper>();
        ICommonUtilWrapper m_pCommonUtilWrapper = DependencyService.Get<ICommonUtilWrapper>();
        ISaveSettingsWrapper m_pSaveSettingsWrapper = DependencyService.Get<ISaveSettingsWrapper>();
        CommonBase m_hCommonBase = CommonBase.GetInstance();

        IBeepAudioTracks m_pBeepAudioTracks = DependencyService.Get<IBeepAudioTracks>();

        private bool scannerConnectedOnCreate = false;
        private LocateTagState locateTagState = LocateTagState.STANDBY;
        CancellationTokenSource m_pCancellationSource = null;
        private float? readPowerLevelOnSearch = null;

        // 検索用タグUII
        // Search tag UII
        private static TagUII _searchTagUII = null;
        private MatchingMethod matchingMethod = MatchingMethod.FORWARD;
        private ReadPowerStage? readPowerStageOnSearch = null;
        private int? readPowerLevelOnReadSearchTag = null;
        //private bool m_bTransitionToSettingScreen = false;
        Assembly m_pAssembly = null;

        public ReadRfidService()
        {
            m_pAssembly = this.GetType().Assembly;
        }

        public void OnInit()
        {
            string device = DependencyService.Get<ISaveSettingsWrapper>().GetString(ReceivingManagementSystem.Common.Constants.Setting_Device, "SP1");

            if ("SP1".Equals(device))
            {

                scannerConnectedOnCreate = m_hCommonBase.IsCommScanner();

                if (scannerConnectedOnCreate)
                {
                    try
                    {
                        // リスナー登録
                        // Set listener
                        m_hCommonBase.GetCommScanner().GetRFIDScanner().SetDataDelegate(this);
                    }
                    catch (Exception /* ex */)
                    {
                        // データリスナーの登録に失敗
                        // Failed to set listener.
                        m_pCommonUtilWrapper.ShowMessage(AppResources.E_MSG_COMMUNICATION);
                    }
                }
                else
                {
                    // SP1が見つからなかったときはエラーメッセージ表示
                    // Show error message if SP1 is not found.
                    m_pCommonUtilWrapper.ShowMessage(AppResources.E_MSG_NO_CONNECTION);
                }

                // UIをセットアップする
                // Set up UI.
                SetupReadPowerLevelSpinner();

                #region 設定系
                //m_hCommonBase.fStage2_Max_Read_Power_Level = m_pSaveSettingsWrapper.GetFloat
                //                                    (Preferences.pref_stage2_max_read_power_level_on_search, float.Parse(Constants.stage2_max_read_power_level_on_search));
                //m_hCommonBase.fStage3_Max_Read_Power_Level = m_pSaveSettingsWrapper.GetFloat
                //                                    (Preferences.pref_stage3_max_read_power_level_on_search, float.Parse(Constants.stage3_max_read_power_level_on_search));
                //m_hCommonBase.fStage4_Max_Read_Power_Level = m_pSaveSettingsWrapper.GetFloat
                //                                    (Preferences.pref_stage4_max_read_power_level_on_search, float.Parse(Constants.stage4_max_read_power_level_on_search));
                //m_hCommonBase.fStage5_Max_Read_Power_Level = m_pSaveSettingsWrapper.GetFloat
                //                                    (Preferences.pref_stage5_max_read_power_level_on_search, float.Parse(Constants.stage5_max_read_power_level_on_search));

                m_hCommonBase.fStage2_Max_Read_Power_Level = float.Parse(Constants.stage2_max_read_power_level_on_search);
                m_hCommonBase.fStage3_Max_Read_Power_Level = float.Parse(Constants.stage3_max_read_power_level_on_search);
                m_hCommonBase.fStage4_Max_Read_Power_Level = float.Parse(Constants.stage4_max_read_power_level_on_search);
                m_hCommonBase.fStage5_Max_Read_Power_Level = float.Parse(Constants.stage5_max_read_power_level_on_search);
                #endregion
            }
        }

        public void Stop()
        {
            m_pBeepAudioTracks.StopAudioTracks();

            // タグの読み込みや検索を停止してから遷移する
            // Transition after stopping tag reading and search.
            RunLocateTagAction(new LocateTagAction(LocateTagAction.LocateTagActionType.STOP));

            // リスナーを登録解除してから遷移する
            // Transition another screen after removing listener
            if (scannerConnectedOnCreate)
            {
                m_hCommonBase.GetCommScanner().GetRFIDScanner().SetDataDelegate(null);


                //m_hCommonBase.DisconnectCommScanner();
            }

        }

        #region Handle received RFID event
        /**
         * データ受信時の処理
         * Processing when receiving data
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
                else if (locateTagState == LocateTagState.READING_SEARCH_MULTI_TAG)
                {
                    ReadDataOnReadSearchMultiTag(rfidDataReceivedEvent);
                }
                else if (locateTagState == LocateTagState.SEARCHING_TAG)
                {
                    ReadDataOnSearch(rfidDataReceivedEvent);
                }
            });
        }

        /**
         * 検索用のデータを読み込むときのデータ受信処理
         * Data reception processing when retrieval data is read.
         * @param event RFID受信イベント RFID receive event
         */
        private void ReadDataOnReadSearchTag(RFIDDataReceivedEvent IN_pEvent)
        {
            // 複数のタグが読み込まれた場合、最初のタグのUIIを設定する
            // When multiple tags are read, set UII of the first tag.
            List<RFIDData> dataList = new List<RFIDData>(IN_pEvent.RFIDData);
            RFIDData firstData = dataList.ElementAt(0);

            try
            {
                TagUII tag = TagUII.ValueOf(firstData.GetUII());
                string rfid = tag.GetHexString();

                var args = new RfidResultEventArgs(rfid);
                OnReadRfid.Raise(this, args);
            }
            catch (OverflowBitException e)
            {
                System.Diagnostics.Debug.WriteLine(e.StackTrace);
            }

            // 1回読み込んだらすぐに読み込みを終える
            // Finish loading immediately after loading once.
            StopReadSearchTag();
        }

        /// <summary>
        /// 検索用のデータを読み込むときのデータ受信処理
        /// Data reception processing when retrieval data is read.
        /// </summary>
        /// <param name="IN_pEvent"></param>
        private void ReadDataOnReadSearchMultiTag(RFIDDataReceivedEvent IN_pEvent)
        {
            // 検索用タグのUIIに該当するデータを検出する
            // 複数のデータが読み込まれた場合、最後のデータのUIIを設定する
            // Detect data corresponding to UII of search tag.
            // If more than one data is loaded, set the UII of the last data.
            List<RFIDData> dataList = new List<RFIDData>(IN_pEvent.RFIDData);
            RfidResultEventArgs args;
            TagUII tag;
            foreach (var item in dataList)
            {
                tag = TagUII.ValueOf(item.GetUII());

                args = new RfidResultEventArgs(tag.GetHexString());
                OnReadRfid.Raise(this, args);
            }
        }

        /**
         * 検索しているときのデータ受信処理
         * Data reception processing when searching
         * @param event RFID受信イベント RFID receive event
         */
        private void ReadDataOnSearch(RFIDDataReceivedEvent IN_pEvent)
        {
            // 検索用タグのUIIに該当するデータを検出する
            // 複数のデータが読み込まれた場合、最後のデータのUIIを設定する
            // Detect data corresponding to UII of search tag.
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
            // If there is no data corresponding to UII of the search tag, it does nothing
            if (matchedData == null)
            {
                return;
            }

            // RSSIからRead出力値を計算して表示と音に反映する
            // (Read出力値[dBm]) = (RSSI) / 10
            // RSSIはShortに入れ込み0x8000以上の値が負の値となるようにする
            // Calculate Read output value from RSSI and reflect it on display and sound
            // (Read output value[dBm]) = (RSSI) / 10
            // RSSI is inserted into Short so that the value of 0x8000 or more becomes a negative value
            short rssi = (short)matchedData.GetRSSI();
            float readPowerLevel = rssi / 10.0f;

            // 読み込みから1.5秒経ったとき、表示を初期状態にする
            // 1.5秒以内に次の読み込みが行われるとremoveCallbacksAndMessagesが呼び出されキャンセルされる
            // When 1.5 seconds have elapsed since reading, put the display in the initial state
            // If the next reading is done within 1.5 seconds removeCallbacksAndMessages will be called and canceled
            if (m_pCancellationSource != null)
            {
                m_pCancellationSource.Cancel();
            }

            ImageSource imageSource = SetReadPowerLevelOnSearch(readPowerLevel);

            var args = new RfidResultEventArgs(readPowerLevel, imageSource, true);
            OnReadPowerLevel.Raise(this, args);

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
         * Whether the UII hexadecimal character string of the search tag matches that of the search target tag.
         * Even if it does not match as a byte array, it matches if it matches as a character string.
         * 
         * Matching method: In case of matching from the beginning, when matching by byte array, the UII of the search tag is [0 A, 12] and the UII of the search target tag does not match [A 1, 23].
         * However, since it matches with a character string, it coincides with the part of "A12" at the head which matches, so it matches.
         * 
         * @param targetTagUII 検索対象タグのUII Search tag UII
         * @return 検索用タグのUIIの16進数文字列が検索対象タグのものに合致していればtrue、合致していなければfalseを返す True if the UII hexadecimal character string of the search tag matches that of the search target tag, false if it does not match
         */
        private bool CheckMatchSearchTagUIIHexString(byte[] targetTagUII)
        {
            // 検索用タグのUIIがなければ、マッチング自体が不要なので常に該当しているものとする
            // If there is no UII for search tags, matching itself is unnecessary so it always applies
            if (GetSearchTagUII() == null)
            {
                return true;
            }

            // 検索用タグのUIIが検索対象のUIIより長い場合、常に該当しない
            // It is not always applicable when UII of search tag is longer than search UII
            TagUII searchTagUII = GetSearchTagUII();
            if (searchTagUII.GetBytes().Length > targetTagUII.Length)
            {
                return false;
            }

            // 検索文字列が検索対象文字列の先頭もしくは末尾にあれば該当しているものとする
            // If the search character string is at the beginning or the end of the search target character string, it is assumed to be applicable
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

        public void ReadRfid()
        {
            RunLocateTagAction(new LocateTagAction(LocateTagAction.LocateTagActionType.READ_SEARCH_TAG));
        }

        public void ReadMultiRfid()
        {
            RunLocateTagAction(new LocateTagAction(LocateTagAction.LocateTagActionType.READ_SEARCH_MULTI_TAG));
        }

        public void SearchRfid(string rfidSearch)
        {
            SetSearchTagUII(TagUII.ValueOf(rfidSearch));
            LocateTagAction action = locateTagState == LocateTagState.STANDBY ?
            new LocateTagAction(LocateTagAction.LocateTagActionType.SEARCH_TAG) : new LocateTagAction(LocateTagAction.LocateTagActionType.STOP);
            RunLocateTagAction(action);

            //float a = 2;
            //readPowerStageOnSearch = null;
            //if (readPowerLevelOnSearch.HasValue)
            //{
            //    a = readPowerLevelOnSearch.Value - 1;
            //}
            //ImageSource imageSource = SetReadPowerLevelOnSearch(a);

            //var args = new RfidResultEventArgs(2, imageSource, true);
            //OnReadPowerLevel.Raise(this, args);
        }

        public void StopRead()
        {
            RunLocateTagAction(new LocateTagAction(LocateTagAction.LocateTagActionType.STOP));
        }

        /**
        * Read出力値を選択するスピナーをセットアップする
        * Set up the spinner to select Read output value.
        */
        private void SetupReadPowerLevelSpinner()
        {
            // 初期状態は最小値
            // The initial state is the minimum value
            readPowerLevelOnReadSearchTag = int.Parse(((string)(Constants.locate_tag_read_power_levels_item1)));
        }
        /**
       * タグを見つける上でのアクションを実行する
       * Perform actions on finding tags
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
                case LocateTagAction.LocateTagActionType.READ_SEARCH_MULTI_TAG:
                    if (locateTagState == LocateTagState.STANDBY)
                    {
                        StartReadMultiTag();
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
                    if (locateTagState == LocateTagState.READING_SEARCH_MULTI_TAG)
                    {
                        StopReadMultiTag();
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


            // Read出力値に設定してインベントリを開く
            // Set to Read output value and open inventory
            try
            {
                SetScannerSettings((int)readPowerLevelOnReadSearchTag, null);
                OpenScannerInventory();
            }
            catch (Exception e)
            {
                Log.Error(e, e.Message);
                if (e is CommException || e is RFIDException)
                {
                    m_pCommonUtilWrapper.ShowMessage(AppResources.E_MSG_COMMUNICATION);
                    System.Diagnostics.Debug.WriteLine(e.StackTrace);
                }

            }
        }

        /**
       * スキャナのインベントリを開く
       * Open inventory of scanner
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
                Log.Error(e, e.Message);
                if (e is CommException || e is RFIDException)
                {
                    throw e;
                }
            }
        }


        /**
        * スキャナを設定する
        * Set up the scanner
        * @param readPowerLevel Read出力値 Read output value
        * @param sessionFlag nullを指定した場合はセッションフラグを設定しない If null is specified, no session flag is set
        * @throws CommException CommScannerに関する例外 Exception on CommScanner
        * @throws RFIDException RFIDScannerに関する例外 Exception on RFIDScanner
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
                Log.Error(e, e.Message);
                if (e is CommException || e is RFIDException)
                {
                    m_pCommonUtilWrapper.ShowMessage(AppResources.E_MSG_COMMUNICATION);
                    System.Diagnostics.Debug.WriteLine(e.StackTrace);
                }
            }

            locateTagState = LocateTagState.STANDBY;
        }

        /**
       * スキャナのインベントリを閉じる
       * Close the scanner inventory
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
                Log.Error(e, e.Message);
                if (e is CommException || e is RFIDException)
                {
                    throw e;
                }
            }
        }

        /**
        * 検索時におけるRead出力値の表示と音を設定する
        * Display Read output value and set sound during search
        * @param readPowerLevel 検索時におけるRead出力値
        * @ param readPowerLevel Read output value when searching
        */
        private ImageSource SetReadPowerLevelOnSearch(float? readPowerLevel)
        {
            // Read出力値に変更がなければ何もしない
            // Do nothing if there is no change in Read output value
            if (readPowerLevelOnSearch == null && readPowerLevel == null ||
                    readPowerLevelOnSearch != null && readPowerLevel != null && readPowerLevelOnSearch.Equals(readPowerLevel))
            {
                return null;
            }
            readPowerLevelOnSearch = readPowerLevel;

            // 変更後のRead出力値を表示に反映する
            // Reflect the changed Read output value in the display
            string text = readPowerLevel != null ? readPowerLevel.ToString() : "0.0";

            // Read出力値の段階に変更がなければこれ以降何もしない
            // If there is no change in the level of Read output value, do not do anything after this
            ReadPowerStage? readPowerStage = GetReadPowerStage(readPowerLevel);

            if (readPowerStageOnSearch == readPowerStage)
            {
                return null;
            }
            readPowerStageOnSearch = readPowerStage;

            // 変更後のRead出力値の段階を円と音に反映する
            // Reflect the stage of Read output value after change to yen and sound
            ImageSource imageSource = GetImageSearchCircle(readPowerStage);
            SetSearchSound(readPowerStage);

            return imageSource;
        }


        private ImageSource GetImageSearchCircle(ReadPowerStage? readPowerStage)
        {
            // Read出力値の段階に応じて円の表示を設定する
            // Set display of circle according to the level of Read output value
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
                    circleResId = "";
                    break;
            }

            if (!string.IsNullOrEmpty(circleResId))
            {
                return ImageSource.FromResource(circleResId, m_pAssembly);
            }

            return null;
        }

        /**
         * Read出力値の段階に応じて音を設定する
         * Set sound according to the level of Read output value
         * @param readPowerStage Read出力値の段階 nullを指定した場合は再生しない stage of Read output value. Do not play if null is specified.
         */
        private void SetSearchSound(ReadPowerStage? readPowerStage)
        {
            // Read出力値の段階がnullであればオーディオトラックを停止する
            // Stop the audio track if the Read output value stage is null
            if (readPowerStage == null)
            {
                m_pBeepAudioTracks.StopAudioTracks();
                return;
            }

            // 再生するオーディオトラック名を求める
            // Find the name of the audio track to play
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
            // Stop audio tracks that you do not play
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
            // Play the corresponding audio track
            m_pBeepAudioTracks.Play(playAudioTrackName);
        }

        /**
        * Read出力値に対応する段階を返す
        * @param readPowerLevel Read出力値 
        * @return Read出力値に対応する段階 引数にnullを指定した場合はnullを返す
        * 
        * Returns the stage corresponding to the Read output value
        * @param readPowerLevel Read output 
        * @return Step corresponding to Read output value. If null is specified as an argument, it returns null.
        * 
        */
        private ReadPowerStage? GetReadPowerStage(float? readPowerLevel)
        {
            if (readPowerLevel == null)
            {
                return null;
            }

            if (readPowerLevel <= m_hCommonBase.fStage5_Max_Read_Power_Level)
            {
                return ReadPowerStage.STAGE_5;
            }
            else if (readPowerLevel <= m_hCommonBase.fStage4_Max_Read_Power_Level)
            {
                return ReadPowerStage.STAGE_4;
            }
            else if (readPowerLevel <= m_hCommonBase.fStage3_Max_Read_Power_Level)
            {
                return ReadPowerStage.STAGE_3;
            }
            else if (readPowerLevel <= m_hCommonBase.fStage2_Max_Read_Power_Level)
            {
                return ReadPowerStage.STAGE_2;
            }
            else
            {
                return ReadPowerStage.STAGE_1;
            }
        }

        /// <summary>
        ///  タグ検索を開始する
        ///  Start tag search
        /// </summary>
        private void StartReadMultiTag()
        {
            locateTagState = LocateTagState.READING_SEARCH_MULTI_TAG;

            // 規定のRead出力値とセッションS0を設定しインベントリを開く
            // Set prescribed Read output value and session S0 and open inventory
            int searchReadPowerLevel = int.Parse(Constants.read_power_level_on_search_tag);

            //RFIDScannerSettings.Scan.SessionFlag searchSessionFlag = RFIDScannerSettings.Scan.SessionFlag.S0;   // session_on_search = "S0"
            try
            {
                //SetScannerSettings(searchReadPowerLevel, searchSessionFlag);
                OpenScannerInventory();
            }
            catch (Exception e)
            {
                Log.Error(e, e.Message);
                if (e is CommException || e is RFIDException)
                {
                    m_pCommonUtilWrapper.ShowMessage(AppResources.E_MSG_COMMUNICATION);
                    System.Diagnostics.Debug.WriteLine(e.StackTrace);
                }
            }
        }

        /// <summary>
        ///  タグ検索を停止する
        /// Stop tag search
        /// </summary>
        private void StopReadMultiTag()
        {
            // インベントリを閉じる
            // Close inventory
            try
            {
                CloseScannerInventory();
            }
            catch (Exception e)
            {
                Log.Error(e, e.Message);
                if (e is CommException || e is RFIDException)
                {
                    m_pCommonUtilWrapper.ShowMessage(AppResources.E_MSG_COMMUNICATION);
                    System.Diagnostics.Debug.WriteLine(e.StackTrace);
                }
            }

            // Read出力値の表示と音をなくす
            // Read output value display and sound is lost
            SetReadPowerLevelOnSearch(null);

            locateTagState = LocateTagState.STANDBY;
        }

        /**
       * タグ検索を開始する
       * Start tag search
       */
        private void StartSearchTag()
        {
            locateTagState = LocateTagState.SEARCHING_TAG;

            // 規定のRead出力値とセッションS0を設定しインベントリを開く
            // Set prescribed Read output value and session S0 and open inventory
            int searchReadPowerLevel = int.Parse(Constants.read_power_level_on_search_tag);

            RFIDScannerSettings.Scan.SessionFlag searchSessionFlag = RFIDScannerSettings.Scan.SessionFlag.S0;   // session_on_search = "S0"
            try
            {
                SetScannerSettings(searchReadPowerLevel, searchSessionFlag);
                OpenScannerInventory();
            }
            catch (Exception e)
            {
                Log.Error(e, e.Message);
                if (e is CommException || e is RFIDException)
                {
                    m_pCommonUtilWrapper.ShowMessage(AppResources.E_MSG_COMMUNICATION);
                    System.Diagnostics.Debug.WriteLine(e.StackTrace);
                }
            }
        }

        /**
       * タグ検索を停止する
       * Stop tag search
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
                Log.Error(e, e.Message);
                if (e is CommException || e is RFIDException)
                {
                    m_pCommonUtilWrapper.ShowMessage(AppResources.E_MSG_COMMUNICATION);
                    System.Diagnostics.Debug.WriteLine(e.StackTrace);
                }
            }

            // Read出力値の表示と音をなくす
            // Read output value display and sound is lost
            SetReadPowerLevelOnSearch(null);

            locateTagState = LocateTagState.STANDBY;
        }



        #region Set/Get/Load search tag UII with UI

        /**
         * 検索用タグのUIIを設定する
         * 該当するTextView及びButtonに反映する
         * Set UII of search tag
         * Reflected in applicable TextView and Button
         * @param searchTagUII 検索用タグのUII nullを指定した場合空として保持する Search tag UII. If null is specified, it is held as empty
         */
        private void SetSearchTagUII(TagUII searchTagUII)
        {
            _searchTagUII = searchTagUII;
        }

        /**
         * 検索用タグのUIIを取得する
         * Get the UII of the search tag
         * @return 検索用タグのUII 値がない場合はnullを返す Search tag UII. Returns null if there is no value.
         */
        private TagUII GetSearchTagUII()
        {
            return _searchTagUII;
        }

        /**
         * アプリ実行中の間常に保持する検索用タグUIIを読みこむ
         * Read search tag UII which is always held during application execution
         */
        private void LoadSearchTagUII()
        {
            SetSearchTagUII(_searchTagUII);
        }

        #endregion

        #region Tag UII class

        /**
         * タグのUIIを表すクラス
         * Class representing UII of tag
         * タグのUIIには範囲および16進数表記の制約があるため、ラップして表現する
         * Since the UII of the tag has constraints of range and hexadecimal notation, it wraps and expresses it
         */
        private class TagUII
        {

            private static char[] hexCharacters =
                    {'0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F'};

            // 16進数のUII値
            // UIIは256ビット分( (0or1) * 256 )に及ぶため、文字列で指定する
            // Hexadecimal UII value
            // Since UII extends over 256 bits((0or1) * 256), it is specified by a character string
            private string hexString;

            // バイトリストのUII値
            // 最高位バイトからリストに入れていく
            // 例えば文字列が"ABC"の場合、{0xA, 0xBC} になる
            // UII value of byte list
            // Put in the list from the highest byte
            // For example, when the character string is "ABC", it becomes {0 × A, 0 × BC }
            private byte[] bytes;

            /**
             * 16進数の文字列をもとにタグのUIIを返す
             * Returns tag UII based on hexadecimal character string
             * @param hexString 16進数の文字列 Hexadecimal character string
             * @return 指定した16進数の文字列をもとにしたタグのUII UII of the tag based on the specified hexadecimal character string
             * @throws NotCapitalException 指定した文字列の形式に小文字が含まれている場合
             * @throws NotHexException 指定した文字列の形式が16進数でない場合 When the format of the specified character string is not a hexadecimal number
             * @throws OverflowBitException 指定した文字列のビット数が規定のビット数を超えている場合 When the number of bits of the specified character string exceeds the specified number of bits
             */
            public static TagUII ValueOf(String hexString)
            {
                if (hexString.Any(char.IsLower))
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
                    // Up to 256 bits can be input, and 64 digits of hexadecimal correspond to 256 bits
                    throw new OverflowBitException(256);
                }

                return new TagUII(hexString);
            }

            /**
             * バイトリストをもとにタグのUIIを返す
             * Return tag UII based on byte list 
             * @param bytes バイトリスト Byte list
             * @return 指定したバイトリストをもとにしたタグのUII The UII of the tag based on the specified byte list
             * @throws OverflowBitException 指定したバイトリストのビット数が規定のビット数を超えている場合 When the number of bits in the specified byte list exceeds the specified number of bits
             */
            public static TagUII ValueOf(byte[] bytes)
            {
                if (bytes.Length > 32)
                {
                    // 256ビットまで入力可能。1バイトは8ビットの為、32バイトは256ビットにあたる
                    // Up to 256 bits can be input. Since 1 byte is 8 bits, 32 bytes correspond to 256 bits
                    throw new OverflowBitException(256);
                }

                return new TagUII(bytes);
            }

            /**
             * 16進数の文字列から初期化する
             * Initialize from hexadecimal character string
             * @param hexString 16進数の文字列 Hexadecimal character string
             */
            private TagUII(String hexString)
            {
                this.hexString = hexString;

                // バイトのリストを求める
                // Find a list of bytes
                bytes = HexStringToBytes(hexString);
            }

            /**
             * バイトリストから初期化する
             * Initialize from byte list
             * @param bytes バイトリスト byte list
             */
            private TagUII(byte[] bytes)
            {
                this.bytes = bytes;

                // 16進数の文字列を求める
                // Find a hexadecimal character string
                hexString = BytesToHexString(bytes);
            }

            /**
             * バイトのリストとして返す
             * Return as a list of bytes
             * @return バイトのリスト list of bytes
             */
            public byte[] GetBytes()
            {
                return bytes;
            }

            /**
             * 16進数の文字列として返す
             * Return as hexadecimal character string
             * @return 16進数の文字列 Hexadecimal character string
             */
            public String GetHexString()
            {
                return hexString;
            }

            /**
             * 16進数の文字列であるか検証する
             * Verify whether it is a hexadecimal character string
             * @param string 対象となる文字列 Target character string
             * @return 16進数の文字列であればtrue、そうでなければfalseを返す Returns true if it is a hexadecimal character string, false otherwise
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
             * Verify that it is a hexadecimal character
             * @param character 対象となる文字 Characters of interest
             * @return 16進数の文字であればtrue、そうでなければfalseを返す  Returns true if it is a hexadecimal character, false otherwise
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
         * This exception is thrown if the number of bits is overflowing
         */
        private class OverflowBitException : Exception
        {

            /**
             * ビット数から初期化する
             * Initialize from the number of bits
             * @param bitNumber ビット数 Number of bits
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
         * This exception is thrown if the number is not a hexadecimal number
         */
        private class NotHexException : Exception
        {
            /**
             * 初期化する
             * initialize
             */
            public NotHexException() : base("指定できる値は16進数でなければいけません。")
            {

            }
        }

        #endregion

        #region Convert hex string and bytes

        /**
         * 16進数の文字列からバイトのリストに変換する
         * Convert hexadecimal string to byte list
         * @param hexString 16進数の文字列 Hexadecimal character string
         * @return 16進数の文字列をもとにしたバイトのリスト List of bytes based on hexadecimal character string
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
            // Cut out hexadecimal character string in byte units and store it in the list
            // Since 1 byte is equivalent to 2 hexadecimal characters, it cuts out two characters at a time
            // In order to be able to cut out two characters at a time irrespective of the length of the character string, 0 is added to the beginning if the character string is an odd number length
            String workHexString = hexString.Length % 2 == 0 ? hexString : "0" + hexString;
            byte[] bytes = new byte[workHexString.Length / 2];
            for (int i = 0; i < bytes.Length; i++)
            {
                // そのままByte.parseByteすると0x80以上のときにオーバーフローしてしまう
                // 一旦大きい型にパースしてからbyteにキャストすることで、0x80以上の値を負の値として入れ込む
                // Byte.parseByte as it is It overflows when 0x80 or more
                // By parsing to a larger type and then casting it to byte, insert a value of 0x80 or more as a negative value
                string hex2Characters = CommandBuilder.JavaStyleSubstring(workHexString, i * 2, i * 2 + 2);
                short number = short.Parse(hex2Characters, NumberStyles.HexNumber);
                bytes[i] = (byte)number;
            }
            return bytes;
        }

        /**
         * バイトのリストから16進数の文字列に変換する
         * Convert from byte list to hexadecimal character string
         * @param bytes バイトのリスト List of bytes
         * @return バイトのリストをもとにした16進数の文字列 Hexadecimal character string based on the list of bytes
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

        /**
        * タグを見つける上でのアクション
        * Action on finding tags
        */
        private class LocateTagAction
        {
            public enum LocateTagActionType
            {
                READ_SEARCH_TAG      // 検索用のタグを読み込む read search tag
                , READ_SEARCH_MULTI_TAG      // 検索用のタグを読み込む read search multi tag
                , SEARCH_TAG         // タグを検索する search tag
                , STOP               // アクションを停止する stop action
            }

            public LocateTagAction(LocateTagActionType IN_ReadActionType)
            {
                m_ReadActionType = IN_ReadActionType;
            }

            public LocateTagActionType m_ReadActionType { get; set; }

            /**
             * リソース に変換する
             * Convert to resources
             * 
             * @return リソース resources
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
    }
}
