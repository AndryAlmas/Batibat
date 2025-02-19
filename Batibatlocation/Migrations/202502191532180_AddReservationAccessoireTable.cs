namespace Batibatlocation.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddReservationAccessoireTable : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Accessoires", "ReservationId", "dbo.Reservations");
            DropIndex("dbo.Accessoires", new[] { "ReservationId" });
            CreateTable(
                "dbo.ReservationAccessoires",
                c => new
                    {
                        ReservationId = c.Int(nullable: false),
                        AccessoireId = c.Int(nullable: false),
                        Quantite = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.ReservationId, t.AccessoireId })
                .ForeignKey("dbo.Reservations", t => t.ReservationId, cascadeDelete: true)
                .ForeignKey("dbo.Accessoires", t => t.AccessoireId, cascadeDelete: true)
                .Index(t => t.ReservationId)
                .Index(t => t.AccessoireId);
            
            DropColumn("dbo.Accessoires", "ReservationId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Accessoires", "ReservationId", c => c.Int(nullable: false));
            DropForeignKey("dbo.ReservationAccessoires", "AccessoireId", "dbo.Accessoires");
            DropForeignKey("dbo.ReservationAccessoires", "ReservationId", "dbo.Reservations");
            DropForeignKey("dbo.ReservationAccessoire1", "Accessoire_Id", "dbo.Accessoires");
            DropForeignKey("dbo.ReservationAccessoire1", "Reservation_Id", "dbo.Reservations");
            DropIndex("dbo.ReservationAccessoire1", new[] { "Accessoire_Id" });
            DropIndex("dbo.ReservationAccessoire1", new[] { "Reservation_Id" });
            DropIndex("dbo.ReservationAccessoires", new[] { "AccessoireId" });
            DropIndex("dbo.ReservationAccessoires", new[] { "ReservationId" });
            DropTable("dbo.ReservationAccessoire1");
            DropTable("dbo.ReservationAccessoires");
            CreateIndex("dbo.Accessoires", "ReservationId");
            AddForeignKey("dbo.Accessoires", "ReservationId", "dbo.Reservations", "Id", cascadeDelete: true);
        }
    }
}
