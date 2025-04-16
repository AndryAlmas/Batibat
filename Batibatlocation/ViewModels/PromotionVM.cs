using Batibatlocation.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using Batibatlocation.Helpers;

namespace Batibatlocation.ViewModels
{
    [AtLeastOneRequired(nameof(CategoryId), nameof(ProductId), ErrorMessage = "Vous devez sélectionner une catégorie ou un produit.")]
    public class PromotionVM
    {
        public int Id { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Vous devez sélectionner une catégorie.")]
        public int? CategoryId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Vous devez sélectionner un produit.")]
        public int? ProductId { get; set; }

        [Required(ErrorMessage = "La date de début est requise.")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "La date de fin est requise.")]
        public DateTime EndDate { get; set; }

        [Required(ErrorMessage = "Le montant de la réduction est requis.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Le montant de la réduction doit être supérieur à 0.")]
        public decimal DiscountValue { get; set; }

        public bool IsPercentage { get; set; }
        public List<SelectListItem> Categories { get; set; } // Lista delle categorie disponibili
        public List<SelectListItem> Products { get; set; } // Lista dei prodotti disponibili
    }
}