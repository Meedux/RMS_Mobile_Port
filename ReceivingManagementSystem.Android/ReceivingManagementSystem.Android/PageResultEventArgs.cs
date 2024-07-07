using System;
using System.Collections.Generic;
using System.Text;

namespace ReceivingManagementSystem.Android
{
    public class PageResultEventArgs
    {
        public bool IsOk { get; }

        public PageResultEventArgs(bool isOk)
        {
            IsOk = isOk;
        }
    }
}
