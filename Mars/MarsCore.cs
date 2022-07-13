using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Windows.Controls;
using Mars.UI;
using Mars.Connection;
using SharpDX;
using SharpDX.Win32;
using SharpDX.Direct3D11;
using SharpDX.DXGI;

namespace Mars
{
    internal class MarsCore
    {
        public WindowsListController windowsListController { get; private set; }
        public RecordPrewiewController recordPrewiewController { get; private set; }
        public StreamingManager streamingManager { get; private set; }

        public MarsCore(ListBox WindowsList, Image RecordPreview, Dispatcher dispatcher)
        {
            this.windowsListController = new WindowsListController(WindowsList);
            this.recordPrewiewController = new RecordPrewiewController(RecordPreview, windowsListController);
            this.streamingManager = new StreamingManager(recordPrewiewController, dispatcher);
        }
    }
}