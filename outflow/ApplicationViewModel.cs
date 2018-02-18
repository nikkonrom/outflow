using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using MonoTorrent.Client;

namespace Outflow
{
    class ApplicationViewModel : INotifyPropertyChanged
    {
        private TorrentWrapper selectedWrapper;
        public ObservableCollection<TorrentWrapper> TorrentsList { get; set; }
        private ClientEngine engine;
        private Dictionary<string, (string, string)> torrentsHashDictianory;

        public TorrentWrapper SelectedWrapper
        {
            get => selectedWrapper;
            set
            {
                selectedWrapper = value;
                OnPropertyChanged("SelectedWrapper");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName]string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        public ApplicationViewModel()
        {
            this.engine = new ClientEngine(new EngineSettings());
            this.TorrentsList = new ObservableCollection<TorrentWrapper>(); ;
            this.torrentsHashDictianory = new Dictionary<string, (string, string)>();
        }
    }
}
