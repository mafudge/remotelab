namespace RemoteLab.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class PoolChanges : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Pools", "RemoteAdminUser", c => c.String(nullable: false));
            AddColumn("dbo.Pools", "RemoteAdminPassword", c => c.String(nullable: false));
            AddColumn("dbo.Pools", "WelcomeMessage", c => c.String(nullable: false));
            DropColumn("dbo.Pools", "Logo");
            Sql(@"alter view dbo.PoolSummary
                                as
                               select p.PoolName, p.ActiveDirectoryUserGroup, p.ActiveDirectoryAdminGroup,
                                    p.EmailNotifyList, p.RdpTcpPort, p.CleanupInMinutes,
                                    p.RemoteAdminUser, p.RemoteAdminPassword, p.WelcomeMessage,
            	                    count(*) as PoolCount, 
            	                    count(c.UserName) as PoolInUse,
            	                    count(*) - count(c.UserName) as PoolAvailable
            	                    from Pools p join Computers c on c.Pool_PoolName=p.PoolName
            	                    group by p.PoolName, p.ActiveDirectoryUserGroup, p.ActiveDirectoryAdminGroup,
                                    p.EmailNotifyList, p.RdpTcpPort, p.CleanupInMinutes,
                                    p.RemoteAdminUser, p.RemoteAdminPassword, p.WelcomeMessage");

        }
        
        public override void Down()
        {
            AddColumn("dbo.Pools", "Logo", c => c.String(maxLength: 200));
            DropColumn("dbo.Pools", "WelcomeMessage");
            DropColumn("dbo.Pools", "RemoteAdminPassword");
            DropColumn("dbo.Pools", "RemoteAdminUser");
            Sql(@"alter view dbo.PoolSummary
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
    }
}
