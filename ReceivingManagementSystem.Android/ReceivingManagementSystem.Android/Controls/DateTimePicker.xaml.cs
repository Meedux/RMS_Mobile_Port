using ReceivingManagementSystem.Android.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ReceivingManagementSystem.Android.Controls
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class DateTimePicker : ContentView, INotifyPropertyChanged
    {


        public DateTimePicker()
        {
            InitializeComponent();

            this.BindingContext = new DateTimePickerViewModel();
        }

        private void ImageButton_Clicked(object sender, EventArgs e)
        {
          
        }
    }

    public class DateTimePickerViewModel : BaseModel
    {
        private bool _isPickerOpen;
        public bool IsPickerOpen
        {
            get { return _isPickerOpen; }
            set { this.SetProperty(ref this._isPickerOpen, value); }
        }

        private DateTime _selectDate;
        public DateTime SelectDate
        {
            get { return _selectDate; }
            set { this.SetProperty(ref this._selectDate, value); }
        }


        public new event PropertyChangedEventHandler PropertyChanged;

        public ICommand OpenPickerCommand { get; }

        public DateTimePickerViewModel()
        {

            OpenPickerCommand = new Command(OpenPicker);
        }

        private void OpenPicker()
        {
            IsPickerOpen = true;
        }

        protected new bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
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