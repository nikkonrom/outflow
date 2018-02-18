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

namespace Outflow
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    public partial class MainWindow : MetroWindow
    {
        
        public MainWindow()
        {
            InitializeComponent();
        }

        private void TorrentsDataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = (e.Row.GetIndex() + 1).ToString();
        }

        

        private void StartDownloadButton_Click(object sender, RoutedEventArgs e)
        {
            if (TorrentsDataGrid.SelectedItem != null)
            {

                if (((TorrentWrapper)TorrentsDataGrid.SelectedItem).Manager.State == TorrentState.Stopped)
                {
                    //StartDownload((TorrentWrapper)TorrentsDataGrid.SelectedItem);
                }
                else if (((TorrentWrapper)TorrentsDataGrid.SelectedItem).Manager.State == TorrentState.Paused)
                {
                    ((TorrentWrapper)TorrentsDataGrid.SelectedItem).ResumeTorrent();
                }
            }
        }
        
        private void PauseDownloadButton_Click(object sender, RoutedEventArgs e)
        {
            if (TorrentsDataGrid.SelectedItem != null)
            {
                if (((TorrentWrapper)TorrentsDataGrid.SelectedItem).Manager.State == TorrentState.Downloading)
                {
                    ((TorrentWrapper)TorrentsDataGrid.SelectedItem).PauseTorrent();
                }
            }

        }

        private void ClientMainWindow_Closed(object sender, EventArgs e)
        {
            //TODO refactor
            //if (torrentsHashDictianory.Count > 0)
            //{
            //    BinaryFormatter formatter = new BinaryFormatter();
            //    using (FileStream fileStream = new FileStream(hashedTorrentsListPath, FileMode.Create))
            //    {
            //        formatter.Serialize(fileStream, torrentsHashDictianory);
            //    }
            //}

        }

        private void ClientMainWindow_Initialized(object sender, EventArgs e)
        {
            //TODO refactor
            //if (File.Exists(hashedTorrentsListPath))
            //{
            //    Dictionary<string, (string, string)> localTorrentsHashDictianory;
            //    using (FileStream fileStream = new FileStream(hashedTorrentsListPath, FileMode.Open))
            //    {
            //        BinaryFormatter formatter = new BinaryFormatter();
            //        localTorrentsHashDictianory = (Dictionary<string, (string, string)>)formatter.Deserialize(fileStream);
            //    }

            //    string[] hashedTorrentsFileNames = Directory.GetFiles(hashedTorrentsFolderPath, "*.torrent");
            //    foreach (var fileName in hashedTorrentsFileNames)
            //    {
            //        try
            //        {
            //            Torrent torrent = Torrent.Load(fileName);
            //            (string, string) torrentHashInfo = localTorrentsHashDictianory[torrent.InfoHash.ToString()];
            //            TorrentsList.Add(new TorrentWrapper(torrentHashInfo.Item1, torrent));
            //            engine.Register(TorrentsList.Last().Manager);
            //            torrentsHashDictianory.Add(TorrentsList.Last().Manager.InfoHash.ToString(), (TorrentsList.Last().Manager.SavePath, TorrentsList.Last().Manager.State.ToString()));
                        
            //            if (torrentHashInfo.Item2 == "Downloading")
            //            {
            //                StartDownload(TorrentsList.Last());
            //            }
            //            else if (torrentHashInfo.Item2 == "Paused")
            //            {
            //                TorrentsList.Last().Manager.HashCheck(false);


            //                //TorrentsList.Last().PauseTorrent();
            //            }
            //        }
            //        catch (KeyNotFoundException exception)
            //        {
            //            Console.WriteLine(exception);
            //            return;
            //        }
            //    }
            //}
        }
    }
}
