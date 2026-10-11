using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using SharpDX;
using SharpDX.DXGI;
using SharpDX.Direct3D11;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using Point = System.Drawing.Point;
using Rectangle = System.Drawing.Rectangle;

namespace Mars.Capture
{
    public enum CaptureApi
    {
        Gdi,
        OutputDuplication
    }

    internal class Capture
    {
        public static Capture instance { get; private set; }

        private OutputDuplication outputDuplication;
        private SharpDX.Direct3D11.Device device;
        private Texture2D screenTexture;
        public int CaptureWidht { get; private set; }
        public int CaptureHeight { get; private set; }
        public CaptureApi Api { get; set; }

        private CancellationTokenSource _captureCancelToken;
        private Bitmap _currentFrame;
        public bool Capturing { get; private set; }

        public delegate void NewFrameEventHandler(Bitmap frame);

        public event NewFrameEventHandler NewFrame = (frame) => { };

        private Capture(CaptureApi api)
        {
            this.Api = api;
            //Output duplication initializing
            var factory = new Factory1();
            var adapter = factory.GetAdapter1(0);
            Console.WriteLine(adapter.Description1.Description);
            device = new SharpDX.Direct3D11.Device(adapter);
            Output output = adapter.GetOutput(0);
            Console.WriteLine(output.Description.DeviceName);
            Output1 output1 = output.QueryInterface<Output1>();

            CaptureWidht = output.Description.DesktopBounds.Right;
            CaptureHeight = output.Description.DesktopBounds.Bottom;

            Texture2DDescription textureDesc = new Texture2DDescription
            {
                CpuAccessFlags = CpuAccessFlags.Read,
                BindFlags = BindFlags.None,
                Format = Format.B8G8R8A8_UNorm,
                Width = CaptureWidht,
                Height = CaptureHeight,
                OptionFlags = ResourceOptionFlags.None,
                MipLevels = 1,
                ArraySize = 1,
                SampleDescription = { Count = 1, Quality = 0 },
                Usage = ResourceUsage.Staging
            };
            screenTexture = new Texture2D(device, textureDesc);
            try
            {
                outputDuplication = output1.DuplicateOutput(device);
            }
            catch (SharpDXException ex)
            {
                Console.WriteLine($"Warning: Desktop Duplication not supported: {ex.Message}");
            }
            Thread.Sleep(20); // захватчику экрана надо время проинициализироваться
        }

        public static Capture Create(CaptureApi api)
        {
            if (instance != null)
                return instance;
            instance = new Capture(api);
            return instance;
        }

        public Bitmap CaptureScreen()
        {
            if (Api == CaptureApi.Gdi)
                return CaptureScreenGdi();
            else
                return CaptureScreenOutputDuplication();
        }

        private Bitmap CaptureScreenGdi()
        {
            Rectangle bounds = Screen.GetBounds(Point.Empty);
            Bitmap bitmap = new Bitmap(bounds.Width, bounds.Height);

            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.CopyFromScreen(new Point(bounds.Left, bounds.Top), Point.Empty, bounds.Size);
            }
            return bitmap;
        }

        private Bitmap CaptureScreenOutputDuplication()
        {
            Bitmap bitmap = new Bitmap(CaptureWidht, CaptureHeight, PixelFormat.Format32bppArgb);
            SharpDX.DXGI.Resource screenResource = null;
            Result res = outputDuplication.TryAcquireNextFrame(16, out OutputDuplicateFrameInformation duplicateFrameInformation, out screenResource);
            if (res != Result.Ok) return bitmap;

            using (Texture2D screenTexture2D = screenResource.QueryInterface<Texture2D>())
            {
                device.ImmediateContext.CopyResource(screenTexture2D, screenTexture);
            }
            DataBox mapSource = device.ImmediateContext.MapSubresource(screenTexture, 0, MapMode.Read, SharpDX.Direct3D11.MapFlags.None);
            BitmapData bmpData = bitmap.LockBits(new Rectangle(Point.Empty, bitmap.Size), ImageLockMode.WriteOnly, bitmap.PixelFormat);
            IntPtr sourcePtr = mapSource.DataPointer;
            IntPtr destPtr = bmpData.Scan0;
            Utilities.CopyMemory(destPtr, sourcePtr, mapSource.RowPitch * CaptureHeight);
            bitmap.UnlockBits(bmpData);
            device.ImmediateContext.UnmapSubresource(screenTexture, 0);
            outputDuplication.ReleaseFrame();
            return bitmap;
        }

        public void StartCapture(int FramesPerSecond = 60)
        {
            if (Capturing)
                return;
            Capturing = true;

            int deltaTime = 1000 / FramesPerSecond;

            _captureCancelToken = new CancellationTokenSource();
            Thread captureThread = new Thread(() =>
            {
                while (!_captureCancelToken.IsCancellationRequested)
                {
                    _currentFrame = CaptureScreen();
                    NewFrame.Invoke(_currentFrame);
                    Thread.Sleep(deltaTime);
                }
            });
            captureThread.Start();
        }

        public void EndCapture()
        {
            if (!Capturing)
                return;
            _captureCancelToken.Cancel();
        }
    }
}
