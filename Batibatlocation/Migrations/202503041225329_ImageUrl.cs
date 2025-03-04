namespace Batibatlocation.Data
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ImageUrl : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Produits", "ImageUrl", c => c.String(maxLength: 255));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Produits", "ImageUrl", c => c.String(nullable: false, maxLength: 255));
        }
    }
}
