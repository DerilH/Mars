using System;
using System.Collections.Generic;
using System.Windows;

namespace Mars
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MarsCore mars;

        public MainWindow()
        {
            InitializeComponent();
            mars = new MarsCore(OpenedWindows__ListBox, RecordedPrewiew);
            Closing += (e, a) => { System.Environment.Exit(0); };
        }

        private void WindowShot_Click(object sender, RoutedEventArgs e)
        {
            var selected = (KeyValuePair<string, IntPtr>)OpenedWindows__ListBox.SelectedItem;
            Capture.CaptureWindow(selected.Value).Save("window.jpg");
        }

        private void ScreenShot_Click(object sender, RoutedEventArgs e)
        {
        }

        private void ReloadList_Click(object sender, RoutedEventArgs e)
        {
            mars.windowsListController.Update();
        }

        private void StartPreview_Click(object sender, RoutedEventArgs e)
        {
            mars.recordPrewiewController.TogglePreview(this.Dispatcher);
        }
    }
}