using Batibatlocation.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Batibatlocation.Models
{
    public class Facture
    {
        [Key]
        [ForeignKey("Reservation")]
        public int Id { get; set; }

        // Progressivo della fattura
        public int Progressivo { get; set; }

        // Anno della fattura
        public int Anno { get; set; }

        // NumeroFattura (combinazione di Progressivo/Anno)
        public string NumeroFattura => $"{Progressivo}/{Anno}";

        // Data di emissione della fattura
        public DateTime DataEmissione { get; set; }

        // Importo totale della fattura
        public decimal Cauzione { get; set; }
        public decimal ImportoTotale { get; set; }

        // Note opzionali
        public string Note { get; set; }

        public int EtatFactureId { get; set; } // Chiave esterna verso StatiFattura

        [ForeignKey("EtatFactureId")]
        public virtual EtatFacture EtatFacture { get; set; }
        public virtual Reservation Reservation { get; set; }
    }
}