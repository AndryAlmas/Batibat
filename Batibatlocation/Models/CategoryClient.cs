using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Batibatlocation.Models
{
    public class CategoryClient
    {
        [Key]
        public int Id { get; set; }
        [Required(ErrorMessage = "Le Nom de la catégorie est requis.")]
        public string Nom { get; set; }


        // Proprietà di navigazione per la relazione uno-a-molti
        public virtual ICollection<Client> Clients { get; set; }
    }

}