namespace RemoteLab.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class PoolSortOrder : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Pools", "SortOrder", c => c.Byte(nullable: true));
            Sql(@"ALTER view [dbo].[PoolSummary]
                                as
                                select p.PoolName, p.ActiveDirectoryUserGroup, p.ActiveDirectoryAdminGroup,
                                    p.EmailNotifyList, p.RdpTcpPort, p.CleanupInMinutes,
                                    p.RemoteAdminUser, p.RemoteAdminPassword, p.WelcomeMessage,
            	                    count(c.ComputerName) as PoolCount, 
            	                    count(c.UserName) as PoolInUse,
            	                    count(c.ComputerName) - count(c.UserName) as PoolAvailable, p.SortOrder

                                    from Pools p left
                                    join Computers c on c.Pool_PoolName = p.PoolName

                                    group by p.PoolName, p.ActiveDirectoryUserGroup, p.ActiveDirectoryAdminGroup,
                                    p.EmailNotifyList, p.RdpTcpPort, p.CleanupInMinutes,
                                    p.RemoteAdminUser, p.RemoteAdminPassword, p.WelcomeMessage, p.SortOrder
                                    order by SortOrder offset 0 rows");
        }
        
        public override void Down()
        {
            DropColumn("dbo.Pools", "SortOrder");
            Sql(@"ALTER view [dbo].[PoolSummary]
                                as
                               select p.PoolName, p.ActiveDirectoryUserGroup, p.ActiveDirectoryAdminGroup,
                                    p.EmailNotifyList, p.RdpTcpPort, p.CleanupInMinutes,
                                    p.RemoteAdminUser, p.RemoteAdminPassword, p.WelcomeMessage,
            	                    count(c.ComputerName) as PoolCount, 
            	                    count(c.UserName) as PoolInUse,
            	                    count(c.ComputerName) - count(c.UserName) as PoolAvailable
            	                    from Pools p left join Computers c on c.Pool_PoolName=p.PoolName
            	                    group by p.PoolName, p.ActiveDirectoryUserGroup, p.ActiveDirectoryAdminGroup,
                                    p.EmailNotifyList, p.RdpTcpPort, p.CleanupInMinutes,
                                    p.RemoteAdminUser, p.RemoteAdminPassword, p.WelcomeMessage");
        }
    }
}
