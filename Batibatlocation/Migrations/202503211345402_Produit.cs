namespace Batibatlocation.Data
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Produit : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Produits", "LastMod", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Produits", "LastMod");
        }
    }
}
