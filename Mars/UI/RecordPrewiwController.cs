using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Drawing;
using System.Windows;
using System.Windows.Interop;
using System.IO;
using System.Windows.Media.Imaging;
using System.Threading;
using System.Windows.Threading;
using System.Runtime.InteropServices;

namespace Mars.UI
{
    internal class RecordPrewiewController
    {
        private WindowsListController windowsListController;

        public bool Started { get; private set; } = false;
        public System.Windows.Controls.Image target { get; protected set; }

        public RecordPrewiewController(System.Windows.Controls.Image target, WindowsListController windowsListController)
        {
            this.target = target;
            this.windowsListController = windowsListController;
        }

        private CancellationTokenSource previewToken = new CancellationTokenSource();
        private byte[] _currentFrame;

        public void TogglePreview(Dispatcher dispatcher, bool capture = true)
        {
            if (Started)
            {
                previewToken.Cancel();
                dispatcher.Invoke(new Action(() =>
                {
                    target.Source = null;
                }), DispatcherPriority.SystemIdle);
                Started = false;
                Capture.EndCaptureScreen();
                return;
            }
            Started = true;
            previewToken = new CancellationTokenSource();
            Thread thread = new Thread(() =>
            {
                if (!Capture.Capturing && capture) Capture.StartCaptureScreen(60);
                Capture.OnNewFrame += (frame) =>
                {
                    _currentFrame = frame;
                };
                while (!previewToken.Token.IsCancellationRequested)
                {
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

        private void Update()
        {
            if (_currentFrame == null || _currentFrame.Length == 0) return;
            MemoryStream s = new MemoryStream(_currentFrame);

            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.StreamSource = s;
            bitmap.EndInit();
            target.Source = bitmap;
        }

        public void Update(byte[] frame)
        {
            this._currentFrame = frame;
        }
    }
}