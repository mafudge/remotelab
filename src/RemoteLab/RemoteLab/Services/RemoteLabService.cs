using RemoteLab.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Data.Entity;
using RemoteLab.Utilities;
using System.IO;
using System.Text;
using System.Security.Claims;

namespace RemoteLab.Services
{
    public class RemoteLabService : IDisposable
    {
        private readonly RemoteLabContext Db;
        private readonly ComputerManagement CompMgmt;
        private readonly SmtpEmail Smtp;

        public RemoteLabService(RemoteLabContext Db, ComputerManagement CompMgmt, SmtpEmail Smtp)
        {
            this.Db = Db;
            this.CompMgmt = CompMgmt;
            this.Smtp = Smtp;

        }

        public async Task<RemoteLabViewModel> PopulateRemoteLabViewModelAsync(String PoolName, String CurrentUser)
        {
            var rvm = new RemoteLabViewModel()
            {
                CurrentUser = CurrentUser,
                ReservationStatus = ReservationStatus.NoPoolSelected
            };
            rvm.Pool = await this.GetPoolByIdAsync(PoolName ?? String.Empty); 
            if (rvm.Pool == null)
            {
                // no pool selected
                rvm.ReservationStatus = ReservationStatus.NoPoolSelected;

            }
            else // we have selected a pool, so we can continue
            {
                rvm.RemoteLabComputer = await this.GetExistingReservationAsync(rvm);
                await this.ReservationCleanupAsync(rvm.Pool.PoolName, rvm.Pool.CleanupInMinutes);
                if (rvm.RemoteLabComputer != null)
                {
                    // we have a reservation
                    rvm.ReservationStatus = ReservationStatus.ExistingReservation;
                }
                else
                {
                    // you are here for the new reservation
                    rvm.ReservationStatus = ReservationStatus.NewReservation;
                    rvm.RemoteLabComputer = await this.GetNewReservationAsync(rvm);
                }

                if (rvm.RemoteLabComputer == null)
                {
                    // The pool is full
                    rvm.ReservationStatus = ReservationStatus.PoolFull;
                }

            }
            return rvm;

        }

        public async Task<bool> RebootComputerAsync(String ComputerName, String CurrentUser, String PoolName, DateTime Now)
        {
            var RebootResult = await this.CompMgmt.RebootComputerAsync(ComputerName, Properties.Settings.Default.ActiveDirectoryFqdn, 
                Properties.Settings.Default.RemotePowershellUser, Properties.Settings.Default.RemotePowershellPassword, Properties.Settings.Default.RemotePoweshellUserDomain);
            if (!RebootResult) 
            {
                await this.LogEventAsync("REBOOT FAILED", CurrentUser, ComputerName, PoolName,Now);
                var msg = String.Format(Properties.Resources.RebootFailedEmailMessage, ComputerName);
                var pool = await this.GetPoolByIdAsync(PoolName);
                // TODO: IOC these arguments for testability.
                await Smtp.SendMailAsync(Properties.Settings.Default.SmtpServer, Properties.Settings.Default.SmtpMessageFromAddress, pool.EmailNotifyList, msg, msg);
            }
            return RebootResult;
        }

        public async Task LogAndEmailPoolFullEventAsync(String PoolName, String ComputerName, String UserName, DateTime Now)
        {
            await this.LogEventAsync("POOL FULL", UserName, ComputerName, PoolName, Now);
            var msg = String.Format(Properties.Resources.LabPoolIsFullEmailMessage, PoolName);
            var pool = await this.GetPoolByIdAsync(PoolName);
            // TODO: IOC these arguments for testability.
            await Smtp.SendMailAsync(Properties.Settings.Default.SmtpServer, Properties.Settings.Default.SmtpMessageFromAddress, pool.EmailNotifyList, msg, msg);
        }

        public async Task<bool> CheckRdpPortAndRebootIfUnresponsiveAsync(String ComputerName, String ComputerDomain, String UserName, String PoolName, int RdpTcpPort)
        {
            bool result = false;
            bool success = await this.CompMgmt.ConnectToTcpPortAsync(ComputerName, ComputerDomain, RdpTcpPort);
            if (!success) 
            {
                await this.LogFailedRdpTcpPortCheckAsync(ComputerName, UserName);
                result = await this.RebootComputerAsync(ComputerName, UserName, PoolName, System.DateTime.Now);
            }

            return success;
        }

        public async Task<IEnumerable<Pool>> GetPoolsAsync()
        {
            return await this.Db.Pools.ToListAsync();
        }

        public async Task AddPoolAsync(Pool p)
        {
            this.Db.Pools.Add(p);
            await this.Db.SaveChangesAsync();
        }

        public async Task<IEnumerable<Computer>> GetComputersByPoolNameAsync(String PoolName)
        {
            return await this.Db.Computers.Where( c => c.Pool.PoolName.Equals(PoolName, StringComparison.InvariantCultureIgnoreCase)).ToListAsync();
        }

        public async Task<Pool> GetPoolByIdAsync(string PoolName) 
        {
            return await this.Db.Pools.FindAsync(PoolName);

        }

        public Pool GetPoolById(string PoolName)
        {
            return this.Db.Pools.Find(PoolName);
        }

