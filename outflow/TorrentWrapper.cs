using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonoTorrent.Client;
using MonoTorrent.Common;


namespace Outflow
{
    internal class TorrentWrapper
    {
        public Torrent Torrent { get; }
        public TorrentManager Manager { get; }
        public string FilePath { get; }
        public string DownloadFolderPath { get; }

        public TorrentWrapper(string filePath)
        {
            this.FilePath = filePath;
            this.Torrent = Torrent.Load(filePath);
        }

    }
}
