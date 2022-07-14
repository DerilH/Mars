using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading;
using System.Windows.Forms;

using SharpDX;
using SharpDX.DXGI;
using SharpDX.Direct3D11;
using Point = System.Drawing.Point;
using Rectangle = System.Drawing.Rectangle;

namespace Mars
{
    internal enum CaptureApi
    {
        Gdi,
        OutputDuplicate
    }

    internal static class Capture
    {
        //public static Capture _instance;

        private static OutputDuplication outputDuplication;
        private static SharpDX.Direct3D11.Device device;
        private static Texture2D screenTexture;

        public static CaptureApi Api { get; set; }
        public static int CaptureWidht { get; private set; }
        public static int CaptureHeight { get; private set; }

        static Capture()
        {
            Api = CaptureApi.OutputDuplicate;

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
            outputDuplication = output1.DuplicateOutput(device);
            Thread.Sleep(20); // захватчику экрана надо время проинициализироваться
        }

        public static byte[] CaptureScreen()
        {
            if (Api == CaptureApi.Gdi)
            {
                //Rectangle bounds = Screen.GetBounds(Point.Empty);
                //bitmap = new Bitmap(bounds.Width, bounds.Height);

                //using (Graphics g = Graphics.FromImage(bitmap))
                //{
                //    g.CopyFromScreen(new Point(bounds.Left, bounds.Top), Point.Empty, bounds.Size);
                //}
            }
            else
            {
                Bitmap bitmap = new Bitmap(CaptureWidht, CaptureHeight, PixelFormat.Format32bppArgb);
                SharpDX.DXGI.Resource screenResource = null;
                try
                {
                    Result res = outputDuplication.TryAcquireNextFrame(10, out OutputDuplicateFrameInformation duplicateFrameInformation, out screenResource);
                    if (res != Result.Ok) return new byte[0];

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

                    MemoryStream ms = new MemoryStream();
                    bitmap.Save(ms, ImageFormat.Bmp);
                    return ms.ToArray();

                    //unsafe
                    //{
                    //    Span<byte> byteArray = new Span<byte>(sourcePtr.ToPointer(), mapSource.RowPitch * CaptureHeight);
                    //    return byteArray.ToArray();
                    //}
                }
                catch (SharpDXException ex)
                {
                    MessageBox.Show(ex.Message);
                }
                finally
                {
                    screenResource?.Dispose();
                }
                return new byte[0];
            }
            return new byte[0];
        }

        public static bool Capturing { get; private set; } = false;
        private static CancellationTokenSource captureToken;

        public delegate void NewFrameEventHandler(byte[] frame);

        public static event NewFrameEventHandler OnNewFrame = (frame) => { };

        private static byte[] _currentFrame;

        public static void StartCaptureScreen(int FramesPerSecond = 60)
        {
            if (Capturing)
                throw new Exception("Capturing already started");

            captureToken = new CancellationTokenSource();
            int deltaTime = 1000 / FramesPerSecond;

            Thread capturingThread = new Thread(() =>
            {
                while (!captureToken.Token.IsCancellationRequested)
                {
                    _currentFrame = CaptureScreen();

                    OnNewFrame.Invoke(_currentFrame);
                    Thread.Sleep(deltaTime);
                }
                Capturing = false;
                _currentFrame = null;
            });
            capturingThread.Start();
            Capturing = true;
        }

        public static void EndCaptureScreen()
        {
            captureToken?.Cancel();
        }

        public static Bitmap CaptureCurrentWindow()
        {
            Bitmap bitmap;
            IntPtr windowPtr = WindowsManager.GetForegroundWindow();
            if (Api == CaptureApi.Gdi)
            {
                Rect windowRect = new Rect();
                WindowsManager.GetWindowRect(windowPtr, ref windowRect);
                Rectangle bounds = new Rectangle(windowRect.Left, windowRect.Top, windowRect.Right - windowRect.Left, windowRect.Bottom - windowRect.Top);
                bitmap = new Bitmap(bounds.Width, bounds.Height);
                using (Graphics g = Graphics.FromImage(bitmap))
                {
                    WindowsManager.PrintWindow(windowPtr, g.GetHdc(), 0);
                }
            }
            else
            {
                bitmap = new Bitmap(1, 1);
            }
            return bitmap;
        }

        public static Bitmap CaptureWindow(IntPtr hwnd)
        {
            Bitmap bitmap;
            IntPtr windowPtr = hwnd;
            if (Api == CaptureApi.Gdi)
            {
                Rect windowRect = new Rect();
                WindowsManager.GetWindowRect(windowPtr, ref windowRect);
                Rectangle bounds = new Rectangle(windowRect.Left, windowRect.Top, windowRect.Right - windowRect.Left, windowRect.Bottom - windowRect.Top);
                bitmap = new Bitmap(bounds.Width, bounds.Height);

                using (Graphics g = Graphics.FromImage(bitmap))
                {
                    WindowsManager.PrintWindow(windowPtr, g.GetHdc(), 0);
                }
            }
            else
            {
                bitmap = new Bitmap(1, 1);
            }

            return bitmap;
        }
    }
}