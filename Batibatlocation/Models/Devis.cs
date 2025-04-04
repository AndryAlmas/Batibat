using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Batibatlocation.Models
{
    public class Devis
    {
        // Chiave primaria
        [Key]
        public int Id { get; set; }

        // Data di connessione
        public DateTime DateConnection { get; set; }

        // Indirizzo IP
        [MaxLength(50)] // Limita la lunghezza dell'indirizzo IP
        public string IPAdresse { get; set; }

        // Localizzazione
        [MaxLength(255)] // Limita la lunghezza della localizzazione
        public string Localisation { get; set; }

        // Chiave esterna verso la classe Produit
        public int ProdID { get; set; }

        // Proprietà di navigazione verso la classe Produit
        [ForeignKey("ProdID")]
        public virtual Produit Produit { get; set; }

        [MaxLength(255)] // Limita la lunghezza dell'email
        public string CookieAdresse { get; set; }

        // Indirizzo email
        [MaxLength(255)] // Limita la lunghezza dell'email
        public string AdresseEmail { get; set; }

        [MaxLength(15)] // Limita la lunghezza dell'email
        public string Telephone { get; set; }

        // Contatore connessioni
        public int ConnectionsCount { get; set; }

        // Chiave esterna verso la classe Produit
        public int? DevisProdID { get; set; }

        // Proprietà di navigazione verso la classe Produit
        [ForeignKey("DevisProdID")]
        public virtual Produit DevisProduit { get; set; }

        // Contatore richieste Devis
        public int DevisCount { get; set; }
        public bool DemandeDeReserver { get; set; }
    }
}