namespace RemoteLab.Migrations
{
    using RemoteLab.Models;
using System;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<RemoteLabContext>
    {

        private readonly bool PendingMigrations;
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
            var migrator = new DbMigrator(this);
            this.PendingMigrations = migrator.GetPendingMigrations().Any();
        }

        protected override void Seed(RemoteLabContext context)
        {
            //  This method will be called after migrating to the latest version.

            foreach( Computer c in context.Computers) { 
                context.Computers.Remove(c);
            }

            if (!this.PendingMigrations) return; 

            context.Computers.Add( new Computer() { ComputerName="RLAB-01", Pool="Prod" });
            context.Computers.Add( new Computer() { ComputerName = "RLAB-02", Pool = "Prod" });
            context.SaveChanges();

        }
    }
}