        public async Task RemovePoolByIdAsync(String PoolName)
        {
            await this.Db.Database.ExecuteSqlCommandAsync(@"DELETE FROM [dbo].[Computers] WHERE [Pool_PoolName] = {0}; DELETE FROM [dbo].[Pools] WHERE [PoolName] = {0}", PoolName);
        }

        public async Task UpdatePoolAsync(Pool p)
        {
            await this.Db.Database.ExecuteSqlCommandAsync(@"UPDATE [dbo].[Pools] 
                    SET [ActiveDirectoryUserGroup] = {0},
                    [Logo] = {1}, 
                    [ActiveDirectoryAdminGroup] = {2}, 
                    [EmailNotifyList] = {3}, 
                    [RdpTcpPort] = {4}, 
                    [CleanupInMinutes] = {5}  
                    WHERE [PoolName] = {6}"
                ,p.ActiveDirectoryUserGroup, p.Logo, p.ActiveDirectoryAdminGroup, p.EmailNotifyList, p.RdpTcpPort, p.CleanupInMinutes, p.PoolName);
        }


        public IEnumerable<PoolSummary> GetPoolSummaryByAdminClaims(ClaimsPrincipal user, String AdministratorADGroup) 
        {

            var summarypools = this.GetPoolSummary();
            //If You're the app admin return all pools
            if (user.Claims.Any( c=> c.Type==ClaimTypes.Role && c.Value.Equals(AdministratorADGroup,StringComparison.InvariantCultureIgnoreCase)) )
            {                
                return summarypools;
            }
            else  // otherwise, return only the pools where you have claim to the Admin Group
            {
                var roles = user.Claims.Where( c=> c.Type==ClaimTypes.Role).Select(c=> c.Value);
                return summarypools.Where( p=> roles.Contains(p.ActiveDirectoryAdminGroup));
            }
        }

        public IEnumerable<PoolSummary> GetPoolSummaryByUserClaims(ClaimsPrincipal user)
        {
            var summarypools = this.GetPoolSummary();
            var roles = user.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value);
            return summarypools.Where(p => roles.Contains(p.ActiveDirectoryUserGroup));
        }
        public async Task<PoolSummary> GetPoolSummaryAsync( String PoolName )
        {
            return await this.Db.Database.SqlQuery<PoolSummary>(@"SELECT * FROM PoolSummary WHERE PoolName = {0}",PoolName).FirstOrDefaultAsync();
        }

        public IEnumerable<PoolSummary> GetPoolSummary()
        {
            return this.Db.Database.SqlQuery<PoolSummary>(@"SELECT * FROM PoolSummary").AsQueryable<PoolSummary>().ToList<PoolSummary>();
        }

        public async Task MakeReservationAsync(String ComputerName, String UserName)
        {
            await this.Db.Database.ExecuteSqlCommandAsync(@"EXECUTE [dbo].[P_remotelabdb_reserve] {0}, {1}",ComputerName, UserName);
        }
        public  async Task LogFailedRdpTcpPortCheckAsync(String ComputerName, String UserName) 
        {
            await this.Db.Database.ExecuteSqlCommandAsync(@"EXECUTE [dbo].[P_remotelabdb_fail_tcp_check] {0}, {1}",ComputerName,UserName); 
        }

        public async Task ReservationCleanupAsync(string PoolName, int CleanupInMinutes)
        {
            await this.Db.Database.ExecuteSqlCommandAsync(@"EXECUTE [dbo].[P_remotelabdb_reservation_cleanup] {0}, {1}", PoolName, CleanupInMinutes);
        }

        public async Task ClearReservationAsync(String ReservationComputerName)
        {
            await this.Db.Database.ExecuteSqlCommandAsync(@"EXECUTE [dbo].[P_remotelabdb_clear_reservation] {0}", ReservationComputerName);
        }

        public async Task LogEventAsync(String EventName,  String UserName, String ComputerName, String PoolName, DateTime Now)
        {
            await this.Db.Database.ExecuteSqlCommandAsync(@"EXECUTE [dbo].[P_remotelabdb_logevent] {0}, {1}, {2}, {3}, {4}", EventName , UserName, ComputerName, PoolName, Now);
        }

        public async Task<Computer> GetExistingReservationAsync(RemoteLabViewModel rvm)
        {
            return await this.Db.Computers.Where(c =>
                    c.Pool.PoolName.Equals(rvm.Pool.PoolName, StringComparison.InvariantCultureIgnoreCase) &&
                    c.UserName.Equals(rvm.CurrentUser, StringComparison.InvariantCultureIgnoreCase)
                ).OrderBy(c => c.ComputerName).FirstOrDefaultAsync();
        }
        

        public async Task<Computer> GetNewReservationAsync(RemoteLabViewModel rvm)
        {
            return await this.Db.Computers.Where(c =>
                    c.Pool.PoolName.Equals(rvm.Pool.PoolName, StringComparison.InvariantCultureIgnoreCase) &&
                    c.UserName == null
                ).OrderBy(c => c.ComputerName).FirstOrDefaultAsync();
        }

        public String GenerateRdpFileContents(string rdpFileSettings, string computer, string username,  int width = 1920, int height = 1200)
        {
            return String.Format(rdpFileSettings,width,height,computer,username);
        }

        public void Dispose()
        {
            this.Db.Dispose();
        }
    }
}