using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonoTorrent.BEncoding;
using MonoTorrent.Client;
using MonoTorrent.Common;


namespace Outflow
{
    public class TorrentWrapper : INotifyPropertyChanged
    {
        private readonly string _fastResumePath;
        public bool PretendToDelete;

        public Torrent Torrent { get; }
        public TorrentManager Manager { get; }

        public event PropertyChangedEventHandler PropertyChanged;

        public double StartLoadAndProgressBarReporter(IProgress<double> progress)
        {
            ResumeTorrent();
            Manager.PieceHashed += delegate
            {
                    progress.Report(Manager.Progress);
                };
            return Manager.Progress;
        }

        public TorrentState TorrentStateReporter(IProgress<TorrentState> progress)
        {
            Manager.TorrentStateChanged += delegate (object sender, TorrentStateChangedEventArgs args)
            {
                progress.Report(Manager.State);
            };
            return Manager.State;
        }

        public string ProgressStringReporter(IProgress<string> progress)
        {
            Manager.PieceHashed += delegate (object sender, PieceHashedEventArgs args)
            {
                progress.Report(ProgressString);
            };
            return ProgressString;
        }

        public string DownloadSpeedReporter(IProgress<string> progress)
        {

            Manager.PieceHashed += delegate (object sender, PieceHashedEventArgs args)
            {
                progress.Report(TorrentConverter.ConvertBytesSpeed(Manager.Monitor.DownloadSpeed));
            };

            Manager.TorrentStateChanged += delegate (object sender, TorrentStateChangedEventArgs args)
            {
                progress.Report(TorrentConverter.ConvertBytesSpeed(Manager.Monitor.DownloadSpeed));
            };

            return TorrentConverter.ConvertBytesSpeed(Manager.Monitor.DownloadSpeed);
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void SetField<T>(ref T field, T value, string propertyName)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return;
            field = value;
            OnPropertyChanged(propertyName);
        }

        private double progress;
        private string progressString;
        private TorrentState state;
        private string downloadSpeed;

        public double Progress
        {
            get => progress;
            set => SetField(ref progress, value, "Progress");
        }

        public string ProgressString
        {
            get => String.Concat(Convert.ToString(Math.Round(Progress, 2)), "%");
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


        public TorrentWrapper(string downloadFolderPath, Torrent torrent)
        {
            this.Torrent = torrent;
            PretendToDelete = false;
            this.Size = TorrentConverter.ConvertBytesSize(Torrent.Size);
            this.Manager = new TorrentManager(this.Torrent, downloadFolderPath, new TorrentSettings());
            this._fastResumePath = $"resume\\{Torrent.InfoHash}";
        }

        public void PauseTorrent()
        {
            Manager.Pause();
            BEncodedList list = new BEncodedList();
            FastResume data = Manager.SaveFastResume();
            BEncodedDictionary fastResume = data.Encode();
            list.Add(fastResume);
            FileInfo file = new FileInfo(_fastResumePath);
            file.Directory?.Create();
            File.WriteAllBytes(file.FullName, list.Encode());
        }

        public void ResumeTorrent()
        {
            if (Manager.State == TorrentState.Stopped)
            {
                if (File.Exists(_fastResumePath))
                {
                    BEncodedList list = (BEncodedList)BEncodedValue.Decode(File.ReadAllBytes(_fastResumePath));
                    foreach (var bEncodedValue in list)
                    {
                        var fastResume = (BEncodedDictionary)bEncodedValue;
                        FastResume data = new FastResume(fastResume);
                        if (Manager.InfoHash == data.Infohash)
                        {
                            Manager.LoadFastResume(data);
                            File.Delete(_fastResumePath);
                            break;
                        }
                            
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


