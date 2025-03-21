using System.Collections.Generic;
using System;
using System.Linq;
using System.Net.Mail;
using System.Web.Mvc;
using Batibatlocation.Data;
using Batibatlocation.Models;
using System.IO;
using WebGrease.Css.Extensions;
using Batibatlocation.Helpers;

namespace Batibatlocation.Controllers
{
    public class ProduitController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProduitController(ApplicationDbContext context)
        {
            _context = context;
        }

        [Obsolete]
        public ActionResult Detail(int id)
        {
            // Cerca il prodotto tramite ID
            var produit = _context.Produits.FirstOrDefault(p => p.Id == id);
            if (produit == null)
            {
                return HttpNotFound();
            }

            // Genera lo slug corretto
            string slug = SlugName.GenerateSlug(id, produit.Nom);

            // Redirigi verso il nuovo URL con lo slug
            return RedirectToActionPermanent("Details", "Produit", new { slug });
        }

        public ActionResult Details(string slug)
        {
            if (string.IsNullOrEmpty(slug))
            {
                return HttpNotFound();
            }

            // Cerca il prodotto tramite slug
            var produit = _context.Produits.ToList().FirstOrDefault(p => SlugName.GenerateSlug(p.Id, p.Nom) == slug);
            if (produit == null)
            {
                return HttpNotFound();
            }

            var imageUrl = produit.ImageUrl.Split('/').LastOrDefault().Split('.').FirstOrDefault();
            string folderPath = Server.MapPath("~/Content/Images/Produits/SlideGallery/" + imageUrl + "/");

            // Leggi tutti i file nella cartella
            string[] imagePaths = Directory.GetFiles(folderPath); // Ottiene i percorsi completi dei file

            for (int i = 0; i < imagePaths.Length; i++)
            {
                var nomeImg = imagePaths[i].Split('\\').LastOrDefault();
                imagePaths[i] = "~/Content/Images/Produits/SlideGallery/" + imageUrl + "/" + nomeImg;
            }
            // Passa i percorsi alla vista tramite ViewBag
            ViewBag.ImagePaths = imagePaths;

            // Restituisci la vista con il prodotto
            return View(produit);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _context.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}