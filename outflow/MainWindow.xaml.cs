using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using MahApps.Metro.Controls;
using Microsoft.Win32;
using MonoTorrent.Client;
using MonoTorrent.Common;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Input;

namespace Outflow
{
    public partial class MainWindow
    {

        public MainWindow() => InitializeComponent();

        private void TorrentsDataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = (e.Row.GetIndex() + 1).ToString();
        }

        private void CloseWindowCommandHandler(object sender, ExecutedRoutedEventArgs e)
        {
            (DataContext as ApplicationViewModel)?.StoreTorrents();
        }

        private void OpenWindowCommandHandler(object sender, EventArgs e)
        {
            (DataContext as ApplicationViewModel)?.RestoreTorrents();
        }
    }
}
