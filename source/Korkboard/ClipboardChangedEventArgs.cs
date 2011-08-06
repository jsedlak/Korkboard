using System;
using System.Windows;

namespace Korkboard
{
    public class ClipboardChangedEventArgs : EventArgs
    {
        public ClipboardChangedEventArgs() : this(null) { }

        public ClipboardChangedEventArgs(IDataObject data)
        {
            Data = data;
        }

        public IDataObject Data { get; private set; }
    }
}
