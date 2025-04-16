using Batibatlocation.Data;
using Batibatlocation.Models;
using Batibatlocation.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Batibatlocation.Controllers
{
    public class HomeController : BaseController
    {
        public HomeController(ApplicationDbContext context) : base(context)
        {
        }
        public ActionResult Index()
        {
            var produits = _context.Produits.Where(e=>e.Visible).ToList();
            var promotionList = new List<Promotion>();
            foreach(var prodotto in produits)
            {
                var promo = GetActivePromotion(prodotto);
                if(!promotionList.Contains(promo) && promo != null)
                    promotionList.Add(promo);
            }
            ViewBag.PromotionList = promotionList;
            return View(produits);
        }

        public ActionResult Reservation()
        {
            // Logic for reservation page can be added here
            return View();
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