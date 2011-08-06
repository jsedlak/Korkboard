using System;
using System.Windows;
using System.Windows.Interop;
using System.Runtime.InteropServices;

namespace Korkboard
{
    public class ClipboardMonitor : IDisposable
    {
        const int WM_DRAWCLIPBOARD = 0x308;
        const int WM_CHANGECBCHAIN = 0x30D;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern IntPtr SetClipboardViewer(IntPtr hWnd);

        [DllImport("user32.dll")]
        static extern bool ChangeClipboardChain(IntPtr hWnd, IntPtr hWndNext);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern long SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        public event ClipboardChangedHandler Changed;

        public ClipboardMonitor()
        {
        }

        ~ClipboardMonitor()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool disposing)
        {
            if (!IsDisposed && disposing)
            {
                ChangeClipboardChain(Handle, NextClipboardHandle);

                Handle = IntPtr.Zero;
                NextClipboardHandle = IntPtr.Zero;

                IsDisposed = true;
            }
        }

        public void Initialize(Window window)
        {
            Console.WriteLine("Initializing Monitor");

            HwndSource source = PresentationSource.FromVisual(window) as HwndSource;
            source.AddHook(WndProc);

            WindowInteropHelper wih = new WindowInteropHelper(window);
            Handle = wih.Handle;

            NextClipboardHandle = SetClipboardViewer(Handle);
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case WM_DRAWCLIPBOARD:
                    Console.WriteLine("hWnd: {0}; wParam: {1}; lParam: {2}, handled: {3}", hwnd, wParam, lParam, handled);

                    IDataObject data = new DataObject();
                    data = Clipboard.GetDataObject();

                    if (Changed != null)
                    {
                        Changed.Invoke(this, new ClipboardChangedEventArgs(data));

                        handled = true;
                    }

                    SendMessage(NextClipboardHandle, msg, wParam, lParam);
                    break;
                case WM_CHANGECBCHAIN:
                    if (wParam == NextClipboardHandle)
                        NextClipboardHandle = lParam;
                    else
                        SendMessage(NextClipboardHandle, msg, wParam, lParam);
                    break;
            }

            return IntPtr.Zero;
        }

        public bool IsDisposed { get; private set; }
        public IntPtr Handle { get; private set; }
        public IntPtr NextClipboardHandle { get; private set; }
    }
}
