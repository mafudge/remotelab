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
                        Reserved = c.DateTime(nullable: true),
                        Logon = c.DateTime(nullable: true),
                        Pool = c.String(maxLength: 50),
                    })
                .PrimaryKey(t => t.ComputerName);

            //AlterColumn("dbo.Computers", "Reserved", c => c.DateTime());
            //AlterColumn("dbo.Computers", "Logon", c => c.DateTime());
            
            CreateTable(
                "dbo.Events",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        EventName = c.String(nullable: false, maxLength: 50),
                        UserName = c.String(nullable: false, maxLength: 50),
                        ComputerName = c.String(nullable: false, maxLength: 50),
                       // PoolName = c.String(nullable: true, maxLength: 50),
                        DtStamp = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
        }
        
        public override void Down()
        {
            DropTable("dbo.Computers");
            DropTable("dbo.Events");
        }
    }
}
