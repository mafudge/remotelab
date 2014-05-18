namespace RemoteLab.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class StoredProcs : DbMigration
    {
        public override void Up()
        {
            Sql(@"CREATE PROCEDURE [dbo].[P_remotelabdb_startup] 
                	@computer varchar(50)
            AS
            BEGIN
	            -- SET NOCOUNT ON added to prevent extra result sets from
	            -- interfering with SELECT statements.
	            SET NOCOUNT ON;
	
                -- Insert statements for procedure here
	            If exists(select * from Computers where ComputerName = @computer)
	            Begin	
		            Update Computers set 
		            UserName= null,
		            Logon = null,
		            Reserved = null
		            where ComputerName= @computer
	            End
	            Else
	            Begin
		            Insert into Computers (ComputerName, UserName, Reserved, Logon) values (@computer,null, null, null)
	            End

	            -- log the event
	            declare @now datetime
	            set @now = getdate()
	            exec dbo.[P_remotelabdb_logevent] 'STARTUP','N/A', @computer, @now 
	
            END");

            Sql(@"CREATE PROCEDURE [dbo].[P_remotelabdb_shutdown] 
	                @computer varchar(50)
                AS
                BEGIN
	                -- SET NOCOUNT ON added to prevent extra result sets from
	                -- interfering with SELECT statements.
	                SET NOCOUNT ON;

	                declare @username varchar(50)
	                set @username = (SELECT UserName from Computers where ComputerName= @computer)
	                if (@username is null) set @username = 'N/A'
	
                    -- Insert statements for procedure here
	                Update Computers set 
		                UserName = 'poweredoff',
		                Logon = null,
		                Reserved = null
	                where ComputerName= @computer
		
	                -- log the event
	                declare @now datetime
	                set @now = getdate()
	                exec dbo.[P_remotelabdb_logevent] 'SHUTDOWN',@username, @computer, @now 
                END");

            Sql(@"CREATE PROCEDURE [dbo].[P_remotelabdb_reserve] 
	                @computer varchar(50),
	                @username varchar(50)
                AS
                BEGIN
	                -- SET NOCOUNT ON added to prevent extra result sets from
	                -- interfering with SELECT statements.
	                SET NOCOUNT ON;
	
                    -- Insert statements for procedure here
	                Update Computers set 
		                UserName = @username,
		                Reserved = getdate()
		                where ComputerName = @computer

	                -- log the event
	                declare @now datetime
	                set @now = getdate()
	                exec dbo.[P_remotelabdb_logevent] 'RESERVE',@username, @computer, @now 
                END");

            Sql(@"CREATE PROCEDURE [dbo].[P_remotelabdb_logon] 
	                @computer varchar(50),
	                @username varchar(50)
                AS
                BEGIN
	                -- SET NOCOUNT ON added to prevent extra result sets from
	                -- interfering with SELECT statements.
	                SET NOCOUNT ON;
	
                    -- Insert statements for procedure here
	                Update Computers set 
		                UserName = @username,
		                Logon = getdate()
		                where ComputerName = @computer

	                -- log the event
	                declare @now datetime
	                set @now = getdate()
	                exec dbo.[P_remotelabdb_logevent] 'LOGON',@username, @computer, @now 
                END");

            Sql(@"CREATE PROCEDURE [dbo].[P_remotelabdb_logoff] 
	                @computer varchar(50)	
                AS
                BEGIN
	                -- SET NOCOUNT ON added to prevent extra result sets from
	                -- interfering with SELECT statements.
	                SET NOCOUNT ON;

	                declare @username varchar(50)
	                set @username = (SELECT UserName from Computers where ComputerName = @computer)
	                if (@username is null) set @username = 'N/A'
	
                    -- Insert statements for procedure here
	                Update Computers set 
		                UserName = null,
		                Reserved = null,
		                Logon = null
	                where ComputerName = @computer


	                -- log the event
	                declare @now datetime
	                set @now = getdate()
	                exec dbo.[P_remotelabdb_logevent] 'LOGOFF',@username, @computer, @now 
                END");

            Sql(@"CREATE PROCEDURE [dbo].[P_remotelabdb_logevent]
	                @event varchar(50),
	                @username varchar(50),
	                @compname varchar(50),
	                @dtstamp datetime
                AS
                BEGIN
	                -- SET NOCOUNT ON added to prevent extra result sets from
	                -- interfering with SELECT statements.
	                SET NOCOUNT ON;

                    -- Insert statements for procedure here
	                insert into Events (EventName, UserName, ComputerName, DtStamp) 
	                values (@event, @username, @compname, @dtstamp); 

                END");

            Sql(@"create PROCEDURE [dbo].[P_remotelabdb_fail_tcp_check] 
	                @computer varchar(50),
	                @username varchar(50)
                AS
                BEGIN
	                -- SET NOCOUNT ON added to prevent extra result sets from
	                -- interfering with SELECT statements.
	                SET NOCOUNT ON;

                    -- Insert statements for procedure here
	                Update Computers set 
		                UserName = 'rdp check',
		                Reserved = null
		                where computername = @computer

	                -- log the event
	                declare @now datetime
	                set @now = getdate()
	                exec dbo.[P_remotelabdb_logevent] 'FAIL RDP CHECK',@username, @computer, @now 
                END");

            Sql(@"CREATE PROCEDURE [dbo].[P_remotelabdb_clear_reservation] 
	                @computer varchar(50)
                AS
                BEGIN
	                -- SET NOCOUNT ON added to prevent extra result sets from
	                -- interfering with SELECT statements.
	                SET NOCOUNT ON;
		                declare @username varchar(50)
	                set @username = (SELECT UserName from Computers where ComputerName = @computer)
	                if @username is null return 

                    -- Insert statements for procedure here
	                Update Computers set 
		                UserName = 'cleared',
		                Reserved = null
		                where ComputerName = @computer

	                -- log the event
	                declare @now datetime
	                set @now = getdate()
	                exec dbo.[P_remotelabdb_logevent] 'CLEAR RESERVATION',@username, @computer, @now 
                END");


            Sql(@"CREATE procedure [dbo].[P_remotelabdb_reservation_cleanup] (
					@poolname varchar(50),
	                @minutes int
                )
                as
                begin
					SET NOCOUNT ON;
	                declare @now datetime
	                declare @compname varchar(50)
	                declare @username varchar(50)
	                declare cleanupCursor cursor for
		                select ComputerName, UserName 
			                from dbo.Computers
				                where (Pool_PoolName=@poolname) AND (Reserved IS NOT NULL) AND (Logon IS NULL) AND (UserName IS NOT NULL)
				                AND DATEDIFF(mi,reserved,GETDATE())>=@minutes

	                open cleanupCursor
		                fetch next from cleanupCursor into @compname, @username

	                while @@FETCH_STATUS= 0
	                begin
		                update Computers set
			                UserName = null,
			                Logon = null,
			                Reserved = null
		                where ComputerName= @compname
		                set @now = getdate()
		                exec dbo.P_remotelabdb_logevent 'EXPIRED RESERVATION', @username, @compname,@now
		                fetch next from cleanupCursor into @compname, @username
	                end

	                close cleanupCursor
	                deallocate cleanupCursor
                end");

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
