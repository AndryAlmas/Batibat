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

        public DbSet<Models.Produit> Produits { get; set; }
        public DbSet<Models.Client> Clients { get; set; }
        public DbSet<Models.Particulier> Particuliers { get; set; }
        public DbSet<Models.Professionnel> Professionnels { get; set; }
        public DbSet<Models.Document> Documents { get; set; }
        public DbSet<Models.Periodicite> Periodicites { get; set; }
        public DbSet<Models.Category> Categories { get; set; }
        public DbSet<Models.CategoryDocument> CategoriesDocuments { get; set; }
        public DbSet<Models.CategoryClient> CategoriesClients { get; set; }
        public DbSet<Models.EtatFacture> EtatFactures { get; set; }
        public DbSet<Models.Reservation> Reservations { get; set; }
        public DbSet<Models.Devis> Devis { get; set; }
        public DbSet<Models.Accessoire> Accessoires { get; set; }
        public DbSet<Models.ReservationAccessoire> ReservationAccessoires { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Client>()
                        .Map<Particulier>(m => m.Requires("Discriminator").HasValue("Particulier"))  // 👈 Usa una colonna "Discriminator"
                        .Map<Professionnel>(m => m.Requires("Discriminator").HasValue("Professionnel"));


            //// Configurazione della relazione 1:1 tra Client e Document
            //modelBuilder.Entity<Client>()
            //.HasRequired(i => i.Document)  
            //.WithMany()
            //.HasForeignKey(c => c.DocumentId); // Chiave esterna su Client

            //// Configurazione della relazione 1:N tra Produit e Periodicite
            //modelBuilder.Entity<Produit>()
            //.HasRequired(i => i.Periodicite)  // Ogni Produit deve avere una Periodicite
            //.WithMany()
            //.HasForeignKey(i => i.PeriodiciteId); // Definiamo la chiave esterna

            // Configura la relazione tra Client e Reservation
            modelBuilder.Entity<Reservation>()
                .HasOptional(r => r.Client)  // 🔹 HasOptional perché ClientId è nullable
                .WithMany(c => c.Reservations)  // 🔹 Un Client può avere più Reservation
                .HasForeignKey(r => r.ClientId)  // 🔹 Chiave esterna
                .WillCascadeOnDelete(false);  // 🚫 Disattiva l'eliminazione a cascata

            //// Configurazione della relazione 1:1 tra Produit e Reservation
            //modelBuilder.Entity<Produit>()
            //    .HasRequired(e => e.Reservation)
            //    .WithRequiredPrincipal(r => r.Produit);

            //// Configurazione della relazione N:N tra Reservation e Accessoire
            modelBuilder.Entity<Reservation>()
                .HasMany(r => r.ReservationAccessoires)
                .WithRequired(ra => ra.Reservation)
                .HasForeignKey(ra => ra.ReservationId);

            modelBuilder.Entity<Accessoire>()
                .HasMany(a => a.ReservationAccessoires)
                .WithRequired(ra => ra.Accessoire)
                .HasForeignKey(ra => ra.AccessoireId);

            base.OnModelCreating(modelBuilder);
        }
    }
}