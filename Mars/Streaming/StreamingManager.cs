using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Drawing;

namespace Mars.Streaming
{
    internal class StreamingManager
    {
        private UI.UIManager _uiManager;
        private Viewing.ViewingManager _viewingManager;
        private Connection.ConnectionManager _connectionManager;
        public bool Streaming { get; private set; } = false;

        public StreamingManager(UI.UIManager uiManager, Viewing.ViewingManager viewingManager, Connection.ConnectionManager connectionManager)
        {
            _uiManager = uiManager;
            _viewingManager = viewingManager;
            _connectionManager = connectionManager;
        }

        public void StartStream(bool WithView = true)
        {
            if (Streaming)
                return;

            Capture.Capture.instance.StartCapture();
            _connectionManager.StartHost();

            if (WithView)
            {
                _viewingManager.StartViewingLocal(60);
                Capture.Capture.instance.NewFrame += SetNewFrameWithViewHandler;
            }

            Capture.Capture.instance.NewFrame += SetNewFrameHandler;
        }

        public void CancelStream()
        {
            if (!Streaming)
                return;
            Capture.Capture.instance.StartCapture();
            Capture.Capture.instance.NewFrame -= SetNewFrameHandler;
            Capture.Capture.instance.NewFrame -= SetNewFrameWithViewHandler;
            _connectionManager.StartHost();
        }

        private void SetNewFrameWithViewHandler(Bitmap frame)
        {
            Bitmap clone = (Bitmap)frame.Clone();
            _connectionManager.SetSendFrame(clone);
            _viewingManager.SetNewFrame(clone);
        }

        private void SetNewFrameHandler(Bitmap frame)
        {
            Bitmap clone = (Bitmap)frame.Clone();

            _connectionManager.SetSendFrame(clone);
        }
    }
}