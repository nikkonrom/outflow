namespace nikkonrom.Outflow.Repositories.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.TorrentInfoes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        InfoHash = c.String(),
                        SavePath = c.String(),
                        LastState = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.TorrentInfoes");
        }
    }
}
