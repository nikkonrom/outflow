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
                    StartDownload(TorrentsList.Last());
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
