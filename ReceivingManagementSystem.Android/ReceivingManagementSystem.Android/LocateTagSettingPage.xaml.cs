using ReceivingManagementSystem.Android.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ReceivingManagementSystem.Android
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class LocateTagSettingPage : ContentPage
	{
        #region Property
        //private bool disposeFlg = true;

        #endregion

        CommonBase m_hCommonBase = CommonBase.GetInstance();

        // 本体に保存していて、アプリを終了後も保持するフィルタ設定値
        // SetFilterが成功したときのみに書き込みを行い、Loadボタン押下時に保持している値をテキストエリアに表示
        ISaveSettingsWrapper m_pSaveSettingsWrapper = DependencyService.Get<ISaveSettingsWrapper>();
        ICommonUtilWrapper m_pCommonUtilWrapper = DependencyService.Get<ICommonUtilWrapper>();

        Assembly m_pAssembly = null;

        public LocateTagSettingPage ()
		{
			InitializeComponent ();

            m_pAssembly = this.GetType().Assembly;

            #region 文言系
            text_title_locate_tag.Text = AppResources.range_setting_title;
            #endregion

            image_setting_radar.Source = ImageSource.FromResource("ReceivingManagementSystem.Resource.locate_tag_setting_radar.png", m_pAssembly);

            stage2_max_read_power_level_on_search.Text = Math.Abs (Math.Round((m_hCommonBase.fStage2_Max_Read_Power_Level * 10.0))).ToString();
            stage3_max_read_power_level_on_search.Text = Math.Abs (Math.Round((m_hCommonBase.fStage3_Max_Read_Power_Level * 10.0))).ToString();
            stage4_max_read_power_level_on_search.Text = Math.Abs (Math.Round((m_hCommonBase.fStage4_Max_Read_Power_Level * 10.0))).ToString();
            stage5_max_read_power_level_on_search.Text = Math.Abs (Math.Round((m_hCommonBase.fStage5_Max_Read_Power_Level * 10.0))).ToString();
        }

        #region Handle click event
        private void TapGestureRecognizer_Tapped(object sender, EventArgs e)
        {
            NavigateUp();
        }
        #endregion

        #region Activity Relation

        protected override bool OnBackButtonPressed()
        {
            NavigateUp();

            return true;
        }

        /**
         * 画面遷移でいう上の階層に移動する
         * Move to the upper level in the screen transition.
         */
        private void NavigateUp()
        {
            //disposeFlg = false;

            if (SaveData() == true)
            {
                this.Navigation.PopModalAsync();
            }
        }

        #endregion

        private bool SaveData()
        {
            float fStage2 = 0.0f;
            float fStage3 = 0.0f;
            float fStage4 = 0.0f;
            float fStage5 = 0.0f;
            try
            {
                fStage2 = (float)(int.Parse(stage2_max_read_power_level_on_search.Text)) / -10.0f;
                fStage3 = (float)(int.Parse(stage3_max_read_power_level_on_search.Text)) / -10.0f;
                fStage4 = (float)(int.Parse(stage4_max_read_power_level_on_search.Text)) / -10.0f;
                fStage5 = (float)(int.Parse(stage5_max_read_power_level_on_search.Text)) / -10.0f;
            }
            catch (Exception /* e */)
            {
                //Error Message
                m_pCommonUtilWrapper.ShowMessage(AppResources.E_MSG_LOCATION_TAG_RANGE);
                return false;
            }

            //条件check
            if (!(fStage2 > fStage3 && fStage3 > fStage4 && fStage4 > fStage5))
            {
                m_pCommonUtilWrapper.ShowMessage(AppResources.E_MSG_LOCATION_TAG_RANGE);
                return false;
            }

            //保存
            m_hCommonBase.fStage2_Max_Read_Power_Level = fStage2;
            m_hCommonBase.fStage3_Max_Read_Power_Level = fStage3;
            m_hCommonBase.fStage4_Max_Read_Power_Level = fStage4;
            m_hCommonBase.fStage5_Max_Read_Power_Level = fStage5;

            if (m_pSaveSettingsWrapper.SaveSettings(new SaveSettingsParam(SaveSettingsParam.SaveTypes.FLOAT, Preferences.pref_stage2_max_read_power_level_on_search, fStage2),
                                                    new SaveSettingsParam(SaveSettingsParam.SaveTypes.FLOAT, Preferences.pref_stage3_max_read_power_level_on_search, fStage3),
                                                    new SaveSettingsParam(SaveSettingsParam.SaveTypes.FLOAT, Preferences.pref_stage4_max_read_power_level_on_search, fStage4),
                                                    new SaveSettingsParam(SaveSettingsParam.SaveTypes.FLOAT, Preferences.pref_stage5_max_read_power_level_on_search, fStage5)))
            {
                m_pCommonUtilWrapper.ShowMessage(AppResources.I_MSG_LOCATION_TAG_SETTING);
            }
            else
            {
                m_pCommonUtilWrapper.ShowMessage(AppResources.E_MSG_LOCATION_TAG_SAVE_ERROR);
                return false;
            }
            
            return true;
        }
    }
}