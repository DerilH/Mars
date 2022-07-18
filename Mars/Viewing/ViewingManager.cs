using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Drawing;
using System.Windows.Media.Imaging;
using Mars.Extensions;

namespace Mars.Viewing
{
    internal class ViewingManager
    {
        private UI.UIManager _uiManager;
        private Connection.ConnectionManager _connectionManager;
        private Bitmap _currentFrame;
        private CancellationTokenSource _viewingCancelToken;
        public bool Viewing { get; private set; } = false;

        public ViewingManager(UI.UIManager uIManager, Connection.ConnectionManager connectionManager)
        {
            _uiManager = uIManager;
            _connectionManager = connectionManager;
        }

        public void StartViewingLocal(int FraamesPerSecond = 60)
        {
            if (Viewing)
                return;
            int deltatime = 1000 / FraamesPerSecond;
            _viewingCancelToken = new CancellationTokenSource();
            Thread ViewingThread = new Thread(() =>
            {
                while (!_viewingCancelToken.Token.IsCancellationRequested)
                {
                    _uiManager.captureView.SetSourceBitmap(_currentFrame);

                    Thread.Sleep(deltatime);
                }
            });
            ViewingThread.Start();
            Viewing = true;
        }

        public void StartViewing(int FraamesPerSecond = 60)
        {
            if (Viewing)
                return;
            int deltatime = 1000 / FraamesPerSecond;
            _viewingCancelToken = new CancellationTokenSource();
            _connectionManager.StartReceive();
            _connectionManager.FrameReceived += SetNewFrame;
            Thread ViewingThread = new Thread(() =>
            {
                while (!_viewingCancelToken.Token.IsCancellationRequested)
                {
                    _uiManager.captureView.SetSourceBitmap(_currentFrame);

                    Thread.Sleep(deltatime);
                }
            });
            ViewingThread.Start();
            Viewing = true;
        }

        public void StopViewing()
        {
            if (!Viewing)
                return;
            _viewingCancelToken.Cancel();
            _connectionManager.StopReceive();
            _connectionManager.FrameReceived -= SetNewFrame;
            Viewing = false;
        }

        public void SetNewFrame(Bitmap frame)
        {
            Bitmap clone = (Bitmap)frame.Clone();
            _currentFrame = clone;
        }
    }
}