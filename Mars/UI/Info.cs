using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Windows.Controls;

namespace Mars.UI
{
    internal class Info : UIElement
    {
        private TextBlock _textBlock;
        private ScrollViewer _scrollViewer;

        public Info(ScrollViewer target, Dispatcher dispatcher) : base(dispatcher)
        {
            _scrollViewer = target;
            _textBlock = target.Content as TextBlock;
        }

        public void Write(string text)
        {
            _dispatcher.Invoke(() =>
            {
                _textBlock.Text += text;
            });
            _scrollViewer.ScrollToEnd();
        }

        public void WriteLine(string text)
        {
            _dispatcher.Invoke(() =>
            {
                _textBlock.Text += text + Environment.NewLine;
            });
            _scrollViewer.ScrollToEnd();
        }

        public void Clear()
        {
            _dispatcher.Invoke(() =>
            {
                _textBlock.Text = "";
            });
            _scrollViewer.ScrollToEnd();
        }
    }
}