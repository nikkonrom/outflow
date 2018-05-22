using System.Data.Entity;
using nikkonrom.DomainModel;
using nikkonrom.Outflow.Repositories.Migrations;

namespace nikkonrom.Outflow.Repositories
{
    public class OutflowDbContext : DbContext
    {
        private const string ConnectionName = "DefaultConnection";

        public DbSet<TorrentInfo> TorrentInfos { get; set; }


        static OutflowDbContext()
        {
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<OutflowDbContext, Configuration>());
        }


        public OutflowDbContext() : base(ConnectionName)
        {
            
        }
    }
}
