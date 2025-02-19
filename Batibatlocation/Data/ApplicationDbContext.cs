using System.Data.Entity;
using Batibatlocation.Models;

namespace Batibatlocation.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext()
            : base("ArubaDB")
        {
        }

        public DbSet<Echafaudage> Echafaudages { get; set; }
        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<Accessoire> Accessoires { get; set; }
        public DbSet<ReservationAccessoire> ReservationAccessoires { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configurazione della relazione 1:1 tra Echafaudage e Reservation
            modelBuilder.Entity<Echafaudage>()
                .HasRequired(e => e.Reservation)
                .WithRequiredPrincipal(r => r.Echafaudage);

            // Configurazione della relazione N:N tra Reservation e Accessoire
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