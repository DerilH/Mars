using System;
using System.Drawing;
using System.Windows.Forms;
using ScreenRecorderLib;
using System.IO;
using System.Windows.Media.Imaging;

namespace Mars
{
    internal class Capture
    {
        public static Bitmap CaptureScreen()
        {
            Rectangle bounds = Screen.GetBounds(Point.Empty);
            Bitmap bitmap = new Bitmap(bounds.Width, bounds.Height);
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.CopyFromScreen(new Point(bounds.Left, bounds.Top), Point.Empty, bounds.Size);
            }
            return bitmap;
        }

        private static Recorder rec = Recorder.CreateRecorder();
        private static MemoryStream stream = new MemoryStream();

        public static BitmapImage CaptureScreenA()
        {
            rec.Record(stream);
            var imageSource = new BitmapImage();
            imageSource.BeginInit();
            imageSource.StreamSource = stream;
            imageSource.EndInit();
            ScreenRecorderLib.WindowOverlay k;
            k.
            //rec.Stop();
            return imageSource;
        }

        public static Bitmap CaptureCurrentWindow()
        {
            IntPtr windowPtr = WindowsManager.GetForegroundWindow();
            Rect windowRect = new Rect();
            WindowsManager.GetWindowRect(windowPtr, ref windowRect);
            Rectangle bounds = new Rectangle(windowRect.Left, windowRect.Top, windowRect.Right - windowRect.Left, windowRect.Bottom - windowRect.Top);
            Bitmap b = new Bitmap(bounds.Width, bounds.Height);
            using (Graphics g = Graphics.FromImage(b))
            {
                WindowsManager.PrintWindow(windowPtr, g.GetHdc(), 0);
            }
            return b;
        }

        public static Bitmap CaptureWindow(IntPtr hwnd)
        {
            IntPtr windowPtr = hwnd;
            Rect windowRect = new Rect();
            WindowsManager.GetWindowRect(windowPtr, ref windowRect);
            Rectangle bounds = new Rectangle(windowRect.Left, windowRect.Top, windowRect.Right - windowRect.Left, windowRect.Bottom - windowRect.Top);
            Bitmap b = new Bitmap(bounds.Width, bounds.Height);
            using (Graphics g = Graphics.FromImage(b))
            {
                WindowsManager.PrintWindow(windowPtr, g.GetHdc(), 0);
            }
            return b;
        }
    }
}