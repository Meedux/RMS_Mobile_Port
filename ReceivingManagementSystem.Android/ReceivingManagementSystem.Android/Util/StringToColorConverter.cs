using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Xamarin.Forms;

namespace ReceivingManagementSystem.Android.Util
{
    public class StringToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string valueAsString = value.ToString();
            switch (valueAsString)
            {
                case (""):
                    {
                        return Color.Default;
                    }
                case ("White"):
                    {
                        return Color.White;
                    }
                case ("Aquamarine"):
                    {
                        return Color.Aquamarine;
                    }
                case ("tag_deep"):
                    {
                        return Color.FromHex("B6D1D0");
                    }
                case ("tag_pale"):
                    {
                        return Color.FromHex("D9E7E6");
                    }
                default:
                    {
                        return Color.FromHex(value.ToString());
                    }
            }
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
