using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonoTorrent.Client;
using MonoTorrent.Common;


namespace Outflow
{
    public class TorrentWrapper
    {
        public Torrent Torrent { get; }
        public TorrentManager Manager { get; }
        
        public TorrentWrapper(string downloadFolderPath, Torrent torrent)
        {
            this.Torrent = torrent;
            this.Manager = new TorrentManager(this.Torrent, downloadFolderPath, new TorrentSettings());
            
        }

    }
}
