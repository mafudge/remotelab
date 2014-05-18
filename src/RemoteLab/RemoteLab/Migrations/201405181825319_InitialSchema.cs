namespace RemoteLab.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialSchema : DbMigration
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
                        Logon = c.DateTime(nullable:true),
                        Pool_PoolName = c.String(maxLength: 50),
                    })
                .PrimaryKey(t => t.ComputerName)
                .ForeignKey("dbo.Pools", t => t.Pool_PoolName)
                .Index(t => t.Pool_PoolName);
            
            CreateTable(
                "dbo.Pools",
                c => new
                    {
                        PoolName = c.String(nullable: false, maxLength: 50),
                        ActiveDirectoryUserGroup = c.String(maxLength: 100),
                        Logo = c.String(maxLength: 200),
                        ActiveDirectoryAdminGroup = c.String(maxLength: 100),
                        EmailNotifyList = c.String(maxLength: 100),
                        RdpTcpPort = c.Int(nullable: false),
                        CleanupInMinutes = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.PoolName);
            
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

//            Sql(@"create view dbo.PoolSummary
//                    as
//                   select p.PoolName, p.ActiveDirectoryUserGroup, p.ActiveDirectoryAdminGroup,
//                        p.Logo, p.EmailNotifyList, p.RdpTcpPort, p.CleanupInMinutes,
//	                    count(*) as PoolCount, 
//	                    count(c.UserName) as PoolInUse,
//	                    count(*) - count(c.UserName) as PoolAvailable
//	                    from Pools p join Computers c on c.Pool_PoolName=p.PoolName
//	                    group by p.PoolName, p.ActiveDirectoryUserGroup, p.ActiveDirectoryAdminGroup,
//                        p.Logo, p.EmailNotifyList, p.RdpTcpPort, p.CleanupInMinutes");
        }
        
        public override void Down()
        {
           // Sql("drop view dbo.PoolSummary");
            DropForeignKey("dbo.Computers", "Pool_PoolName", "dbo.Pools");
            DropIndex("dbo.Computers", new[] { "Pool_PoolName" });
            DropTable("dbo.Events");
            DropTable("dbo.Pools");
            DropTable("dbo.Computers");
        }
    }
}
