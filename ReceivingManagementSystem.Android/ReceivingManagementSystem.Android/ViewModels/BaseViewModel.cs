using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using Xamarin.Forms;
using System.Windows.Input;
using ReceivingManagementSystem.Common.Resources;
using System.Threading.Tasks;
using ReceivingManagementSystem.Android.Interfaces;
using System.Diagnostics;

namespace ReceivingManagementSystem.Android.ViewModels
{
    public class BaseViewModel : INotifyPropertyChanged
    {
        public ContentPage Owner;
        public bool IsClose;
        public bool IsOK;

        #region Command

        public ICommand CloseCommand { get; }

        #endregion

        public event PropertyChangedEventHandler PropertyChanged;


        public IResourceProvider ResourceProvider;
        public ICommonUtilWrapper CommonUtilWrapper = DependencyService.Get<ICommonUtilWrapper>();


        public BaseViewModel()
        {
            CloseCommand = new Command(Close);
            ResourceProvider = DependencyService.Get<IResourceProvider>();
            CommonUtilWrapper = DependencyService.Get<ICommonUtilWrapper>();
        }

        public BaseViewModel(ContentPage owner)
        {
            this.Owner = owner;
            CloseCommand = new Command(Close);
            ResourceProvider = DependencyService.Get<IResourceProvider>();
            CommonUtilWrapper = DependencyService.Get<ICommonUtilWrapper>();
        }

        public void Close()
        {
            this.Owner.Navigation.PopAsync();
        }

        public void Close(bool isOK)
        {
            IsOK = isOK;

            Owner.Navigation.PopAsync();
        }

        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (object.Equals(storage, value)) return false;
            storage = value;
            this.OnPropertyChaned(propertyName);
            return true;
        }

        private void OnPropertyChaned(string propertyName)
        {
            var eventHandler = this.PropertyChanged;
            if (eventHandler != null)
                eventHandler(this, new PropertyChangedEventArgs(propertyName));
        }

        public async Task ShowAlert(string title, string cancel, string messageCode, params string[] parameters)
        {
            await this.Owner.DisplayAlert(ResourceProvider.GetResourceByName(title),
                    ResourceProvider.GetMesResource(messageCode, parameters),
                   ResourceProvider.GetResourceByName(cancel));
        }

        public async Task ShowActionSheet(string title, string cancel, List<string> messages)
        {
            await Owner.DisplayActionSheet(ResourceProvider.GetResourceByName(title),
                   ResourceProvider.GetResourceByName(cancel), null, messages.ToArray());
        }

        public void ShowMessage(string messageCode, params string[] parameters)
        {
            CommonUtilWrapper.ShowMessage(ResourceProvider.GetMesResource(messageCode, parameters));
        }
    }
}
