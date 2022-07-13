using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Drawing;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Threading;
using System.Windows.Threading;
using System.Runtime.InteropServices;

namespace Mars.UI
{
    internal class RecordPrewiewController
    {
        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private WindowsListController windowsListController;

        public bool Started { get; private set; } = false;
        public System.Windows.Controls.Image target { get; protected set; }

        public RecordPrewiewController(System.Windows.Controls.Image target, WindowsListController windowsListController)
        {
            this.target = target;
            this.windowsListController = windowsListController;
        }

        public Bitmap record;

        public void TogglePreview(Dispatcher dispatcher)
        {
            if (Started)
            {
                cancellationTokenSource.Cancel();
                target.Source = null;
                Started = false;
                return;
            }
            Started = true;
            cancellationTokenSource.Dispose();
            cancellationTokenSource = new CancellationTokenSource();
            Thread thread = new Thread(() =>
            {
                while (!cancellationTokenSource.Token.IsCancellationRequested)
                {
                    if (windowsListController.SelectedHwnd != IntPtr.Zero)
                    {
                        //record = capture.CaptureWindow(windowsListController.SelectedHwnd);
                    }
                    else
                    {
                        //record = capture.CaptureScreen();
                    }
                    dispatcher.Invoke(new Action(() =>
                    {
                        Update();
                    }), DispatcherPriority.SystemIdle);
                    Thread.Sleep(16);
                }
            });
            thread.Start();
        }

        [DllImport("gdi32.dll", EntryPoint = "DeleteObject")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DeleteObject([In] IntPtr hObject);

        public void Update()
        {
            if (record == null) return;

            var handle = record.GetHbitmap();
            target.Source = Imaging.CreateBitmapSourceFromHBitmap(handle, IntPtr.Zero, System.Windows.Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            DeleteObject(handle);
        }
    }
}