using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using Xamarin.Forms;
using System.Windows.Input;

namespace ReceivingManagementSystem.ViewModels
{
    public class BasePopupViewModel : INotifyPropertyChanged
    {
        public Xamarin.CommunityToolkit.UI.Views.Popup Owner;

        #region Command

        public ICommand DismissCommand { get; set; }
        public ICommand CloseCommand { get; }
        #endregion

        public event PropertyChangedEventHandler PropertyChanged;


        public BasePopupViewModel()
        {
            CloseCommand = new Command(Close);
        }

        public BasePopupViewModel(Xamarin.CommunityToolkit.UI.Views.Popup owner)
        {
            this.Owner = owner;
            CloseCommand = new Command(Close);
        }

        public void Close()
        {
            Owner.Dismiss(null);
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
    }
}
