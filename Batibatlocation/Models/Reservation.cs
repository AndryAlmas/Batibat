using Batibatlocation.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Batibatlocation.Models
{
    public class Reservation
    {
        [Key]
        public int Id { get; set; }
        public DateTime DateDebut { get; set; }
        public DateTime DateFin { get; set; }
        [DefaultValue(false)]
        public bool isConfirmée { get; set; }

        public int? ClientId { get; set; }
        [ForeignKey("ClientId")]
        public virtual Client Client { get; set; }

        public int ProduitId { get; set; }
        [ForeignKey("ProduitId")]
        public virtual Produit Produit { get; set; }
        public virtual ICollection<Facture> Factures { get; set; }

        public virtual ICollection<ReservationAccessoire> ReservationAccessoires { get; set; }
        public virtual ICollection<Accessoire> Accessoires { get; set; }

    }
}