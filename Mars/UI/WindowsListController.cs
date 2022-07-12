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
        public ListBox list { get; private set; }
        private Dictionary<string, IntPtr> openedWindows = new Dictionary<string, IntPtr>();
        public IntPtr SelectedHwnd { get; private set; }

        public WindowsListController(ListBox target)
        {
            this.list = target;
            Update();
        }

        public void Update()
        {
            Dictionary<string, IntPtr> windows = WindowsManager.GetOpenedWindows();
            list.Items.Clear();
            foreach (var pair in windows)
            {
                ListBoxItem item = new ListBoxItem();
                StackPanel sp = new StackPanel();
                sp.Orientation = Orientation.Horizontal;
                Image img = new Image();
                img.Source = WindowsManager.GetWindowIcon(pair.Value);
                img.Width = 20;
                img.Height = 20;
                TextBlock txt = new TextBlock();
                txt.Text = pair.Key;
                sp.Children.Add(img);
                sp.Children.Add(txt);
                item.Content = sp;
                item.Selected += (e, a) =>
                {
                    SelectedHwnd = pair.Value;
                };
                list.Items.Add(item);
            }
        }

        public IntPtr GetHwnd(string title)
        {
            return openedWindows[title];
        }
    }
}