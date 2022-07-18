using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Mars.UI
{
    internal class Input : UIElement
    {
        private TextBox _textBox;
        public string DefaultText { get; }

        public Input(TextBox target, Dispatcher dispatcher) : base(dispatcher)
        {
            _textBox = target;
            DefaultText = _textBox.Text;
        }

        public string Read()
        {
            return _textBox.Text;
        }

        public void SetDefault()
        {
            _dispatcher.Invoke(() =>
            {
                _textBox.Text = DefaultText;
            });
        }
    }
}