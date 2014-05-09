using RemoteLab.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Data.Entity;
using RemoteLab.Utilities;

namespace RemoteLab.Services
{
    public class RemoteLabService
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

        public async Task<RemoteLabViewModel> PopulateRemoteLabViewModelAsync(String DefaultPool, String CurrentUser, int CleanupInMinutes)
        {
            var rvm = new RemoteLabViewModel()
            {
                Pool = DefaultPool,
                CurrentUser = CurrentUser,
                ReservationStatus = ReservationStatus.Unknown
            };
            await this.ReservationCleanupAsync(CleanupInMinutes);
            rvm.RemoteLabComputer = await this.GetExistingReservationAsync(rvm);
            if (rvm.RemoteLabComputer != null)
            {
                // we have a reservation
                rvm.ReservationStatus = ReservationStatus.ExistingReservation;
            }
            else
            {
                // you are here fore the new reservation
                rvm.ReservationStatus = ReservationStatus.NewReservation;
                rvm.RemoteLabComputer = await this.GetNewReservationAsync(rvm);
            }

            if (rvm.RemoteLabComputer == null)
            {
                // The pool is full
                rvm.ReservationStatus = ReservationStatus.PoolFull;
            }

            return rvm;

        }

        public async Task<bool> RebootComputerAsync(String ComputerName, String CurrentUser, DateTime Now)
        {
            var RebootResult = await this.CompMgmt.RebootComputerAsync(ComputerName, Properties.Settings.Default.ActiveDirectoryFqdn, 
                Properties.Settings.Default.RemotePowershellUser, Properties.Settings.Default.RemotePowershellPassword, Properties.Settings.Default.RemotePoweshellUserDomain);
            if (!RebootResult) 
            {
                await this.LogEventAsync("REBOOT FAILED", CurrentUser, ComputerName, Now);
                var msg = String.Format(Properties.Resources.RebootFailedEmailMessage, ComputerName);
                // TODO: IOC these arguments for testability.
                await Smtp.SendMailAsync(Properties.Settings.Default.SmtpServer, Properties.Settings.Default.SmtpMessageFromAddress, Properties.Settings.Default.SmtpMessageToAddress, msg, msg);
            }
            return RebootResult;
        }

        public async Task LogAndEmailPoolFullEventAsync(String Pool, String ComputerName, String UserName, DateTime Now)
        {
            await this.LogEventAsync(String.Format("POOL {0} IS FULL",Pool), UserName, ComputerName, Now);
            var msg = String.Format(Properties.Resources.LabPoolIsFullEmailMessage, Pool);
            // TODO: IOC these arguments for testability.
            await Smtp.SendMailAsync(Properties.Settings.Default.SmtpServer, Properties.Settings.Default.SmtpMessageFromAddress, Properties.Settings.Default.SmtpMessageToAddress, msg, msg);
        }

        public async Task<bool> CheckRdpPortAndRebootIfUnresponsiveAsync(String ComputerName, String ComputerDomain, String UserName, int RdpTcpPort)
        {
            bool result = false;
            bool success = await this.CompMgmt.ConnectToTcpPortAsync(ComputerName, ComputerDomain, RdpTcpPort);
            if (!success) 
            {
                await this.LogFailedRdpTcpPortCheckAsync(ComputerName, UserName);
                result = await this.RebootComputerAsync(ComputerName, UserName, System.DateTime.Now);
            }
            //TODO: FOR NOW ALWARE RETURN TRUE
            return true;

            //return result && success;
        }


        public async Task MakeReservationAsync(String ComputerName, String UserName)
        {
            await this.Db.Database.ExecuteSqlCommandAsync(@"EXECUTE [dbo].[P_remotelabdb_reserve] {0}, {1}",ComputerName, UserName);
        }
        public  async Task LogFailedRdpTcpPortCheckAsync(String ComputerName, String UserName) 
        {
            await this.Db.Database.ExecuteSqlCommandAsync(@"EXECUTE [dbo].[P_remotelabdb_fail_tcp_check] {0}, {1}",ComputerName,UserName); 
        }

        public async Task ReservationCleanupAsync(int CleanupInMinutes)
        {
            await this.Db.Database.ExecuteSqlCommandAsync(@"EXECUTE [dbo].[P_remotelabdb_reservation_cleanup] {0}", CleanupInMinutes);
        }

        public async Task ClearReservationAsync(String ReservationComputerName)
        {
            await this.Db.Database.ExecuteSqlCommandAsync(@"EXECUTE [dbo].[P_remotelabdb_clear_reservation] {0}", ReservationComputerName);
        }

        public async Task LogEventAsync(String EventName,  String UserName, String ComputerName, DateTime Now)
        {
            await this.Db.Database.ExecuteSqlCommandAsync(@"EXECUTE [dbo].[P_remotelabdb_logevent] {0}, {1}, {2}, {3}", EventName , UserName, ComputerName, Now);
        }

        public async Task<Computer> GetExistingReservationAsync(RemoteLabViewModel rvm)
        {
            return await this.Db.Computers.Where(c =>
                    c.Pool.Equals(rvm.Pool, StringComparison.InvariantCultureIgnoreCase) &&
                    c.UserName.Equals(rvm.CurrentUser, StringComparison.InvariantCultureIgnoreCase)
                ).OrderBy(c => c.ComputerName).FirstOrDefaultAsync();
        }

        public async Task<Computer> GetNewReservationAsync(RemoteLabViewModel rvm)
        {
            return await this.Db.Computers.Where(c =>
                    c.Pool.Equals(rvm.Pool, StringComparison.InvariantCultureIgnoreCase) &&
                    c.UserName == null
                ).OrderBy(c => c.ComputerName).FirstOrDefaultAsync();
        }

           
    }
}