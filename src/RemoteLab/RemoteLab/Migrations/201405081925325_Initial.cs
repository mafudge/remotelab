namespace RemoteLab.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Initial : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Computers",
                c => new
                    {
                        ComputerName = c.String(nullable: false, maxLength: 50),
                        UserName = c.String(maxLength: 50),
                        Reserved = c.DateTime(nullable: false),
                        Logon = c.DateTime(nullable: false),
                        Pool = c.String(maxLength: 50),
                    })
                .PrimaryKey(t => t.ComputerName);
            
            CreateTable(
                "dbo.Events",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        EventName = c.String(nullable: false, maxLength: 50),
                        UserName = c.String(nullable: false, maxLength: 50),
                        ComputerName = c.String(nullable: false, maxLength: 50),
                        DtStamp = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.Events");
            DropTable("dbo.Computers");
        }
    }
}
