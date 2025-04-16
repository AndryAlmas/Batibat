using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Batibatlocation.Models
{
	public class Promotion
	{
        [Key]
        public int Id { get; set; }
        public int? CategoryId { get; set; } // ID della categoria (opzionale)
        [ForeignKey("CategoryId")]
        public virtual Category Category { get; set; }
        public int? ProductId { get; set; } // ID del prodotto (opzionale)
        [ForeignKey("ProductId")]
        public virtual Produit Produit { get; set; }
        public DateTime? StartDate { get; set; } // Data di inizio della promozione
        public DateTime? EndDate { get; set; } // Data di fine della promozione
        public decimal DiscountValue { get; set; } // Valore dello sconto
        public bool IsPercentage { get; set; } // True = percentuale, False = importo fisso
    }
}