using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ReceivingManagementSystem.Common
{
    public static class EventExtensions
    {
        public static void Raise<TEventArgs>(
            this EventHandler<TEventArgs> eventHandler,
            object sender,
            TEventArgs args) where TEventArgs : EventArgs
        {
            var handler = Volatile.Read(ref eventHandler);

            handler?.Invoke(sender, args);
        }
    }
}
