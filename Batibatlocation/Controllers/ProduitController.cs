using System.Collections.Generic;
using System;
using System.Linq;
using System.Net.Mail;
using System.Web.Mvc;
using Batibatlocation.Data;
using Batibatlocation.Models;
using System.IO;
using WebGrease.Css.Extensions;

namespace Batibatlocation.Controllers
{
    public class ProduitController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProduitController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Produit/Details/{id}
        public ActionResult Details(int id)
        {
            var produit = _context.Produits.Where(e => e.Visible && e.Id == id).SingleOrDefault();
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