using ReceivingManagementSystem.Wrapper;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using Xamarin.Forms;

namespace ReceivingManagementSystem.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private string _titleMain;
        public string TitleMain
        {
            get { return _titleMain; }
            set { this.SetProperty(ref this._titleMain, value); }
        }

        private bool _isSettingRFID;
        public bool IsSettingRFID
        {
            get { return _isSettingRFID; }
            set { this.SetProperty(ref this._isSettingRFID, value); }
        }

        public ICommand ShowVersionCommand { get; }
        public ICommand OrderCommand { get; }
        public ICommand ChangeTabCommand { get; }


        private ISaveSettingsWrapper _pSaveSettingsWrapper;

        public MainViewModel(ContentPage owner) : base(owner)
        {
            _titleMain = "預り業務";

            ShowVersionCommand = new Command(ShowVersion);
            OrderCommand = new Command(ShowOrder);
            ChangeTabCommand = new Command<string>(ChangeTab);

            _pSaveSettingsWrapper = DependencyService.Get<ISaveSettingsWrapper>();

            CheckSettingRFID();
        }

        private void CheckSettingRFID()
        {
            string device = _pSaveSettingsWrapper.GetString(ReceivingManagementSystem.Common.Constants.Setting_Device, "");

            IsSettingRFID = !string.IsNullOrEmpty(device);
        }

        private void ShowVersion()
        {

        }

        private void ShowOrder()
        {

        }

        private void ChangeTab(string tabName)
        {
            if (tabName == "商品")
            {
                TitleMain = "商品業務";
            }
            else
            {
                TitleMain = "預り業務";
            }
        }
    }
}
