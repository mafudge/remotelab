namespace RemoteLab.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class PoolSupport : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Pools",
                c => new
                    {
                        PoolName = c.String(nullable: false, maxLength: 50),
                        Logo = c.String(maxLength: 200),
                        ActiveDirectoryUserGroup = c.String(maxLength: 100),
                        ActiveDirectoryAdminGroup = c.String(maxLength: 100),
                        EmailNotifyList = c.String(maxLength: 100),
                        RdpTcpPort = c.Int(nullable: false, defaultValue: 3389),
                        CleanupInMinutes = c.Int(nullable: false, defaultValue: 30),
                    })
                .PrimaryKey(t => t.PoolName);
            
            AddColumn("dbo.Computers", "Pool_PoolName", c => c.String(maxLength: 50));
            CreateIndex("dbo.Computers", "Pool_PoolName");
            AddForeignKey("dbo.Computers", "Pool_PoolName", "dbo.Pools", "PoolName");
            DropColumn("dbo.Computers", "Pool");
            Sql(@"create view dbo.PoolSummary
                    as
                    select p.PoolName,
	                    count(*) as PoolCount, 
	                    count(c.UserName) as PoolInUse,
	                    count(*) - count(c.UserName) as PoolAvailable
	                    from Pools p join Computers c on c.Pool_PoolName=p.PoolName
	                    group by p.PoolName");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Computers", "Pool", c => c.String(maxLength: 50));
            DropForeignKey("dbo.Computers", "Pool_PoolName", "dbo.Pools");
            DropIndex("dbo.Computers", new[] { "Pool_PoolName" });
            DropColumn("dbo.Computers", "Pool_PoolName");
            DropTable("dbo.Pools");
            Sql("drop view dbo.PoolSummary");
        }
    }
}
