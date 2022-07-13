using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Runtime.InteropServices;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.IO;

namespace Mars.UI
{
    internal class WindowsListController
    {
        public ListBox List { get; private set; }
        private Dictionary<string, IntPtr> openedWindows = new Dictionary<string, IntPtr>();
        public IntPtr SelectedHwnd { get; private set; }

        public WindowsListController(ListBox target)
        {
            this.List = target;
            Update();
        }

        public void Update()
        {
            Dictionary<string, IntPtr> windows = WindowsManager.GetOpenedWindows();
            List.Items.Clear();
            foreach (var pair in windows)
            {
                ListBoxItem item = new ListBoxItem();
                StackPanel sp = new StackPanel
                {
                    Orientation = Orientation.Horizontal
                };
                Image img = new Image
                {
                    Source = WindowsManager.GetWindowIcon(pair.Value),
                    Width = 20,
                    Height = 20
                };
                TextBlock txt = new TextBlock
                {
                    Text = pair.Key
                };
                sp.Children.Add(img);
                sp.Children.Add(txt);
                item.Content = sp;
                item.Selected += (e, a) =>
                {
                    SelectedHwnd = pair.Value;
                };
                List.Items.Add(item);
            }
        }

        public IntPtr GetHwnd(string title)
        {
            return openedWindows[title];
        }
    }
}