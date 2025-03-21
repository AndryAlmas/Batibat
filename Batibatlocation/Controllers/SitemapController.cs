using Batibatlocation.Data;
using Batibatlocation.Helpers;
using Batibatlocation.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml.Linq;

namespace Batibatlocation.Controllers
{
    public class SitemapController : BaseController
    {
        public SitemapController(ApplicationDbContext context) : base(context)
        {
        }

        // GET: Sitemap
        [AllowAnonymous]
        public ActionResult Index()
        {
            // Recupera tutti i prodotti dal database
            var produits = _context.Produits.ToList();

            // Crea l'XML del sitemap
            var sitemap = new XDocument(
                new XElement("{http://www.sitemaps.org/schemas/sitemap/0.9}urlset", // Usa il namespace direttamente nel nome del tag
                                                                                    // Aggiungi la home page
                    new XElement("{http://www.sitemaps.org/schemas/sitemap/0.9}url",
                        new XElement("{http://www.sitemaps.org/schemas/sitemap/0.9}loc", "https://www.batibatlocation.com/"),
                        new XElement("{http://www.sitemaps.org/schemas/sitemap/0.9}lastmod", DateTime.UtcNow.ToString("yyyy-MM-dd")),
                        new XElement("{http://www.sitemaps.org/schemas/sitemap/0.9}changefreq", "daily"),
                        new XElement("{http://www.sitemaps.org/schemas/sitemap/0.9}priority", "1.0")
                    ),

                    // Aggiungi i dettagli dei prodotti
                    from produit in produits
                    select new XElement("{http://www.sitemaps.org/schemas/sitemap/0.9}url",
                        new XElement("{http://www.sitemaps.org/schemas/sitemap/0.9}loc", $"https://www.batibatlocation.com/Produit/{SlugName.GenerateSlug(produit.Id, produit.Nom)}"),
                        new XElement("{http://www.sitemaps.org/schemas/sitemap/0.9}lastmod", produit.LastMod.ToString("yyyy-MM-dd")),
                        new XElement("{http://www.sitemaps.org/schemas/sitemap/0.9}changefreq", "weekly"),
                        new XElement("{http://www.sitemaps.org/schemas/sitemap/0.9}priority", "0.8")
                    )
                )
            );
            // Restituisci il sitemap come XML
            return Content(sitemap.ToString(), "application/xml");
        }
    }
}