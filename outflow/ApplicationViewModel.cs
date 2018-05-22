using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Win32;
using MonoTorrent.Client;
using MonoTorrent.Common;
using nikkonrom.DomainModel;
using nikkonrom.Repositories;
using Ninject;

namespace Outflow
{
    internal class ApplicationViewModel : INotifyPropertyChanged
    {
        private const string hashedTorrentsListPath = "storedtorrents\\list.data";
        private const string hashedTorrentsFolderPath = "storedtorrents\\";
        private readonly Dispatcher _dispatcher;
        private readonly ClientEngine _engine;
        private readonly IUnitOfWork _unitOfWork;
        private TorrentWrapper _selectedWrapper;
        private string storedTorrentsPath;

        public ApplicationViewModel()
        {

            _engine = new ClientEngine(new EngineSettings());
            TorrentsList = new ObservableCollection<TorrentWrapper>();
            _unitOfWork = ((App)Application.Current).NinjectKernel.Get<IUnitOfWork>();
            _dispatcher = Dispatcher.CurrentDispatcher;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void AddTorrentOpenFileDialog(object arg)
        {
            var chooseTorrentFileDialog = new OpenFileDialog
            {
                Title = "Open torrent files",
                Filter = "Torrent-files (*.torrent)|*.torrent"
            };

            if (chooseTorrentFileDialog.ShowDialog() == true)
            {
                Directory.CreateDirectory(hashedTorrentsFolderPath);
                storedTorrentsPath = hashedTorrentsFolderPath + chooseTorrentFileDialog.SafeFileName;
                File.Copy(chooseTorrentFileDialog.FileName, storedTorrentsPath);
                var torrent = Torrent.Load(storedTorrentsPath);
                TorrentSettingsDialog(torrent, storedTorrentsPath);
            }
        }

        private void TorrentSettingsDialog(Torrent torrent, string storedTorrentsPath)
        {
            var dialogWIndow =
                new FolderDialogWIndow(torrent.Name)
                {
                    Owner = Application.Current.MainWindow,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner
                };
            dialogWIndow.ShowDialog();
            if (dialogWIndow.DialogResult == true)
            {
                RegisterTorrent(torrent, dialogWIndow.DownloadFolderPath.Text);
                CreateTorrentInfo();
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
            var wrapper = new TorrentWrapper(downloadFolderPath, torrent);
            TorrentsList.Add(wrapper);
            SelectedWrapper = wrapper;
            _engine.Register(SelectedWrapper.Manager);
        }

        private async void CreateTorrentInfo()
        {
            _unitOfWork.GetRepository<TorrentInfo>().Create(new TorrentInfo()
            {
                InfoHash = SelectedWrapper.Manager.InfoHash.ToString(),
                SavePath = SelectedWrapper.Manager.SavePath,
                LastState = TorrentsList.Last().Manager.State.ToString()
            });
            await _unitOfWork.SaveChangesAsync();
        }


        private void StartTorrent(object arg)
        {
            if (SelectedWrapper != null)
                if (SelectedWrapper.Manager.State == TorrentState.Stopped)
                    StartDownload();
                else if (SelectedWrapper.Manager.State == TorrentState.Paused)
                    SelectedWrapper.ResumeTorrent();
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
                var deleteAction = new Action(async () =>
                {
                    _engine.Unregister(SelectedWrapper.Manager);
                    var torrentHashString = SelectedWrapper.Manager.InfoHash.ToString();
                    var searchResult = await _unitOfWork.GetRepository<TorrentInfo>().GetSingleOrDefaultAsync(torrentInfo =>
                        torrentInfo.InfoHash == torrentHashString);
                    _unitOfWork.GetRepository<TorrentInfo>().Delete(searchResult);
                    await _unitOfWork.SaveChangesAsync();
                    SelectedWrapper.Manager.Dispose();
                    SelectedWrapper.DeleteFastResume();
                    File.Delete(SelectedWrapper.Torrent.TorrentPath);
                    TorrentsList.Remove(SelectedWrapper);
                });


                SelectedWrapper.PretendToDelete = true;

                if (SelectedWrapper.State == TorrentState.Stopped)
                {
                    _dispatcher.Invoke(deleteAction);
                }
                else
                {
                    StopCommand.Execute(null);
                    SelectedWrapper.Manager.TorrentStateChanged +=
                        delegate (object sender, TorrentStateChangedEventArgs args)
                        {
                            if (args.NewState == TorrentState.Stopped && SelectedWrapper.PretendToDelete)
                                _dispatcher.Invoke(deleteAction);
                        };
                }
            }
        }

        //private void StoreTorrents()
        //{
        //    if (_torrentsHashDictianory.Count > 0)
        //    {
        //        var formatter = new BinaryFormatter();
        //        using (var fileStream = new FileStream(hashedTorrentsListPath, FileMode.Create))
        //        {
        //            formatter.Serialize(fileStream, _torrentsHashDictianory);
        //        }
        //    }
        //}

        private async void RestoreTorrents()
        {
            var hashedTorrentsFileNames = Directory.GetFiles(hashedTorrentsFolderPath, "*.torrent");
            foreach (var fileName in hashedTorrentsFileNames)
                {
                    var torrent = Torrent.Load(fileName);
                    var torrentHashString = torrent.InfoHash.ToString();
                    var torrenInfo = await _unitOfWork.GetRepository<TorrentInfo>()
                        .GetSingleOrDefaultAsync(torrentInfoPredicate =>
                            torrentInfoPredicate.InfoHash == torrentHashString);

                    RegisterTorrent(torrent, torrenInfo.SavePath);
                    if (torrenInfo.LastState == "Downloading")
                        StartDownload();
                    else if (torrenInfo.LastState == "Paused")
                        TorrentsList.Last().Manager.HashCheck(false);
                }
        }

        private void ExitProgramm(object arg)
        {
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

            var resultProgress = await Task.Factory.StartNew(
                () => SelectedWrapper.StartLoadAndProgressBarReporter(barProgress),
                TaskCreationOptions.LongRunning);
            SelectedWrapper.Progress = resultProgress;

            var resultState = await Task.Factory.StartNew(
                () => SelectedWrapper.TorrentStateReporter(stateProgress),
                TaskCreationOptions.LongRunning);
            SelectedWrapper.State = resultState;

            var resultProgressString = await Task.Factory.StartNew(
                () => SelectedWrapper.ProgressStringReporter(stringProgress),
                TaskCreationOptions.LongRunning);
            SelectedWrapper.ProgressString = resultProgressString;

            var resultDownloadSpeed = await Task.Factory.StartNew(
                () => SelectedWrapper.DownloadSpeedReporter(downloadSpeedProgress),
                TaskCreationOptions.LongRunning);
            SelectedWrapper.DownloadSpeed = resultDownloadSpeed;

            SelectedWrapper.Manager.TorrentStateChanged += async delegate (object o, TorrentStateChangedEventArgs args)
            {
                _engine.DiskManager.Flush(SelectedWrapper.Manager);
                var torrentHashString = SelectedWrapper.Torrent.InfoHash.ToString();
                var torrentInfo = await _unitOfWork.GetRepository<TorrentInfo>().GetSingleOrDefaultAsync(torrentInfoPredicate =>
                    torrentInfoPredicate.InfoHash == torrentHashString);
                torrentInfo.LastState = args.NewState.ToString();
                _unitOfWork.GetRepository<TorrentInfo>().Update(torrentInfo);
                await _unitOfWork.SaveChangesAsync();
            };
        }

        private void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

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
                OnPropertyChanged();
            }
        }

        public ObservableCollection<TorrentWrapper> TorrentsList { get; set; }

        public RelayCommand AddCommand => _addCommand ?? (_addCommand = new RelayCommand(AddTorrentOpenFileDialog));
        public RelayCommand StartCommand => _startCommand ?? (_startCommand = new RelayCommand(StartTorrent));
        public RelayCommand PauseCommand => _pauseCommand ?? (_pauseCommand = new RelayCommand(PauseTorrent));
        public RelayCommand StopCommand => _stopCommand ?? (_stopCommand = new RelayCommand(StopTorrent));
        public RelayCommand DeleteCommand => _deleteTorrent ?? (_deleteTorrent = new RelayCommand(DeleteTorrent));
        public RelayCommand ExitCommand => _exitCommand ?? (_exitCommand = new RelayCommand(ExitProgramm));
        public RelayCommand OpenCommand => _openCommand ?? (_openCommand = new RelayCommand(OpenProgramm));

        #endregion
    }
}