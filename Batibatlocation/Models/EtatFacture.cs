using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Batibatlocation.Models
{
    public class EtatFacture
    {
        [Key]
        public int Id { get; set; }
        [Required(ErrorMessage = "Le Nom est requis.")]
        public string Nom { get; set; }


        // Proprietà di navigazione per la relazione uno-a-molti
        public virtual ICollection<Facture> Factures { get; set; }
    }

}