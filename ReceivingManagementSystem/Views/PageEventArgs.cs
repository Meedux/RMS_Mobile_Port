using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace ReceivingManagementSystem.Views
{
    public class PageEventArgs : EventArgs
    {
        public object ReturnValue { get; }

        public PageEventArgs(object returnValue)
        {
            ReturnValue = returnValue;
        }
    }
}
