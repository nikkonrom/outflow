using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;
using MonoTorrent.Client;
using MonoTorrent.Common;

namespace Outflow
{
    class ApplicationViewModel : INotifyPropertyChanged
    {
        private TorrentWrapper _selectedWrapper;
        private ClientEngine _engine;
        private Dictionary<string, (string, string)> _torrentsHashDictianory;

        private const string hashedTorrentsListPath = "storedtorrents\\list.data";
        private const string hashedTorrentsFolderPath = "storedtorrents\\";

        #region Commands

        private RelayCommand addCommand;

        #endregion


        #region Properties

        public TorrentWrapper SelectedWrapper
        {
            get => _selectedWrapper;
            set
            {
                _selectedWrapper = value;
                OnPropertyChanged("SelectedWrapper");
            }
        }

        public ObservableCollection<TorrentWrapper> TorrentsList { get; set; }

        public RelayCommand AddCommand => addCommand ?? (new RelayCommand(AddTorrentOpenFIleDialog));

        #endregion

        private void AddTorrentOpenFIleDialog(object arg)
        {
            OpenFileDialog chooseTorrentFileDialog = new OpenFileDialog();
            chooseTorrentFileDialog.Title = "Open torrent files";
            chooseTorrentFileDialog.Filter = "Torrent-files (*.torrent)|*.torrent";

            if (chooseTorrentFileDialog.ShowDialog() == true)
            {
                Directory.CreateDirectory(hashedTorrentsFolderPath);
                var storedTorrentsPath = hashedTorrentsFolderPath + chooseTorrentFileDialog.SafeFileName;
                File.Copy(chooseTorrentFileDialog.FileName, storedTorrentsPath);
                Torrent torrent = Torrent.Load(storedTorrentsPath);
                TorrentSettingsDialog(torrent, storedTorrentsPath);
            }
        }

        private void TorrentSettingsDialog(Torrent torrent, string storedTorrentsPath)
        {
            FolderDialogWIndow dialogWIndow =
                new FolderDialogWIndow(torrent.Name)
                {
                    Owner = Application.Current.MainWindow,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner
                };
            dialogWIndow.ShowDialog();
            if (dialogWIndow.DialogResult == true)
            {
                RegisterTorrent(torrent, dialogWIndow.DownloadFolderPath.Text);
                if (dialogWIndow.startCheckBox.IsChecked == true)
                    StartDownload();
            }
            else
            {
                File.Delete(storedTorrentsPath);
            }
        }

        private void RegisterTorrent(Torrent torrent, string downloadFolderPath)
        {
            TorrentWrapper wrapper = new TorrentWrapper(downloadFolderPath, torrent);
            TorrentsList.Add(wrapper);
            SelectedWrapper = wrapper;
            _engine.Register(SelectedWrapper.Manager);
            _torrentsHashDictianory.Add(SelectedWrapper.Manager.InfoHash.ToString(),
                (SelectedWrapper.Manager.SavePath, TorrentsList.Last().Manager.State.ToString()));
        }

        private async void StartDownload()
        {
            var barProgress = new Progress<double>(value => SelectedWrapper.Progress = value);
            var stateProgress = new Progress<TorrentState>(value => SelectedWrapper.State = value);
            var stringProgress = new Progress<string>(value => SelectedWrapper.ProgressString = value);
            var downloadSpeedProgress = new Progress<string>(value => SelectedWrapper.DownloadSpeed = value);

            double resultProgress = await Task.Factory.StartNew(() => SelectedWrapper.StartLoadAndProgressBarReporter(barProgress),
                creationOptions: TaskCreationOptions.LongRunning);
            SelectedWrapper.Progress = resultProgress;

            TorrentState resultState = await Task.Factory.StartNew(
                () => SelectedWrapper.TorrentStateReporter(stateProgress),
            creationOptions: TaskCreationOptions.LongRunning);
            SelectedWrapper.State = resultState;

            string resultProgressString = await Task.Factory.StartNew(
                () => SelectedWrapper.ProgressStringReporter(stringProgress),
                creationOptions: TaskCreationOptions.LongRunning);
            SelectedWrapper.ProgressString = resultProgressString;

            string resultDownloadSpeed = await Task.Factory.StartNew(
                () => SelectedWrapper.DownloadSpeedReporter(downloadSpeedProgress),
                creationOptions: TaskCreationOptions.LongRunning);
            SelectedWrapper.DownloadSpeed = resultDownloadSpeed;

            SelectedWrapper.Manager.TorrentStateChanged += delegate (object o, TorrentStateChangedEventArgs args)
            {
                _engine.DiskManager.Flush(SelectedWrapper.Manager);
                _torrentsHashDictianory[SelectedWrapper.Torrent.InfoHash.ToString()] = (_torrentsHashDictianory[SelectedWrapper.Torrent.InfoHash.ToString()].Item1, args.NewState.ToString());
            };
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName]string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        public ApplicationViewModel()
        {
            this._engine = new ClientEngine(new EngineSettings());
            this.TorrentsList = new ObservableCollection<TorrentWrapper>(); ;
            this._torrentsHashDictianory = new Dictionary<string, (string, string)>();
        }
    }
}
