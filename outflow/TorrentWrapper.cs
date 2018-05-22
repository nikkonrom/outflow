using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using MonoTorrent.BEncoding;
using MonoTorrent.Client;
using MonoTorrent.Common;

namespace Outflow
{
    public class TorrentWrapper : INotifyPropertyChanged
    {
        private readonly string _fastResumePath;
        private string downloadSpeed;
        public bool PretendToDelete;

        private double progress;
        private string progressString;
        private TorrentState state;


        public TorrentWrapper(string downloadFolderPath, Torrent torrent)
        {
            Torrent = torrent;
            PretendToDelete = false;
            Size = TorrentConverter.ConvertBytesSize(Torrent.Size);
            Manager = new TorrentManager(Torrent, downloadFolderPath, new TorrentSettings());
            _fastResumePath = $"resume\\{Torrent.InfoHash}";
        }

        public Torrent Torrent { get; }
        public TorrentManager Manager { get; }

        public double Progress
        {
            get => progress;
            set => SetField(ref progress, value, "Progress");
        }

        public string ProgressString
        {
            get => string.Concat(Convert.ToString(Math.Round(Progress, 2)), "%");
            set => SetField(ref progressString, value, "ProgressString");
        }

        public TorrentState State
        {
            get => state;
            set => SetField(ref state, value, "State");
        }

        public string DownloadSpeed
        {
            get => downloadSpeed;
            set => SetField(ref downloadSpeed, value, "DownloadSpeed");
        }

        public string Size { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public double StartLoadAndProgressBarReporter(IProgress<double> progress)
        {
            ResumeTorrent();
            Manager.PieceHashed += delegate { progress.Report(Manager.Progress); };
            return Manager.Progress;
        }

        public TorrentState TorrentStateReporter(IProgress<TorrentState> progress)
        {
            Manager.TorrentStateChanged += delegate { progress.Report(Manager.State); };
            return Manager.State;
        }

        public string ProgressStringReporter(IProgress<string> progress)
        {
            Manager.PieceHashed += delegate { progress.Report(ProgressString); };
            return ProgressString;
        }

        public string DownloadSpeedReporter(IProgress<string> progress)
        {
            Manager.PieceHashed += delegate
            {
                progress.Report(TorrentConverter.ConvertBytesSpeed(Manager.Monitor.DownloadSpeed));
            };

            Manager.TorrentStateChanged += delegate
            {
                progress.Report(TorrentConverter.ConvertBytesSpeed(Manager.Monitor.DownloadSpeed));
            };

            return TorrentConverter.ConvertBytesSpeed(Manager.Monitor.DownloadSpeed);
        }

        private void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void SetField<T>(ref T field, T value, string propertyName)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return;
            field = value;
            OnPropertyChanged(propertyName);
        }

        public void PauseTorrent()
        {
            Manager.Pause();
            var list = new BEncodedList();
            var data = Manager.SaveFastResume();
            var fastResume = data.Encode();
            list.Add(fastResume);
            var file = new FileInfo(_fastResumePath);
            file.Directory?.Create();
            File.WriteAllBytes(file.FullName, list.Encode());
        }

        public void ResumeTorrent()
        {
            if (Manager.State == TorrentState.Stopped)
                if (File.Exists(_fastResumePath))
                {
                    var list = (BEncodedList) BEncodedValue.Decode(File.ReadAllBytes(_fastResumePath));
                    foreach (var bEncodedValue in list)
                    {
                        var fastResume = (BEncodedDictionary) bEncodedValue;
                        var data = new FastResume(fastResume);
                        if (Manager.InfoHash == data.Infohash)
                        {
                            Manager.LoadFastResume(data);
                            File.Delete(_fastResumePath);
                            break;
                        }
                    }
                }

            Manager.Start();
        }

        public void DeleteFastResume()
        {
            if (File.Exists(_fastResumePath))
                File.Delete(_fastResumePath);
        }
    }
}