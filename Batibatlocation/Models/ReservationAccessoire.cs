using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Batibatlocation.Models
{
    [Table("ReservationAccessoires")]
    public class ReservationAccessoire
    {
        [Key, Column(Order = 0)]
        public int ReservationId { get; set; }

        [Key, Column(Order = 1)]
        public int AccessoireId { get; set; }

        public int Quantite { get; set; } // Proprietà per la quantità

        public virtual Reservation Reservation { get; set; }
        public virtual Accessoire Accessoire { get; set; }
    }
}