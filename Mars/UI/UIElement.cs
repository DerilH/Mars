using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Mars.UI
{
    internal abstract class UIElement
    {
        protected Dispatcher _dispatcher;

        public UIElement(Dispatcher dispatcher)
        {
            _dispatcher = dispatcher;
        }
    }
}