using System.Data.Entity;
using Batibatlocation.Enum;
using System.Reflection.Emit;
using Batibatlocation.Models;

namespace Batibatlocation.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext()
            : base("ArubaDB")
        {
        }

        public DbSet<Produit> Produits { get; set; }
        public DbSet<Periodicite> Periodicites { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<Accessoire> Accessoires { get; set; }
        public DbSet<ReservationAccessoire> ReservationAccessoires { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            // Configurazione della relazione uno-a-molti
            //modelBuilder.Entity<Produit>()
            //    .HasRequired(p => p.Category)
            //    .WithMany(c => c.Produits)
            //    .HasForeignKey(p => p.CategoryId);

            // Configurazione della relazione 1:N tra Produit e Periodicite
            modelBuilder.Entity<Produit>()
            .HasRequired(i => i.Periodicite)  // Ogni Produit deve avere una Periodicite
            .WithMany()
            .HasForeignKey(i => i.PeriodiciteId); // Definiamo la chiave esterna

            base.OnModelCreating(modelBuilder);

            // Configurazione della relazione 1:1 tra Produit e Reservation
            modelBuilder.Entity<Produit>()
                .HasRequired(e => e.Reservation)
                .WithRequiredPrincipal(r => r.Produit);

            //// Configurazione della relazione N:N tra Reservation e Accessoire
            modelBuilder.Entity<Reservation>()
                .HasMany(r => r.ReservationAccessoires)
                .WithRequired(ra => ra.Reservation)
                .HasForeignKey(ra => ra.ReservationId);

            modelBuilder.Entity<Accessoire>()
                .HasMany(a => a.ReservationAccessoires)
                .WithRequired(ra => ra.Accessoire)
                .HasForeignKey(ra => ra.AccessoireId);
        }
    }
}