using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Drawing;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Net;

namespace Mars.Connection
{
    internal class ConnectionManager
    {
        private Bitmap _currentFrame;
        private CancellationTokenSource _hostingCancelToken, _receivingCancelToken;
        private TcpListener _listener;
        private TcpClient _client;

        public delegate void FrameReceivedEventHandler(Bitmap frame);

        public event FrameReceivedEventHandler FrameReceived = (frame) => { };

        public bool Hosting { get; private set; } = false;
        public bool Receiving { get; private set; } = false;

        public ConnectionManager()
        {
        }

        public void StartHost()
        {
            if (Hosting)
                return;
            _listener = new TcpListener(IPAddress.Parse("0.0.0.0"), 8080);
            _listener.Start();
            _hostingCancelToken = new CancellationTokenSource();
            Thread HostingThread = new Thread(() =>
            {
                TcpClient client = _listener.AcceptTcpClient();
                _listener.Stop();
                NetworkStream stream = client.GetStream();
                byte[] info = new byte[4];
                BinaryFormatter formatter = new BinaryFormatter();
                while (!_hostingCancelToken.IsCancellationRequested)
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        formatter.Serialize(ms, _currentFrame);
                        int size = (int)ms.Length;
                        info = BitConverter.GetBytes(size);
                        stream.Write(info, 0, 4);
                        stream.Write(ms.ToArray(), 0, size);
                        Thread.Sleep(1000 / 60);
                    }
                }
            });
            HostingThread.Start();
            Hosting = true;
        }

        public void StopHost()
        {
            if (!Hosting)
                return;
            _hostingCancelToken.Cancel();
            Hosting = false;
        }

        public void SetSendFrame(Bitmap frame)
        {
            if (!Hosting)
                return;
            _currentFrame = frame;
        }

        public void StartReceive()
        {
            if (Hosting || Receiving)
                return;
            _client = new TcpClient();
            _receivingCancelToken = new CancellationTokenSource();
            Thread ReceivingThread = new Thread(() =>
            {
                _client.Connect(IPAddress.Parse("127.0.0.1"), 8080);
                NetworkStream stream = _client.GetStream();
                byte[] info = new byte[4];
                byte[] data = new byte[2000000];
                BinaryFormatter formatter = new BinaryFormatter();
                Bitmap frame;
                while (!_receivingCancelToken.IsCancellationRequested)
                {
                    //if (!stream.DataAvailable)
                    //    continue;
                    try
                    {
                        stream.Read(info, 0, 4);
                        int size = BitConverter.ToInt32(info, 0);
                        stream.Read(data, 0, size);
                        using (MemoryStream ms = new MemoryStream(data))
                        {
                            frame = (Bitmap)formatter.Deserialize(ms);
                            FrameReceived.Invoke(frame);
                        }
                    }
                    catch (Exception ex) { }
                    Thread.Sleep(1000 / 60);
                }
            });
            ReceivingThread.Start();

            Receiving = true;
        }

        public void StopReceive()
        {
            if (Hosting || !Receiving)
                return;

            Receiving = false;
        }
    }
}