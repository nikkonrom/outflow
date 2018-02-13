using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
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
        public ObservableCollection<TorrentWrapper> TorrentsList { get; set; }
        private ClientEngine engine = new ClientEngine(new EngineSettings());
        private Dictionary<string, (string, string)> torrentsHashDictianory;

        private const string hashedTorrentsListPath = "storedtorrents\\list.data";
        private const string hashedTorrentsFolderPath = "storedtorrents\\";
        public MainWindow()
        {
            this.TorrentsList = new ObservableCollection<TorrentWrapper>();
            this.torrentsHashDictianory = new Dictionary<string, (string, string)>();
            InitializeComponent();
            //TorrentsDataGrid.ItemsSource = TorrentsList;


        }

        private void AddNewTorrent_OnClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog chooseTorrentFileDialog = new OpenFileDialog();
            chooseTorrentFileDialog.Title = "Open torrent files";
            chooseTorrentFileDialog.Filter = "Torrent-files (*.torrent)|*.torrent";

            if (chooseTorrentFileDialog.ShowDialog() == true)
            {
                Directory.CreateDirectory(hashedTorrentsFolderPath);
                var storedTorrentspath = hashedTorrentsFolderPath + chooseTorrentFileDialog.SafeFileName;
                File.Copy(chooseTorrentFileDialog.FileName, storedTorrentspath);
                Torrent torrent = Torrent.Load(storedTorrentspath);
                FolderDialogWIndow dialogWIndow =
                    new FolderDialogWIndow(torrent.Name)
                    {
                        Owner = Application.Current.MainWindow,
                        WindowStartupLocation = WindowStartupLocation.CenterOwner
                    };
                dialogWIndow.ShowDialog();
                if (dialogWIndow.DialogResult == true)
                {
                    TorrentsList.Add(new TorrentWrapper(dialogWIndow.DownloadFolderPath.Text, torrent));
                    engine.Register(TorrentsList.Last().Manager);
                    torrentsHashDictianory.Add(TorrentsList.Last().Manager.InfoHash.ToString(), (TorrentsList.Last().Manager.SavePath, TorrentsList.Last().Manager.State.ToString()));
                    if (dialogWIndow.startCheckBox.IsChecked == true)
                        StartDownload(TorrentsList.Last());
                }
                else
                {
                    File.Delete(storedTorrentspath);
                }
            }
        }

        private void TorrentsDataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = (e.Row.GetIndex() + 1).ToString();
        }

        private async void StartDownload(TorrentWrapper selectedWrapper)
        {
            var barProgress = new Progress<double>(value => selectedWrapper.Progress = value);
            var stateProgress = new Progress<TorrentState>(value => selectedWrapper.State = value);
            var stringProgress = new Progress<string>(value => selectedWrapper.ProgressString = value);
            var downloadSpeedProgress = new Progress<string>(value => selectedWrapper.DownloadSpeed = value);

            double resultProgress = await Task.Factory.StartNew(() => selectedWrapper.StartLoadAndProgressBarReporter(barProgress),
                creationOptions: TaskCreationOptions.LongRunning);
            selectedWrapper.Progress = resultProgress;

            TorrentState resultState = await Task.Factory.StartNew(
                () => selectedWrapper.TorrentStateReporter(stateProgress),
            creationOptions: TaskCreationOptions.LongRunning);
            selectedWrapper.State = resultState;

            string resultProgressString = await Task.Factory.StartNew(
                () => selectedWrapper.ProgressStringReporter(stringProgress),
                creationOptions: TaskCreationOptions.LongRunning);
            selectedWrapper.ProgressString = resultProgressString;

            string resultDownloadSpeed = await Task.Factory.StartNew(
                () => selectedWrapper.DownloadSpeedReporter(downloadSpeedProgress),
                creationOptions: TaskCreationOptions.LongRunning);
            selectedWrapper.DownloadSpeed = resultDownloadSpeed;

            selectedWrapper.Manager.TorrentStateChanged += delegate (object o, TorrentStateChangedEventArgs args)
            {
                engine.DiskManager.Flush(selectedWrapper.Manager);
                torrentsHashDictianory[selectedWrapper.Torrent.InfoHash.ToString()] = (torrentsHashDictianory[selectedWrapper.Torrent.InfoHash.ToString()].Item1, args.NewState.ToString());
            };
        }



        private void StartDownloadButton_Click(object sender, RoutedEventArgs e)
        {
            if (TorrentsDataGrid.SelectedItem != null)
            {

                if (((TorrentWrapper)TorrentsDataGrid.SelectedItem).Manager.State == TorrentState.Stopped)
                {
                    StartDownload((TorrentWrapper)TorrentsDataGrid.SelectedItem);
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
            if (torrentsHashDictianory.Count > 0)
            {
                BinaryFormatter formatter = new BinaryFormatter();
                using (FileStream fileStream = new FileStream(hashedTorrentsListPath, FileMode.Create))
                {
                    formatter.Serialize(fileStream, torrentsHashDictianory);
                }
            }

        }

        private void ClientMainWindow_Initialized(object sender, EventArgs e)
        {
            if (File.Exists(hashedTorrentsListPath))
            {
                Dictionary<string, (string, string)> localTorrentsHashDictianory;
                using (FileStream fileStream = new FileStream(hashedTorrentsListPath, FileMode.Open))
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    localTorrentsHashDictianory = (Dictionary<string, (string, string)>)formatter.Deserialize(fileStream);
                }

                string[] hashedTorrentsFileNames = Directory.GetFiles(hashedTorrentsFolderPath, "*.torrent");
                foreach (var fileName in hashedTorrentsFileNames)
                {
                    try
                    {
                        Torrent torrent = Torrent.Load(fileName);
                        (string, string) torrentHashInfo = localTorrentsHashDictianory[torrent.InfoHash.ToString()];
                        TorrentsList.Add(new TorrentWrapper(torrentHashInfo.Item1, torrent));
                        engine.Register(TorrentsList.Last().Manager);
                        torrentsHashDictianory.Add(TorrentsList.Last().Manager.InfoHash.ToString(), (TorrentsList.Last().Manager.SavePath, TorrentsList.Last().Manager.State.ToString()));
                        StartDownload(TorrentsList.Last());
                        if (torrentHashInfo.Item2 == "Paused")
                        {
                            TorrentsList.Last().PauseTorrent();
                        }
                    }
                    catch (KeyNotFoundException exception)
                    {
                        Console.WriteLine(exception);
                        return;
                    }
                }
            }
        }
    }
}
