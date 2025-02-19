namespace Batibatlocation.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateTableEchafaudage : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Echafaudages", "SpecificheTechniques", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Echafaudages", "SpecificheTechniques");
        }
    }
}
