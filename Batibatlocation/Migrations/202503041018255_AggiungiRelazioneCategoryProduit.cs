namespace Batibatlocation.Data
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AggiungiRelazioneCategoryProduit : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Produits", "PeriodiciteId", c => c.Int(nullable: false, identity: false, defaultValue: 1));
            CreateIndex("dbo.Produits", "PeriodiciteId");
            AddForeignKey("dbo.Produits", "PeriodiciteId", "dbo.Periodicites", "Id", cascadeDelete: true);
        }

        public override void Down()
        {
            DropForeignKey("dbo.Produits", "PeriodiciteId", "dbo.Periodicites");
            DropIndex("dbo.Produits", new[] { "PeriodiciteId" });
            DropColumn("dbo.Produits", "PeriodiciteId");
        }
    }
}
