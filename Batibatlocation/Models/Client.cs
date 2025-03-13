using Batibatlocation.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Batibatlocation.Models
{
    public abstract class Client
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Email { get; set; }

        [Required]
        public string Telephone { get; set; }

        // Nuove proprietà
        [Required]
        public string AdresseChantier { get; set; }
        [Required]
        public string AdresseDomiciliation { get; set; }

        // Relazione con CategoryClient
        public int CategoryClientId { get; set; }

        [ForeignKey("CategoryClientId")]
        public virtual CategoryClient CategoryClient { get; set; }

        public virtual Document Document { get; set; }



        // 1 a molti
        public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();


    }

    public class Particulier : Client
    {
        [Required]
        public string Nom { get; set; }

        [Required]
        public string Prenom { get; set; }
    }

    public class Professionnel : Client
    {
        [Required]
        public string NomEntreprise { get; set; }

        [Required]
        public string AdresseEntreprise { get; set; }

        [Required]
        public string NumeroKBISS { get; set; }
    }

}