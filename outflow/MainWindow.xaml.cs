using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MahApps.Metro.Controls;
using Microsoft.Win32;
using MonoTorrent.Common;

namespace Outflow
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        List<TorrentWrapper> torrentsList = new List<TorrentWrapper>();

        public MainWindow()
        {
            InitializeComponent();

        }

        private void AddNewTorrent_OnClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog chooseTorrentFileDialog = new OpenFileDialog();
            chooseTorrentFileDialog.Title = "Open torrent files";
            chooseTorrentFileDialog.Filter = "Торрент-файлы (*.torrent)|*.torrent";
            if (chooseTorrentFileDialog.ShowDialog() == true)
            {
                Torrent torrent = Torrent.Load(chooseTorrentFileDialog.FileName);
                FolderDialogWIndow dialogWIndow = new FolderDialogWIndow();
                dialogWIndow.Owner = Application.Current.MainWindow;
                dialogWIndow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                dialogWIndow.ShowDialog();
            }
        }
    }
}
