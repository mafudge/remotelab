namespace RemoteLab.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ComputersLastModified : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Computers", "LastModified", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Computers", "LastModified");
        }
    }
}
