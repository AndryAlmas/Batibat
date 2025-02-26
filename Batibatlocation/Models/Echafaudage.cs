using Batibatlocation.Enum;
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
        public bool Visible { get; set; }
        public string ImageUrl { get; set; }
        public string SpecifiquesTechniques { get; set; }

        public virtual Reservation Reservation { get; set; }
        public int PeriodiciteId { get; set; }  // Chiave esterna
        public virtual Periodicite Periodicite { get; set; }
    }
}