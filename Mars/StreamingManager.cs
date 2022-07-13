using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using System.Windows.Threading;
using System.Windows;
using System.Threading.Tasks;
using Mars.Connection;
using System.Drawing;
using System.Drawing.Imaging;
using Mars.UI;

namespace Mars
{
    public enum StreamingState
    {
        Hosting,
        Receiving,
        Idle
    }

    internal class StreamingManager
    {
        private Capture _capture = new Capture(CaptureApi.OutputDuplicate);
        private Bitmap _currentFrame;
        private RecordPrewiewController recordPrewiewController;
        private Dispatcher dispatcher;
        public Host host { get; private set; }
        public Client client { get; private set; }
        public StreamingState State { get; private set; } = StreamingState.Idle;

        public StreamingManager(RecordPrewiewController recordPrewiewController, Dispatcher dispatcher)
        {
            this.recordPrewiewController = recordPrewiewController;
            this.dispatcher = dispatcher;
        }

        public void Host(string ip, int port)
        {
            if (State == StreamingState.Hosting)
            {
                MessageBox.Show("Alredy hosting");
                return;
            }
            if (State == StreamingState.Receiving)
            {
                MessageBox.Show("You can not host and receive at the same time");
                return;
            }

            host = new Host(ip, port);
            host.StartListen();

            Thread streamingThread = new Thread(() =>
            {
                while (true)
                {
                    _currentFrame = _capture.CaptureScreen();
                    byte[] data;
                    using (MemoryStream ms = new MemoryStream())
                    {
                        _currentFrame.Save(ms, ImageFormat.Bmp);
                        data = ms.ToArray();
                    }
                    host.SendAll(data);
                }
            });
            streamingThread.Start();
            State = StreamingState.Hosting;
            MessageBox.Show("Host started");
        }

        public void StopHost()
        {
            State = StreamingState.Idle;
        }

        public void ConnectToHost(string ip, int port)
        {
            if (State == StreamingState.Hosting)
            {
                MessageBox.Show("You can not host and receive at the same time");
                return;
            }
            if (State == StreamingState.Receiving)
            {
                MessageBox.Show("Alredy receiving");
                return;
            }

            client = new Client();
            client.Connect(ip, port);
            recordPrewiewController.TogglePreview(dispatcher);
            client.DataReceived += (data) =>
            {
                Bitmap bmp;
                using (var ms = new MemoryStream(data))
                {
                    bmp = new Bitmap(ms);
                }
            };
            MessageBox.Show("Connected to host");
        }

        public void DisconnectFromHost()
        {
            State = StreamingState.Idle;
        }
    }
}