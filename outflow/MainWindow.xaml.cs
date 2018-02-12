using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using MahApps.Metro.Controls;
using Microsoft.Win32;
using MonoTorrent.Client;
using MonoTorrent.Common;

namespace Outflow
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    public partial class MainWindow : MetroWindow
    {
        public ObservableCollection<TorrentWrapper> TorrentsList { get; set; }
        private ClientEngine engine = new ClientEngine(new EngineSettings());

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
            chooseTorrentFileDialog.Filter = "Torrent-files (*.torrent)|*.torrent";
            if (chooseTorrentFileDialog.ShowDialog() == true)
            {
                Torrent torrent = Torrent.Load(chooseTorrentFileDialog.FileName);
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
                    if (dialogWIndow.startCheckBox.IsChecked == true)
                        StartDownload(TorrentsList.Last());
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
            };
        }



        private void StartDownloadButton_Click(object sender, RoutedEventArgs e)
        {
            if (TorrentsDataGrid.SelectedItem != null)
            {
                StartDownload((TorrentWrapper)TorrentsDataGrid.SelectedItem);
                //for (int i = 0; i < 1000;i++)
                //{
                //    Console.WriteLine($"{((TorrentWrapper)TorrentsDataGrid.SelectedItem).Manager.Peers.Seeds}   {((TorrentWrapper)TorrentsDataGrid.SelectedItem).Manager.Peers.Leechs}");
                //    Thread.Sleep(500);
                //}
            }
        }




        private void PauseDownloadButton_Click(object sender, RoutedEventArgs e)
        {
            if (TorrentsDataGrid.SelectedItem != null)
            {
                if (((TorrentWrapper) TorrentsDataGrid.SelectedItem).Manager.State == TorrentState.Downloading)
                {
                    ((TorrentWrapper)TorrentsDataGrid.SelectedItem).PauseTorrent();
                }
                else if (((TorrentWrapper)TorrentsDataGrid.SelectedItem).Manager.State == TorrentState.Paused)
                {
                    ((TorrentWrapper)TorrentsDataGrid.SelectedItem).ResumeTorrent();
                }
                

            }
            
        }

        private void ClientMainWindow_Closed(object sender, EventArgs e)
        {
            foreach (var torrentWrapper in TorrentsList)
            {
                torrentWrapper.PauseTorrent();
            }
        }
    }
}
