using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
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
        private Dispatcher _dispatcher;

        private const string hashedTorrentsListPath = "storedtorrents\\list.data";
        private const string hashedTorrentsFolderPath = "storedtorrents\\";
        private string storedTorrentsPath;

        #region Commands

        private RelayCommand _addCommand;
        private RelayCommand _startCommand;
        private RelayCommand _pauseCommand;
        private RelayCommand _stopCommand;
        private RelayCommand _deleteTorrent;
        private RelayCommand _exitCommand;
        private RelayCommand _openCommand;

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

        public RelayCommand AddCommand => _addCommand ?? (_addCommand = new RelayCommand(AddTorrentOpenFIleDialog));
        public RelayCommand StartCommand => _startCommand ?? (_startCommand = new RelayCommand(StartTorrent));
        public RelayCommand PauseCommand => _pauseCommand ?? (_pauseCommand = new RelayCommand(PauseTorrent));
        public RelayCommand StopCommand => _stopCommand ?? (_stopCommand = new RelayCommand(StopTorrent));
        public RelayCommand DeleteCommand => _deleteTorrent ?? (_deleteTorrent = new RelayCommand(DeleteTorrent));
        public RelayCommand ExitCommand => _exitCommand ?? (_exitCommand = new RelayCommand(ExitProgramm));
        public RelayCommand OpenCommand => _openCommand ?? (_openCommand = new RelayCommand());
        #endregion

        private void AddTorrentOpenFIleDialog(object arg)
        {
            OpenFileDialog chooseTorrentFileDialog = new OpenFileDialog();
            chooseTorrentFileDialog.Title = "Open torrent files";
            chooseTorrentFileDialog.Filter = "Torrent-files (*.torrent)|*.torrent";

            if (chooseTorrentFileDialog.ShowDialog() == true)
            {
                Directory.CreateDirectory(hashedTorrentsFolderPath);
                storedTorrentsPath = hashedTorrentsFolderPath + chooseTorrentFileDialog.SafeFileName;
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

        private void StartTorrent(object arg)
        {
            if (SelectedWrapper != null)
            {

                if (SelectedWrapper.Manager.State == TorrentState.Stopped)
                {
                    StartDownload();
                }
                else if (SelectedWrapper.Manager.State == TorrentState.Paused)
                {
                    SelectedWrapper.ResumeTorrent();
                }
            }
        }

        private void PauseTorrent(object arg)
        {
            if (SelectedWrapper?.Manager.State == TorrentState.Downloading)
                SelectedWrapper.PauseTorrent();
        }

        private void StopTorrent(object arg)
        {
            SelectedWrapper?.Manager.Stop();
        }

        private void DeleteTorrent(object arg)
        {
            if (SelectedWrapper != null)
            {
                var deleteAction = new Action(() =>
                {
                    _engine.Unregister(SelectedWrapper.Manager);
                    _torrentsHashDictianory.Remove(SelectedWrapper.Manager.InfoHash.ToString());
                    SelectedWrapper.Manager.Dispose();
                    SelectedWrapper.DeleteFastResume();
                    TorrentsList.Remove(SelectedWrapper);
                    if (storedTorrentsPath != null)
                        File.Delete(storedTorrentsPath);
                });


                SelectedWrapper.PretendToDelete = true;

                if (SelectedWrapper.State == TorrentState.Stopped)
                    _dispatcher.Invoke(deleteAction);
                else
                {
                    StopCommand.Execute(null);
                    SelectedWrapper.Manager.TorrentStateChanged +=
                        delegate (object sender, TorrentStateChangedEventArgs args)
                        {
                            if (args.NewState == TorrentState.Stopped && SelectedWrapper.PretendToDelete)
                            {
                                _dispatcher.Invoke(deleteAction);

                            }
                        };
                }
                

                
            }
        }

        private void StoreTorrents()
        {
            if (_torrentsHashDictianory.Count > 0)
            {
                BinaryFormatter formatter = new BinaryFormatter();
                using (FileStream fileStream = new FileStream(hashedTorrentsListPath, FileMode.Create))
                {
                    formatter.Serialize(fileStream, _torrentsHashDictianory);
                }
            }
        }

        public void RestoreTorrents()
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
                        RegisterTorrent(torrent, torrentHashInfo.Item1);
                        if (torrentHashInfo.Item2 == "Downloading")
                        {
                            StartDownload();
                        }
                        else if (torrentHashInfo.Item2 == "Paused")
                        {
                            TorrentsList.Last().Manager.HashCheck(false);
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

        private void ExitProgramm(object arg)
        {
            StoreTorrents();
            (arg as Window)?.Close();
        }

        private void OpenProgramm(object arg)
        {
            RestoreTorrents();
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

        private void OnPropertyChanged([CallerMemberName]string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        public ApplicationViewModel()
        {
            this._engine = new ClientEngine(new EngineSettings());
            this.TorrentsList = new ObservableCollection<TorrentWrapper>(); ;
            this._torrentsHashDictianory = new Dictionary<string, (string, string)>();
            this._dispatcher = Dispatcher.CurrentDispatcher;
        }
    }
}
