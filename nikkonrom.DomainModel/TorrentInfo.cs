namespace nikkonrom.DomainModel
{
    public class TorrentInfo
    {
        public int Id { get; set; }

        public string InfoHash { get; set; }

        public string SavePath { get; set; }

        public string LastState { get; set; }
    }
}