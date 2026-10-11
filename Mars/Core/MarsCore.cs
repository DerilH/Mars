using System;
using Mars;
using Mars.UI;
using Mars.Streaming;
using Mars.Viewing;
using Mars.Connection;

namespace Mars.Core
{
    internal class MarsCore

    {
        private MainWindow _window;
        public static MarsCore instance { get; private set; }
        public UIManager UI { get; private set; }
        public StreamingManager Streaming { get; private set; }
        public ViewingManager Viewing { get; private set; }
        public ConnectionManager Connection { get; private set; }

        private MarsCore(MainWindow window)
        {
            Capture.Capture.Create(Capture.CaptureApi.OutputDuplication);
            _window = window;
        }

        public static MarsCore Create(MainWindow window)
        {
            if (instance != null) return instance;
            instance = new MarsCore(window);

            return instance;
        }

        public void Run()
        {
            //Window configuration
            _window.Closed += (e, a) => Environment.Exit(0);
            UI = new UIManager(_window);
            Connection = new ConnectionManager();
            Viewing = new ViewingManager(UI, Connection);
            Streaming = new StreamingManager(UI, Viewing, Connection);
            UI.host.Click += () => Streaming.StartStream();
            UI.connect.Click += () => Viewing.StartViewing();

            // Debug.Init(UI.debugBlock);
            // MemoryBarrier.Enable();
        }
    }
}
