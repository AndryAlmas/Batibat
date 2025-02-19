using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Batibatlocation.Models
{
    public class Echafaudage
    {
        public int Id { get; set; }
        public string Nom { get; set; }
        public string Description { get; set; }
        public decimal Prix { get; set; }
        public bool Disponible { get; set; }

        public string ImageUrl { get; set; }
        public string SpecificheTechniques { get; set; } // Nuova proprietà per le specifiche tecniche

        public virtual Reservation Reservation { get; set; }
    }
}