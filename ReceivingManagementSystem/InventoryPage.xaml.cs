using ReceivingManagementSystem.Wrapper;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DensoScannerSDK.Interface;
using DensoScannerSDK.RFID;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ReceivingManagementSystem
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class InventoryPage : ContentPage, RFIDDataDelegate
    {
        ICommManagerWrapper m_pCommManagerWrapper = DependencyService.Get<ICommManagerWrapper>();
        ICommonUtilWrapper m_pCommonUtilWrapper = DependencyService.Get<ICommonUtilWrapper>();
        CommonBase m_hCommonBase = CommonBase.GetInstance();

        private ReadAction nextReadAction = new ReadAction(ReadAction.ReadActionType.START);
        private bool disposeFlg = true;

        // この画面が生成された時に、アプリがスキャナと接続されていたかどうか
        // この画面にいる間に接続が切断された場合でも、この画面の生成時にスキャナと接続されていた場合は通信エラーが出るようにする
        // Whether the application was connected to the scanner when this screen was generated.
        // Even if the connection is disconnected while on this screen, make sure that a communication error occurs when connected to the scanner when generating this screen.
        private bool scannerConnectedOnCreate = false;

        private ObservableCollection<TagData> m_TagDataSet = new ObservableCollection<TagData>();

        #region Recycler
        // ダミーデータの利用数
        // The number of usage of dummy data
        private readonly int DEFAULT_TAG_LINE_NUMBER = 15;

        private int storedTagCount = 0;

        /**
         * タグを追加する
         * Add tag.
         * @param tagText 追加するタグのテキスト Text of the tag to be added.
         */
        public void AddTag(String tagText)
        {
            // 未格納タグがあればそこに設定、なければ新規タグを追加
            if (storedTagCount < m_TagDataSet.Count)
            {
                m_TagDataSet[storedTagCount].SetDataFromText(tagText);
            }
            else
            {
                // Dataを追加する際に色を指定してListに追加する。
                // Specify a color when adding Data and add it to the List.
                TagData pNewTagData = new TagData(tagText);
                if (m_TagDataSet.Count % 2 == 0)
                {
                    pNewTagData.BackgroundColor = "tag_deep";
                }
                else
                {
                    pNewTagData.BackgroundColor = "tag_pale";
                }

                m_TagDataSet.Add(pNewTagData);
            }
            ++storedTagCount;
        }

        /**
         * タグをクリアする
         * 境界線表示のための未格納タグは配置している状態にもどしている
         * Clear the tag.
         * The non-stored tag for displaying the boundary line is returned to the state where it is arranged.
         */
        public void ClearTags()
        {
            // 一旦すべてのタグを削除する
            // Delete all tags once
            m_TagDataSet.Clear();

            // 境界線表示のため未格納タグを一定数追加する
            // Add a certain number of unstored tags for displaying the boundary line
            for (int i = 0; i < DEFAULT_TAG_LINE_NUMBER; i++)
            {
                TagData pNewTagData = new TagData();

                if (m_TagDataSet.Count % 2 == 0)
                {
                    pNewTagData.BackgroundColor = "tag_deep";
                }
                else
                {
                    pNewTagData.BackgroundColor = "tag_pale";
                }
                m_TagDataSet.Add(pNewTagData);
            }

            // 格納されているタグはないので格納タグ数は0にする
            // Since there is no tag stored, set the number of stored tags to 0
            storedTagCount = 0;
        }

        /**
         * 格納タグ数を取得する
         * return the number of stored tags
         * @return タグ数 the number of stored tags
         */
        public int GetStoredTagCount()
        {
            return storedTagCount;
        }

        #endregion

        /**
         * 読み込みアクション
         * Read Action
         */
        private class ReadAction
        {
            public enum ReadActionType
            {
                START      // 読み込みを開始する start reading 
                , STOP     // 読み込みを停止する stop reading
            }

            public ReadAction(ReadActionType IN_ReadActionType)
            {
                m_ReadActionType = IN_ReadActionType;
            }

            public ReadActionType m_ReadActionType { get; set; }

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
                    case ReadActionType.START:
                        return AppResources.start;
                    case ReadActionType.STOP:
                        return AppResources.stop;
                    default:
                        throw new Exception();
                }
            }
        }

        #region Event Listeners
        public void OnRFIDDataReceived(ICommScanner scanner, RFIDDataReceivedEvent rfidEvent)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                ReadData(rfidEvent);
                // OnScrollにイベントが発行されないため、手動で表示範囲を更新する
                //refreshShowRangeIfNeeded();
                // タグ数refreshTotalTagsに更新があるのでTotalTagsを更新する
                // Updated TotalTags because there is an update in tag number refreshTotalTags
                RefreshTotalTags();

                // タグ追加時は一番下にスクロールする
                // Scroll to the bottom when tags are added.
                var v = list_view.ItemsSource.Cast<object>().ElementAt(storedTagCount - 1);
                list_view.ScrollTo(v, ScrollToPosition.Start, false);
            });
        }
        #endregion

        public InventoryPage()
        {
            InitializeComponent();

            #region 文言系 text
            button_navigate_up.Text = AppResources.navigate_up;
            text_title_inventory.Text = AppResources.inventory;
            text_total_tags_head.Text = AppResources.total_tags;
            #endregion

            list_view.ItemsSource = m_TagDataSet;
            list_view.ItemSelected += (sender, e) =>
            {
                ((ListView)sender).SelectedItem = null;
            };

            // 境界線を表示するため、ダミーデータを一定数追加する
            // In order to display the boundary line, add a certain number of dummy data
            for (int i = 0; i < DEFAULT_TAG_LINE_NUMBER; i++)
            {
                TagData pNewTagData = new TagData("");

                if (m_TagDataSet.Count % 2 == 0)
                {
                    pNewTagData.BackgroundColor = "tag_deep";
                }
                else
                {
                    pNewTagData.BackgroundColor = "tag_pale";
                }
                m_TagDataSet.Add(pNewTagData);
            }

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

            if (scannerConnectedOnCreate)
            {
                try
                {
                    m_hCommonBase.GetCommScanner().GetRFIDScanner().SetDataDelegate(this);
                }
                catch (Exception /* e */)
                {
                    // データリスナーの登録に失敗
                    // Failed to set data listener.
                    m_pCommonUtilWrapper.ShowMessage(AppResources.E_MSG_COMMUNICATION);
                }
            }
            else
            {
                // SP1が見つからなかった時はエラーメッセージ表示
                // Show error message if SP1 is not found.
                m_pCommonUtilWrapper.ShowMessage(AppResources.E_MSG_NO_CONNECTION);
            }

            m_pCommonUtilWrapper.StartService();
        }

        protected override void OnDisappearing()
        {
            if (scannerConnectedOnCreate && disposeFlg)
            {
                m_hCommonBase.DisconnectCommScanner();
            }

            #region Android限定イベントの解除 release android event
            ICommonEventWrapper pEvent = DependencyService.Get<ICommonEventWrapper>();
            pEvent.OnUserLeaveHint -= OnUserLeaveHint;
            pEvent.OnRestart -= OnRestart;
            #endregion

            base.OnDisappearing();
        }

        protected override bool OnBackButtonPressed()
        {
            if (scannerConnectedOnCreate)
            {
                // タグ読み込みが開始中の場合
                // When status is reading.
                if (nextReadAction.m_ReadActionType == ReadAction.ReadActionType.STOP)
                {
                    // タグ読み込み終了
                    // Finish reading action
                    RunReadAction();
                }

                // delegateの除去
                // Remove delegate
                m_hCommonBase.GetCommScanner().GetRFIDScanner().SetDataDelegate(null);
            }

            disposeFlg = false;
            this.Navigation.PopModalAsync();

            return true;
        }

        /// <summary>
        ///  Android限定のイベント android event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnUserLeaveHint(object sender, EventArgs e)
        {
            if (scannerConnectedOnCreate)
            {
                //　タグ読み込みが開始中の場合
                // Next action is stop.
                if (nextReadAction.m_ReadActionType == ReadAction.ReadActionType.STOP)
                {
                    // タグ読み込み終了
                    // Finish reading action
                    RunReadAction();
                    disposeFlg = false;
                }
            }
        }

        /// <summary>
        /// Android限定のイベント android event
        /// </summary>
        private void OnRestart(object sender, EventArgs e)
        {
            disposeFlg = true;
        }

        /**
         * TotalTagsを更新する
         * Update the total number of tags.
         */
        private void RefreshTotalTags()
        {
            //ObservableCollection<TagData> pList = (ObservableCollection<TagData>)list_view.ItemsSource;

            Text_Total_Tags_Value.Text = storedTagCount.ToString();

        }

        /**
         * 読み込みアクションを実行する
         * Execute the reading action
         */
        private void RunReadAction()
        {
            // 設定された読み込みアクションを実行する
            // Execute the set reading action.
            switch (nextReadAction.m_ReadActionType)
            {
                case ReadAction.ReadActionType.START:
                    // タグ読み込み開始
                    // Execute the set reading action
                    if (scannerConnectedOnCreate)
                    {
                        try
                        {
                            m_hCommonBase.GetCommScanner().GetRFIDScanner().OpenInventory();
                        }
                        catch (Exception e)
                        {
                            m_pCommonUtilWrapper.ShowMessage(AppResources.E_MSG_COMMUNICATION);
                            System.Diagnostics.Debug.WriteLine(e.StackTrace);
                        }
                    }
                    break;

                case ReadAction.ReadActionType.STOP:
                    // タグ読み込み終了
                    // Finish reading tags
                    if (scannerConnectedOnCreate)
                    {
                        try
                        {
                            m_hCommonBase.GetCommScanner().GetRFIDScanner().Close();
                        }
                        catch (Exception e)
                        {
                            m_pCommonUtilWrapper.ShowMessage(AppResources.E_MSG_COMMUNICATION);
                            System.Diagnostics.Debug.WriteLine(e.StackTrace);
                        }
                    }
                    break;
            }

            // 次の読み込みアクションを設定する 前の読み込みアクションがSTARTならSTOPに、STOPならSTARTに切り替える
            // Set next read action. If the previous reading action is START switch to STOP, STOP switch to START.
            nextReadAction.m_ReadActionType = nextReadAction.m_ReadActionType == ReadAction.ReadActionType.START ? ReadAction.ReadActionType.STOP : ReadAction.ReadActionType.START;

            // ボタンには、次に実行するアクション名を設定する
            // Set the name of the action to be executed next to the button
            button_switch.Text = nextReadAction.ToResourceString();

        }

        /**
         * 画面遷移でいう上の階層に移動する
         * Move to the upper level in the screen transition
         */
        private void NavigateUp()
        {
            if (scannerConnectedOnCreate)
            {
                // タグ読み込みが開始中の場合
                // Next action is stop.
                if (nextReadAction.m_ReadActionType == ReadAction.ReadActionType.STOP)
                {
                    // タグ読み込み終了
                    // Finish reading tags.
                    RunReadAction();
                }

                // delegateの除去
                // Remove listener
                m_hCommonBase.GetCommScanner().GetRFIDScanner().SetDataDelegate(null);
            }

            disposeFlg = false;

            this.Navigation.PopModalAsync();
        }

        private void TapGestureRecognizer_Tapped(object sender, EventArgs e)
        {
            NavigateUp();
        }

        private void button_switch_Clicked(object sender, EventArgs e)
        {
            RunReadAction();
        }

        public void ReadData(RFIDDataReceivedEvent rfidDataReceivedEvent)
        {
            for (int i = 0; i < rfidDataReceivedEvent.RFIDData.Count; i++)
            {
                String data = "";
                byte[] uii = rfidDataReceivedEvent.RFIDData[i].GetUII();
                for (int loop = 0; loop < uii.Length; loop++)
                {
                    data += uii[loop].ToString("X2");
                }

                AddTag(data);
            }
        }

        private void list_view_SizeChanged(object sender, EventArgs e)
        {
            double dHeight = ((ListView)sender).Height;
            list_view.RowHeight = (int)(dHeight / (double)DEFAULT_TAG_LINE_NUMBER);
        }
    }
}