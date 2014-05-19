namespace RemoteLab.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class StoredProcs : DbMigration
    {
        public override void Up()
        {

            // P_remotelabdb_logevent
            Sql(@"CREATE PROCEDURE [dbo].[P_remotelabdb_logevent]
	                @event varchar(50),
	                @username varchar(50),
	                @compname varchar(50),
					@poolname varchar(50),
	                @dtstamp datetime
                AS
                BEGIN
	                -- SET NOCOUNT ON added to prevent extra result sets from
	                -- interfering with SELECT statements.
	                SET NOCOUNT ON;

                    -- Insert statements for procedure here
	                insert into Events (EventName, UserName, ComputerName, PoolName, DtStamp) 
	                values (@event, @username, @compname, @poolname, @dtstamp); 

                END");

            // P_remotelabdb_startup
            Sql(@"CREATE PROCEDURE [dbo].[P_remotelabdb_startup] 
                	@computer varchar(50),
					@poolname varchar(50)
            AS
            BEGIN
	            -- SET NOCOUNT ON added to prevent extra result sets from
	            -- interfering with SELECT statements.
	            SET NOCOUNT ON;
				declare @now datetime
				set @now = getdate()
	
				-- check for existence of pool
				If exists(select * from Pools where PoolName = @poolname)
				Begin
					-- insert or update computer record 
					If exists(select * from Computers where ComputerName = @computer)
					Begin	
						Update Computers set 
						UserName= null,
						Logon = null,
						Reserved = null,
						Pool_PoolName = @poolname
						where ComputerName= @computer
					End
					Else 
					Begin					
						Insert into Computers (ComputerName, UserName, Reserved, Logon, Pool_PoolName) values (@computer,null, null, null,@poolname)
					End

					-- log the event
					exec dbo.[P_remotelabdb_logevent] 'STARTUP','N/A', @computer, @poolname, @now 
				End
				Else -- Pool does not exist
				Begin
					exec dbo.[P_remotelabdb_logevent] 'INVALID POOL','N/A', @computer, @poolname, @now 
				End	
            END");

            // P_remotelabdb_shutdown
            Sql(@"CREATE PROCEDURE [dbo].[P_remotelabdb_shutdown] 
	                @computer varchar(50)
                AS
                BEGIN
	                -- SET NOCOUNT ON added to prevent extra result sets from
	                -- interfering with SELECT statements.
	                SET NOCOUNT ON;

	                declare @username varchar(50)
					declare @poolname varchar(50)
	                SELECT  @username = UserName, @poolname = Pool_PoolName from Computers where ComputerName= @computer
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
	                exec dbo.[P_remotelabdb_logevent] 'SHUTDOWN',@username, @computer, @poolname, @now 
                END");

            // P_remotelabdb_reserve
            Sql(@"CREATE PROCEDURE [dbo].[P_remotelabdb_reserve] 
	                @computer varchar(50),
	                @username varchar(50)
                AS
                BEGIN
	                -- SET NOCOUNT ON added to prevent extra result sets from
	                -- interfering with SELECT statements.
	                SET NOCOUNT ON;
					declare @poolname as varchar(50)
					set @poolname = (select Pool_PoolName from Computers where ComputerName=@computer);
	
                    -- Insert statements for procedure here
	                Update Computers set 
		                UserName = @username,
		                Reserved = getdate()
		                where ComputerName = @computer

	                -- log the event
	                declare @now datetime
	                set @now = getdate()
	                exec dbo.[P_remotelabdb_logevent] 'RESERVE',@username, @computer, @poolname, @now 
                END");

            // P_remotelabdb_logon
            Sql(@"CREATE PROCEDURE [dbo].[P_remotelabdb_logon] 
	                @computer varchar(50),
	                @username varchar(50)
                AS
                BEGIN
	                -- SET NOCOUNT ON added to prevent extra result sets from
	                -- interfering with SELECT statements.
	                SET NOCOUNT ON;
					declare @poolname as varchar(50)
					set @poolname = (select Pool_PoolName from Computers where ComputerName=@computer)
	
                    -- Insert statements for procedure here
	                Update Computers set 
		                UserName = @username,
		                Logon = getdate()
		                where ComputerName = @computer

	                -- log the event
	                declare @now datetime
	                set @now = getdate()
	                exec dbo.[P_remotelabdb_logevent] 'LOGON',@username, @computer, @poolname, @now 
                END");

            // P_remotelabdb_logoff
            Sql(@"CREATE PROCEDURE [dbo].[P_remotelabdb_logoff] 
	                @computer varchar(50)	
                AS
                BEGIN
	                -- SET NOCOUNT ON added to prevent extra result sets from
	                -- interfering with SELECT statements.
	                SET NOCOUNT ON;

	                declare @username varchar(50)
					declare @poolname varchar(50)
	                SELECT  @username = UserName, @poolname = Pool_PoolName from Computers where ComputerName= @computer
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
	                exec dbo.[P_remotelabdb_logevent] 'LOGOFF',@username, @computer, @poolname, @now 
                END");

            // P_remotelabdb_fail_tcp_check
            Sql(@"CREATE PROCEDURE [dbo].[P_remotelabdb_fail_tcp_check] 
	                @computer varchar(50),
	                @username varchar(50)
                AS
                BEGIN
	                -- SET NOCOUNT ON added to prevent extra result sets from
	                -- interfering with SELECT statements.
	                SET NOCOUNT ON;
					declare @poolname as varchar(50)
					set @poolname = (select Pool_PoolName from Computers where ComputerName=@computer);

                    -- Insert statements for procedure here
	                Update Computers set 
		                UserName = 'rdp check',
		                Reserved = null
		                where computername = @computer

	                -- log the event
	                declare @now datetime
	                set @now = getdate()
	                exec dbo.[P_remotelabdb_logevent] 'FAIL RDP CHECK',@username, @computer, @poolname, @now 
                END");

            // P_remotelabdb_clear_reservation
            Sql(@"CREATE PROCEDURE [dbo].[P_remotelabdb_clear_reservation] 
	                @computer varchar(50)
                AS
                BEGIN
	                -- SET NOCOUNT ON added to prevent extra result sets from
	                -- interfering with SELECT statements.
	                SET NOCOUNT ON;
		            declare @username varchar(50)
					declare @poolname varchar(50)
	                SELECT  @username = UserName, @poolname = Pool_PoolName from Computers where ComputerName= @computer
	                if @username is null return 

                    -- Insert statements for procedure here
	                Update Computers set 
		                UserName = 'cleared',
		                Reserved = null
		                where ComputerName = @computer

	                -- log the event
	                declare @now datetime
	                set @now = getdate()
	                exec dbo.[P_remotelabdb_logevent] 'CLEAR RESERVATION',@username, @computer, @poolname, @now 
                END");


            // P_remotelabdb_reservation_cleanup
            Sql(@"CREATE PROCEDURE [dbo].[P_remotelabdb_reservation_cleanup] 
					@poolname varchar(50),
	                @minutes int
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
		                exec dbo.P_remotelabdb_logevent 'EXPIRED RESERVATION', @username, @compname,@poolname, @now
		                fetch next from cleanupCursor into @compname, @username
	                end

	                close cleanupCursor
	                deallocate cleanupCursor
                END");

        }

        public override void Down()
        {
            Sql(@"DROP PROCEDURE [dbo].[P_remotelabdb_startup]");
            Sql(@"DROP PROCEDURE [dbo].[P_remotelabdb_shutdown]");
            Sql(@"DROP PROCEDURE [dbo].[P_remotelabdb_reserve]");
            Sql(@"DROP PROCEDURE [dbo].[P_remotelabdb_logon]");
            Sql(@"DROP PROCEDURE [dbo].[P_remotelabdb_logoff]");
            Sql(@"DROP PROCEDURE [dbo].[P_remotelabdb_fail_tcp_check]");
            Sql(@"DROP PROCEDURE [dbo].[P_remotelabdb_clear_reservation]");
            Sql(@"DROP PROCEDURE [dbo].[P_remotelabdb_reservation_cleanup]");
            Sql(@"DROP PROCEDURE [dbo].[P_remotelabdb_logevent]");

        }

    }
}
