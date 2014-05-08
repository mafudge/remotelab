namespace RemoteLab.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class FixComputerTable : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Computers", "Reserved", c => c.DateTime());
            AlterColumn("dbo.Computers", "Logon", c => c.DateTime());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Computers", "Logon", c => c.DateTime(nullable: false));
            AlterColumn("dbo.Computers", "Reserved", c => c.DateTime(nullable: false));
        }
    }
}
