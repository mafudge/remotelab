namespace RemoteLab.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class wtf : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Pools",
                c => new
                    {
                        PoolName = c.String(nullable: false, maxLength: 50),
                        ActiveDirectoryUserGroup = c.String(),
                        Logo = c.String(maxLength: 200),
                        ActiveDirectoryAdminGroup = c.String(maxLength: 100),
                        EmailNotifyList = c.String(maxLength: 100),
                        RdpTcpPort = c.Int(nullable: false),
                        CleanupInMinutes = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.PoolName);
            
            AddColumn("dbo.Computers", "Pool_PoolName", c => c.String(maxLength: 50));
            AlterColumn("dbo.Computers", "Reserved", c => c.DateTime());
            AlterColumn("dbo.Computers", "Logon", c => c.DateTime());
            CreateIndex("dbo.Computers", "Pool_PoolName");
            AddForeignKey("dbo.Computers", "Pool_PoolName", "dbo.Pools", "PoolName");
            DropColumn("dbo.Computers", "Pool");

            Sql(@"create view dbo.PoolSummary
                    as
                   select p.PoolName, p.ActiveDirectoryUserGroup, p.ActiveDirectoryAdminGroup,
                        p.Logo, p.EmailNotifyList, p.RdpTcpPort, p.CleanupInMinutes,
	                    count(*) as PoolCount, 
	                    count(c.UserName) as PoolInUse,
	                    count(*) - count(c.UserName) as PoolAvailable
	                    from Pools p join Computers c on c.Pool_PoolName=p.PoolName
	                    group by p.PoolName, p.ActiveDirectoryUserGroup, p.ActiveDirectoryAdminGroup,
                        p.Logo, p.EmailNotifyList, p.RdpTcpPort, p.CleanupInMinutes");

        }
        
        public override void Down()
        {
            AddColumn("dbo.Computers", "Pool", c => c.String(maxLength: 50));
            DropForeignKey("dbo.Computers", "Pool_PoolName", "dbo.Pools");
            DropIndex("dbo.Computers", new[] { "Pool_PoolName" });
            AlterColumn("dbo.Computers", "Logon", c => c.DateTime(nullable: false));
            AlterColumn("dbo.Computers", "Reserved", c => c.DateTime(nullable: false));
            DropColumn("dbo.Computers", "Pool_PoolName");
            DropTable("dbo.Pools");
            Sql("drop view dbo.PoolSummary");

        }
    }
}
