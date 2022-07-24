using System;
using System.Collections.Generic;
using System.Windows;
using Mars.Core;

namespace Mars
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            MarsCore mars = MarsCore.Create(this);
            mars.Run();

            Debugging.Debug.Log("Strated");
        }
    }
}