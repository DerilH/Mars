using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Drawing;
using Mars.Extensions;
using Image = System.Windows.Controls.Image;

namespace Mars.UI
{
    internal class CaptureView : UIElement
    {
        private Image _image;

        public CaptureView(Image target, Dispatcher dispatcher) : base(dispatcher)
        {
            _image = target;
        }

        public void SetSourceBitmap(Bitmap source)
        {
            if (source == null)
                return;

            BitmapSource bi = source.ToImageSource();
            bi.Freeze();
            _dispatcher.Invoke(new Action(() =>
            {
                _image.Source = bi;
            }), DispatcherPriority.Render);
        }
    }
}