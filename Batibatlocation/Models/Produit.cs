using Batibatlocation.Enum;
using Batibatlocation.Filters;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Batibatlocation.Models
{
    public class Produit
    {
        [Key]
        public int Id { get; set; }
        [Required(ErrorMessage = "Le nom est requis.")]
        [StringLength(100)]
        public string Nom { get; set; }

        [Required(ErrorMessage = "La description est requise.")]
        [StringLength(500)]
        public string Description { get; set; }

        [Required(ErrorMessage = "Le prix est requis.")]
        [DataType(DataType.Currency)]
        public decimal Prix { get; set; }

        [Required(ErrorMessage = "Le statut de visibilité est requis.")]
        public bool Visible { get; set; }

        [StringLength(255)]
        public string ImageUrl { get; set; }

        [Required(ErrorMessage = "Les spécifications techniques sont requises.")]
        [StringLength(500)]
        public string SpecifiquesTechniques { get; set; }
        public DateTime LastMod { get; internal set; }

        [Required(ErrorMessage = "La périodicité est requise.")]
        public int PeriodiciteId { get; set; } 

        [ForeignKey("PeriodiciteId")]
        public virtual Periodicite Periodicite { get; set; }

        [Required(ErrorMessage = "La category est requise.")]
        public int CategoryId { get; set; }

        [ForeignKey("CategoryId")]
        public virtual Category Category { get; set; }

        public virtual ICollection<Reservation> Reservations { get; set; }
    }
}