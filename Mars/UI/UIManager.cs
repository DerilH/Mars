using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Mars.UI
{
    internal class UIManager
    {
        private MainWindow window { get; }
        public Dispatcher _dispatcher;
        public CaptureView captureView;
        public Info info;
        public Input ipInput;
        public Button host, connect;

        public UIManager(MainWindow window)
        {
            this.window = window;
            _dispatcher = window.Dispatcher;
            captureView = new CaptureView(window.CaptureView, _dispatcher);
            info = new Info(window.Info, _dispatcher);
            ipInput = new Input(window.IPInput, _dispatcher);
            host = new Button(window.Host, _dispatcher);
            connect = new Button(window.Connect, _dispatcher);
        }
    }
}