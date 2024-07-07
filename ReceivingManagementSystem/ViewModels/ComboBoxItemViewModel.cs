
namespace ReceivingManagementSystem.ViewModels
{
    public class ComboBoxItemViewModel : BaseModel
    {
        private string _displayValue;
        public string DisplayValue
        {
            get => _displayValue;
            set => this.SetProperty(ref _displayValue, value);
        }

        private object _value;
        public object Value
        {
            get => _value;
            set => this.SetProperty(ref _value, value);
        }

        private object _id;
        public object Id
        {
            get => _id;
            set => this.SetProperty(ref _id, value);
        }

        public override string ToString()
        {
            return DisplayValue;
        }
    }
}