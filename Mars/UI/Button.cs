using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using System.Windows.Controls;

namespace Mars.UI
{
    internal class Button : UIElement
    {
        private System.Windows.Controls.Button _button;

        public delegate void ClickEventHandler();

        public event ClickEventHandler Click;

        public Button(System.Windows.Controls.Button target, Dispatcher dispatcher) : base(dispatcher)
        {
            _button = target;
            _button.Click += (e, a) => Click.Invoke();
        }
    }
}