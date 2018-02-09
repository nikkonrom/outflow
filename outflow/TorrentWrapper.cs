using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonoTorrent.Client;
using MonoTorrent.Common;


namespace Outflow
{
    public class TorrentWrapper : INotifyPropertyChanged
    {
        public Torrent Torrent { get; }
        public TorrentManager Manager { get; }

        public event PropertyChangedEventHandler PropertyChanged;

        public double Download(IProgress<double> progress)
        {
            Manager.Start();
            Manager.PieceHashed += delegate (object sender, PieceHashedEventArgs args)
                {
                    progress.Report(Manager.Progress);
                };
            return Manager.Progress;
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetField<T>(ref T field, T value, string propertyName)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        private double progress;
        //private 

        public double Progress
        {
            get => progress;
            set => SetField(ref progress, value, "Progress");
        }


        public TorrentWrapper(string downloadFolderPath, Torrent torrent)
        {
            this.Torrent = torrent;
            this.Manager = new TorrentManager(this.Torrent, downloadFolderPath, new TorrentSettings());
        }

    }
}
