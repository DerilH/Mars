using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Media.Imaging;
using System.Windows;
using System.IO;

namespace Mars.Extensions
{
    internal static class BitmapExtensions
    {
        public static BitmapSource ToImageSource(this Bitmap src)
        {
            //MemoryStream ms = new MemoryStream();
            //src.Save(ms, ImageFormat.Bmp);
            //BitmapImage image = new BitmapImage();
            //image.BeginInit();
            //ms.Seek(0, SeekOrigin.Begin);
            //image.StreamSource = ms;
            //image.EndInit();
            return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(src.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            //return image;
        }
    }
}