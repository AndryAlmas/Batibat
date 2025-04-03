using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Batibatlocation.Models
{
    public class CostiLivraison
    {
        [Key]
        public int ID { get; set; }
        public int CategoryId { get; set; }
        [ForeignKey("CategoryId")]
        public virtual Category Category { get; set; }

        public DateTime? DateDebutValidite { get; set; }
        public DateTime? DateFinValidite { get; set; }
        public decimal? PrixMin { get; set; }
        public decimal? PrixAuKm { get; set; }

    }
}