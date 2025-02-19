using Batibatlocation.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Batibatlocation.Models
{
    public class Reservation
    {
        public int Id { get; set; }
        public string Nom { get; set; }
        public string Email { get; set; }
        public string Telephone { get; set; }
        public DateTime DateDebut { get; set; }
        public DateTime DateFin { get; set; }

        // Relazione 1:1 con Echafaudage
        public int EchafaudageId { get; set; }
        public virtual Echafaudage Echafaudage { get; set; }

        // Relazione N:N con Accessoire
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ReservationAccessoire> ReservationAccessoires { get; set; } = new HashSet<ReservationAccessoire>();

        // Proprietà di navigazione per Accessoires
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Accessoire> Accessoires { get; set; } = new HashSet<Accessoire>();

    }
}