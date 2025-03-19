using Batibatlocation.Data;
using Batibatlocation.Utils;
using Microsoft.Ajax.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Batibatlocation.Controllers
{
    public class DevisController : BaseController
    {
        public DevisController(ApplicationDbContext context) : base(context)
        {
        }
        private static bool _localisation = false;
        // GET: Devis
        public ActionResult Index(int prodId)
        {
            if (_localisation is true)
            {
                ViewBag.Localisation = true;
                ViewBag.ProdID = prodId;
                _localisation = false;
            }

            return View();
        }

        // POST: Devis
        [HttpPost]
        public ActionResult TrackVisit(int prodId, bool localisation)
        {
            if (localisation is true)
                _localisation = true;
                
            return RedirectToAction("Index", new { prodId });
        }
        public new ActionResult TrackVisit(int prodId, double? lat, double? lon)
        {
            return base.TrackVisit(prodId, lat, lon);
        }
    }
}