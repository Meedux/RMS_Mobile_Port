using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using Xamarin.Forms;
using System.Windows.Input;

namespace ReceivingManagementSystem.ViewModels
{
    public class BaseModel : INotifyPropertyChanged
    {
        public ContentPage Owner;

        #region Command

        public ICommand CloseCommand { get; }
        #endregion

        public event PropertyChangedEventHandler PropertyChanged;


        public BaseModel()
        {
            CloseCommand = new Command(Close);
        }

        public BaseModel(ContentPage owner)
        {
            this.Owner = owner;
            CloseCommand = new Command(Close);
        }

        public void Close()
        {
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
    }
}
