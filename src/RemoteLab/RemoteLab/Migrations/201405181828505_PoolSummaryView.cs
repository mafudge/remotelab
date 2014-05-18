namespace RemoteLab.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class PoolSummaryView : DbMigration
    {
        public override void Up()
        {
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
            Sql("drop view dbo.PoolSummary");
        }
        
    }
}
