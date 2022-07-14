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
        private byte[] _currentFrame;
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
                MessageBox.Show("Already hosting");
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
                if (!Capture.Capturing) Capture.StartCaptureScreen(60);
                Capture.OnNewFrame += (frame) =>
                {
                    this._currentFrame = frame;
                    recordPrewiewController.Update(frame);
                };
                while (true)
                {
                    host.SendAll(_currentFrame);
                    Thread.Sleep(16);
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
                MessageBox.Show("Already receiving");
                return;
            }

            client = new Client();
            client.Connect(ip, port);
            recordPrewiewController.TogglePreview(dispatcher);
            client.DataReceived += (data) =>
            {
                recordPrewiewController.Update(data);
            };
            MessageBox.Show("Connected to host");
        }

        public void DisconnectFromHost()
        {
            State = StreamingState.Idle;
        }
    }
}