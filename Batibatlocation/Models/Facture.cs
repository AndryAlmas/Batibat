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
        public int Id { get; set; }

        // Progressivo della fattura
        public int ProgressivoAnnuo { get; set; }

        // Anno della fattura
        public int Anno { get; set; }

        // NumeroFattura (funzione GenerateUniqueCode)
        public string NumeroFattura { get; set; }

        // Data di emissione della fattura
        public DateTime DataEmissione { get; set; }

        // Importo totale della fattura
        public decimal Caution { get; set; }
        public decimal TotaleTTC { get; set; }
        public decimal RemiseEuro { get; set; }
        [Range(0,100)]
        public decimal RemisePercentage { get; set; }

        // Note opzionali
        public string Note { get; set; }

        public int IdReserevation { get; set; } // Chiave esterna verso Reservation
        public int EtatFactureId { get; set; } // Chiave esterna verso StatiFattura

        [ForeignKey("EtatFactureId")]
        public virtual EtatFacture EtatFacture { get; set; }

        [ForeignKey("IdReserevation")]
        public virtual Reservation Reservation { get; set; }
    }
}