using Batibatlocation.Models;
using System.Collections.Generic;

namespace Batibatlocation.Models
{
    public class Accessoire
    {
        public int Id { get; set; }
        public string Nom { get; set; }
        public string Description { get; set; }
        public decimal Prix { get; set; }
        public int Quantite { get; set; } // Proprietà per la quantità

        // Relazione N:N con Reservation
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ReservationAccessoire> ReservationAccessoires { get; set; } = new HashSet<ReservationAccessoire>();

        // Proprietà di navigazione per Reservations
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Reservation> Reservations { get; set; } = new HashSet<Reservation>();


    }
}