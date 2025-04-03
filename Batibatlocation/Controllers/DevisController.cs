using Batibatlocation.Data;
using Batibatlocation.Utils;
using Batibatlocation.ViewModels;
using Microsoft.Ajax.Utilities;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
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
                _localisation = false;
            }
            var prod = _context.Produits.Where(p => p.Id == prodId).FirstOrDefault();

            ViewBag.ProdID = prodId;
            ViewBag.ProdName = prod.Nom;
            ViewBag.ProdCat = prod.Category.Nom;


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

        [HttpPost]
        public async Task<JsonResult> CalculateDevis()
        {
            try
            {
                // Recupera i dati dalla richiesta AJAX
                var request = Request.InputStream;
                using (var reader = new System.IO.StreamReader(request))
                {
                    var json = reader.ReadToEnd();
                    var data = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(json);

                    string userType = data.UserType;
                    int prodID = Convert.ToInt32(data.ProdID);
                    string name = data.Name;
                    string email = data.Email;
                    string StartDate = data.StartDate; // Data in formato gg/mm/yyyy
                    string EndDate = data.EndDate; // Data in formato gg/mm/yyyy
                    DateTime startDate = DateTime.Parse(StartDate);
                    DateTime endDate = DateTime.Parse(EndDate);
                    bool deliveryEnabled = data.DeliveryEnabled;
                    double startLat = latCoord_Rouvray;
                    double startLon = lonCoord_Rouvray;
                    double endLat = deliveryEnabled ? Convert.ToDouble(data.EndLat) : 0;
                    double endLon = deliveryEnabled ? Convert.ToDouble(data.EndLon) : 0;

                    // Calcolo della distanza (esempio fittizio)
                    double distance = 0;
                    if (deliveryEnabled)
                    {
                        // Usa un servizio di routing come GraphHopper o OSRM per calcolare la distanza
                        distance = await CalculateDistanceKm(startLat, startLon, endLat, endLon);
                    }

                    // Calcolo del prezzo (esempio fittizio)
                    int catID = _context.Produits.Where(p => p.Id == prodID).FirstOrDefault().CategoryId;
                    decimal priceLivraison = 0; 
                    if (deliveryEnabled)
                        priceLivraison = CalculatePrixLivraison(distance, catID); 
                    decimal priceTotal = CalculatePrice(userType, prodID, startDate, endDate, priceLivraison);

                    // Restituisci il risultato
                    return Json(new { priceLivraison, priceTotal });
                }
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }
        private new async Task<ActionResult> CalculateDistance(double startLat, double startLon, double endLat, double endLon)
        {
            return await base.CalculateDistance(startLat, startLon, endLat, endLon);
        }

        [HttpPost]
        public ActionResult Reserve([Bind(Include = "ProdName,UserType,Name,Email,StartDate,EndDate,DeliveryEnabled,EndLat,EndLon")] ReservationVM model)
        {
            if (ModelState.IsValid)
            {
                // Salva i dati nel database o esegui altre operazioni
                return Json(new { success = true, message = "Réservation réussie!" });
            }

            return Json(new { success = false, message = "Erreur lors de la réservation." });
        }
    }
}