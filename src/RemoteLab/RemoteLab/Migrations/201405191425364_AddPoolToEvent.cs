namespace RemoteLab.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddPoolToEvent : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Events", "PoolName", c => c.String(nullable: true, maxLength: 50));


        }
        
        public override void Down()
        {
            DropColumn("dbo.Events", "PoolName");
        }
    }
}
