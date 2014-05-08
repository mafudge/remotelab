namespace RemoteLab.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class StoredProcs : DbMigration
    {
        public override void Up()
        {
            Sql(@"CREATE PROCEDURE [dbo].[P_remotelabdb_startup] 
	            -- Add the parameters for the stored procedure here
                	@computer varchar(50)
            AS
            BEGIN
	            -- SET NOCOUNT ON added to prevent extra result sets from
	            -- interfering with SELECT statements.
	            SET NOCOUNT ON;
	
                -- Insert statements for procedure here
	            If exists(select * from remotelab_computers where computername = @computer)
	            Begin	
		            Update remotelab_computers set 
		            netid = null,
		            logon = null,
		            reserved = null
		            where computername = @computer
	            End
	            Else
	            Begin
		            Insert into remotelab_computers (computername,netid, reserved, logon) values (@computer,null, null, null)
	            End

	            -- log the event
	            declare @now datetime
	            set @now = getdate()
	            exec dbo.[P_remotelabdb_logevent] 'STARTUP','N/A', @computer, @now 
	
            END");

            Sql(@"CREATE PROCEDURE [dbo].[P_remotelabdb_shutdown] 
	                -- Add the parameters for the stored procedure here
	                @computer varchar(50)
                AS
                BEGIN
	                -- SET NOCOUNT ON added to prevent extra result sets from
	                -- interfering with SELECT statements.
	                SET NOCOUNT ON;

	                declare @netid varchar(50)
	                set @netid = (SELECT netid from remotelab_computers where computername = @computer)
	                if (@netid is null) set @netid = 'N/A'
	
                    -- Insert statements for procedure here
	                Update remotelab_computers set 
		                netid = 'poweredoff',
		                logon = null,
		                reserved = null
	                where computername = @computer
		
	                -- log the event
	                declare @now datetime
	                set @now = getdate()
	                exec dbo.[P_remotelabdb_logevent] 'SHUTDOWN',@netid, @computer, @now 
                END");

            Sql(@"CREATE PROCEDURE [dbo].[P_remotelabdb_reserve] 
	                -- Add the parameters for the stored procedure here
	                @computer varchar(50),
	                @netid varchar(50)
                AS
                BEGIN
	                -- SET NOCOUNT ON added to prevent extra result sets from
	                -- interfering with SELECT statements.
	                SET NOCOUNT ON;
	
                    -- Insert statements for procedure here
	                Update remotelab_computers set 
		                netid = @netid,
		                reserved = getdate()
		                where computername = @computer

	                -- log the event
	                declare @now datetime
	                set @now = getdate()
	                exec dbo.[P_remotelabdb_logevent] 'RESERVE',@netid, @computer, @now 
                END");

            Sql(@"CREATE PROCEDURE [dbo].[P_remotelabdb_logon] 
	                -- Add the parameters for the stored procedure here
	                @computer varchar(50),
	                @netid varchar(50)
                AS
                BEGIN
	                -- SET NOCOUNT ON added to prevent extra result sets from
	                -- interfering with SELECT statements.
	                SET NOCOUNT ON;
	
                    -- Insert statements for procedure here
	                Update remotelab_computers set 
		                netid = @netid,
		                logon = getdate()
		                where computername = @computer

	                -- log the event
	                declare @now datetime
	                set @now = getdate()
	                exec dbo.[P_remotelabdb_logevent] 'LOGON',@netid, @computer, @now 
                END");

            Sql(@"CREATE PROCEDURE [dbo].[P_remotelabdb_logoff] 
	                -- Add the parameters for the stored procedure here
	                @computer varchar(50)	
                AS
                BEGIN
	                -- SET NOCOUNT ON added to prevent extra result sets from
	                -- interfering with SELECT statements.
	                SET NOCOUNT ON;

	                declare @netid varchar(50)
	                set @netid = (SELECT netid from remotelab_computers where computername = @computer)
	                if (@netid is null) set @netid = 'N/A'
	
                    -- Insert statements for procedure here
	                Update remotelab_computers set 
		                netid = null,
		                reserved = null,
		                logon = null
	                where computername = @computer


	                -- log the event
	                declare @now datetime
	                set @now = getdate()
	                exec dbo.[P_remotelabdb_logevent] 'LOGOFF',@netid, @computer, @now 
                END");
            Sql(@"CREATE PROCEDURE [dbo].[P_remotelabdb_logevent]
	                -- Add the parameters for the stored procedure here
	                @event varchar(50),
	                @netid varchar(50),
	                @compname varchar(50),
	                @dtstamp datetime
                AS
                BEGIN
	                -- SET NOCOUNT ON added to prevent extra result sets from
	                -- interfering with SELECT statements.
	                SET NOCOUNT ON;

                    -- Insert statements for procedure here
	                insert into dbo.remotelab_eventlog (eventname, netid, computername, dtstamp) 
	                values (@event, @netid, @compname, @dtstamp); 

                END");

            Sql(@"create PROCEDURE [dbo].[P_remotelabdb_fail_tcp_check] 
	                -- Add the parameters for the stored procedure here
	                @computer varchar(50),
	                @netid varchar(50)
                AS
                BEGIN
	                -- SET NOCOUNT ON added to prevent extra result sets from
	                -- interfering with SELECT statements.
	                SET NOCOUNT ON;

                    -- Insert statements for procedure here
	                Update remotelab_computers set 
		                netid = 'rdp check',
		                reserved = null
		                where computername = @computer

	                -- log the event
	                declare @now datetime
	                set @now = getdate()
	                exec dbo.[P_remotelabdb_logevent] 'FAIL RDP CHECK',@netid, @computer, @now 
                END");
            Sql(@"CREATE PROCEDURE [dbo].[P_remotelabdb_clear_reservation] 
	                -- Add the parameters for the stored procedure here
	                @computer varchar(50)
                AS
                BEGIN
	                -- SET NOCOUNT ON added to prevent extra result sets from
	                -- interfering with SELECT statements.
	                SET NOCOUNT ON;
		                declare @netid varchar(50)
	                set @netid = (SELECT netid from remotelab_computers where computername = @computer)
	                if @netid is null return 

                    -- Insert statements for procedure here
	                Update remotelab_computers set 
		                netid = 'cleared',
		                reserved = null
		                where computername = @computer

	                -- log the event
	                declare @now datetime
	                set @now = getdate()
	                exec dbo.[P_remotelabdb_logevent] 'CLEAR RESERVATION',@netid, @computer, @now 
                END");
            Sql(@"CREATE procedure [dbo].[P_remotelabdb_reservation_cleanup] (
	                @minutes int
                )
                as
                begin
	                declare @now datetime
	                declare @compname varchar(50)
	                declare @netid varchar(50)
	                declare cleanupCursor cursor for
		                select computername, netid
			                from dbo.remotelab_computers
				                where (reserved IS NOT NULL) AND (logon IS NULL) AND (netid IS NOT NULL)
				                AND DATEDIFF(mi,reserved,GETDATE())>=@minutes

	                open cleanupCursor
		                fetch next from cleanupCursor into @compname, @netid

	                while @@FETCH_STATUS= 0
	                begin
		                update remotelab_computers set
			                netid = null,
			                logon = null,
			                reserved = null
		                where computername = @compname
		                set @now = getdate()
		                exec dbo.P_remotelabdb_logevent 'EXPIRED RESERVATION', @netid, @compname,@now
		                fetch next from cleanupCursor into @compname, @netid
	                end

	                close cleanupCursor
	                deallocate cleanupCursor
                end ");

        }
        
        public override void Down()
        {
            Sql(@"DROP PROCEDURE [dbo].[P_remotelabdb_startup]");
            Sql(@"DROP PROCEDURE [dbo].[P_remotelabdb_shutdown]");
            Sql(@"DROP PROCEDURE [dbo].[P_remotelabdb_reserve]");
            Sql(@"DROP PROCEDURE [dbo].[P_remotelabdb_logon]");
            Sql(@"DROP PROCEDURE [dbo].[P_remotelabdb_logoff]");
            Sql(@"DROP PROCEDURE [dbo].[P_remotelabdb_logevent]");
            Sql(@"DROP PROCEDURE [dbo].[P_remotelabdb_fail_tcp_check]");
            Sql(@"DROP PROCEDURE [dbo].[P_remotelabdb_clear_reservation]");
            Sql(@"DROP PROCEDURE [dbo].[P_remotelabdb_reservation_cleanup]");

        }
    }
}
