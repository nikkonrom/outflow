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
        public readonly Torrent torrent;
        public readonly TorrentManager manager;
        public readonly string downloadFolderPath;

        public TorrentWrapper(string downloadFolderPath, Torrent torrent)
        {
            this.torrent = torrent;
            this.downloadFolderPath = downloadFolderPath;
            this.manager = new TorrentManager(this.torrent, this.downloadFolderPath, new TorrentSettings());
        }

    }
}
