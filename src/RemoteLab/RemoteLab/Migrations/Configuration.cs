namespace RemoteLab.Migrations
{
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
    using System.Data.Entity.Migrations;
    using System.Data.Entity.Validation;

    internal sealed class Configuration : DbMigrationsConfiguration<RemoteLabContext>
    {

        private readonly bool PendingMigrations;
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;   
            var migrator = new DbMigrator(this);
            this.PendingMigrations = migrator.GetPendingMigrations().Any();
        }

        private void DataCleanup(RemoteLabContext context)
        {
            foreach( Event e in context.Events) { context.Events.Remove(e); }
            foreach (Computer c in context.Computers) { context.Computers.Remove(c); }
            foreach (Pool p in context.Pools) { context.Pools.Remove(p); }
            context.SaveChanges();

        }

        private void DataInit( RemoteLabContext context ) 
        {
            PasswordUtility Pass = new PasswordUtility(Properties.Settings.Default.EncryptionKeyForPasswords);
            var stringVector = Pass.NewInitializationVector();
            var prodPool = context.Pools.Add(new Pool() 
            {
                PoolName = "Prod",
                EmailNotifyList = "mjschug@syr.edu",
                RdpTcpPort = 3389,
                CleanupInMinutes = 30,
                ActiveDirectoryAdminGroup = "IST-Users-ITServices",
                ActiveDirectoryUserGroup = "IST-Users-ITServices",
                RemoteAdminUser = "w-ist-labsetup",
                RemoteAdminPassword = Pass.Encrypt("6f1HMkW1dB", stringVector),
                WelcomeMessage = "Welcome to this pool!",
                InitializationVector = stringVector
            });
            stringVector = Pass.NewInitializationVector();
            var whit = context.Pools.Add(new Pool() 
            {
                PoolName = "Whitman",
                EmailNotifyList = "mjschug@syr.edu",
                RdpTcpPort = 3389,
                CleanupInMinutes = 30,
                ActiveDirectoryAdminGroup = "IST-Staff",
                ActiveDirectoryUserGroup = "IST-Staff",
                RemoteAdminUser = "w-ist-labsetup",
                RemoteAdminPassword = Pass.Encrypt("6f1HMkW1dB", stringVector),
                WelcomeMessage = "Welcome to this pool!",
                InitializationVector = stringVector
            });
            stringVector = Pass.NewInitializationVector();
            var ischool = context.Pools.Add(new Pool() 
            {
                PoolName = "iSchool",
                EmailNotifyList = "mjschug@syr.edu",
                RdpTcpPort = 3389,
                CleanupInMinutes = 30,
                ActiveDirectoryAdminGroup = "IST-Staff",
                ActiveDirectoryUserGroup = "IST-Users",
                RemoteAdminUser = "w-ist-labsetup",
                RemoteAdminPassword = Pass.Encrypt("6f1HMkW1dB", stringVector),
                WelcomeMessage = "Welcome to this pool!",
                InitializationVector = stringVector
            });
            stringVector = Pass.NewInitializationVector();
            var maxwell = context.Pools.Add(new Pool() 
            {
                PoolName = "Maxwell",
                EmailNotifyList = "mjschug@syr.edu",
                RdpTcpPort = 3389,
                CleanupInMinutes = 30,
                ActiveDirectoryAdminGroup = "IST-Staff",
                ActiveDirectoryUserGroup = "IST-Staff",
                RemoteAdminUser = "w-ist-labsetup",
                RemoteAdminPassword = Pass.Encrypt("6f1HMkW1dB", stringVector),
                WelcomeMessage = "Welcome to this pool!",
                InitializationVector = stringVector
            });

            try
            {
                context.SaveChanges();
            }
            catch (DbEntityValidationException e)
            {
                foreach (var eve in e.EntityValidationErrors)
                {
                    Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                        eve.Entry.Entity.GetType().Name, eve.Entry.State);
                    foreach (var ve in eve.ValidationErrors)
                    {
                        Console.WriteLine("- Property: \"{0}\", Error: \"{1}\"",
                            ve.PropertyName, ve.ErrorMessage);
                    }
                }
            }  

            context.Computers.Add(new Computer() { ComputerName = "IST-LD-RLAB-H31", Pool = prodPool });
            context.Computers.Add(new Computer() { ComputerName = "IST-LD-RLAB-H32", Pool = prodPool });

            for (int i = 1; i <= 50; i++)
            {
                context.Computers.Add(new Computer() { ComputerName = String.Format("MAX-{0:00}", i), Pool = maxwell });
                context.Computers.Add(new Computer() { ComputerName = String.Format("WHIT-{0:00}", i), Pool = whit });
            }

            for (int i = 1; i <= 30; i++)
            {
                context.Computers.Add(new Computer() { ComputerName = String.Format("IST-{0:00}", i), Pool = ischool });
            }

            try 
            {
                context.SaveChanges();
            }
            catch (DbEntityValidationException e)
            {
                foreach (var eve in e.EntityValidationErrors)
                {
                    Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                        eve.Entry.Entity.GetType().Name, eve.Entry.State);
                    foreach (var ve in eve.ValidationErrors)
                    {
                        Console.WriteLine("- Property: \"{0}\", Error: \"{1}\"",
                            ve.PropertyName, ve.ErrorMessage);
                    }
                }
            }  

            for (int i = 1; i <= 10; i++)
            {
                context.Database.ExecuteSqlCommand(@"exec [dbo].[P_remotelabdb_reserve] {0}, {1}", String.Format("WHIT-{0:00}", i), String.Format("whtusr{0}", i));
                context.Database.ExecuteSqlCommand(@"exec [dbo].[P_remotelabdb_reserve] {0}, {1}", String.Format("MAX-{0:00}", i), String.Format("maxusr{0}", i)); ;
                context.Database.ExecuteSqlCommand(@"exec [dbo].[P_remotelabdb_reserve] {0}, {1}", String.Format("IST-{0:00}", i), String.Format("istusr{0}", i));
            }

            for (int i = 11; i <= 15; i++)
            {
                context.Database.ExecuteSqlCommand(@"exec [dbo].[P_remotelabdb_reserve] {0}, {1}", String.Format("MAX-{0:00}", i), String.Format("maxusr{0}", i)); ;
                context.Database.ExecuteSqlCommand(@"exec [dbo].[P_remotelabdb_reserve] {0}, {1}", String.Format("IST-{0:00}", i), String.Format("istusr{0}", i));
            }
            for (int i = 13; i <= 20; i++)
            {
                context.Database.ExecuteSqlCommand(@"exec [dbo].[P_remotelabdb_reserve] {0}, {1}", String.Format("MAX-{0:00}", i), String.Format("maxusr{0}", i));
                context.Database.ExecuteSqlCommand(@"exec [dbo].[P_remotelabdb_reserve] {0}, {1}", String.Format("IST-{0:00}", i), String.Format("istusr{0}", i));
            }
            for (int i = 21; i <= 25; i++)
            {
                context.Database.ExecuteSqlCommand(@"exec [dbo].[P_remotelabdb_reserve] {0}, {1}", String.Format("IST-{0:00}", i), String.Format("istusr{0}", i));
            }


            context.SaveChanges();

        }


        protected override void Seed(RemoteLabContext context)
        {

#if (DEBUG)
            DataCleanup(context);
            DataInit(context);
#endif

        }
    }
}
