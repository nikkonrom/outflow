﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        public ObservableCollection<TorrentWrapper> TorrentsList { get; set; }

        public MainWindow()
        {
            this.TorrentsList = new ObservableCollection<TorrentWrapper>();
            InitializeComponent();
            //TorrentsDataGrid.ItemsSource = TorrentsList;


        }

        private void AddNewTorrent_OnClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog chooseTorrentFileDialog = new OpenFileDialog();
            chooseTorrentFileDialog.Title = "Open torrent files";
            chooseTorrentFileDialog.Filter = "Торрент-файлы (*.torrent)|*.torrent";
            if (chooseTorrentFileDialog.ShowDialog() == true)
            {
                Torrent torrent = Torrent.Load(chooseTorrentFileDialog.FileName);
                FolderDialogWIndow dialogWIndow = new FolderDialogWIndow(torrent.Name);
                dialogWIndow.Owner = Application.Current.MainWindow;
                dialogWIndow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                dialogWIndow.ShowDialog();
                if (dialogWIndow.DialogResult == true)
                    TorrentsList.Add(new TorrentWrapper(dialogWIndow.DownloadFolderPath.Text, torrent));


            }
        }

        private void TorrentsDataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = (e.Row.GetIndex() + 1).ToString();
        }
    }
}