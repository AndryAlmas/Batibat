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
                    string format = "dd/MM/yyyy";    // Formato specifico
                    DateTime startDate = DateTime.ParseExact(StartDate, format, CultureInfo.InvariantCulture);
                    DateTime endDate = DateTime.ParseExact(EndDate, format, CultureInfo.InvariantCulture);
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
                    double price = CalculatePrice(userType, prodID, startDate, endDate, distance);

                    // Restituisci il risultato
                    return Json(new { distance, price });
                }
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }
        private double CalculatePrice(string userType, int prodID, DateTime startDate, DateTime endDate, double distance)
        {
            // Implementa la logica di calcolo del prezzo in base al tipo di utente, durata e distanza
            double basePrice = userType == "private" ? 50 : 100; // Prezzo base per privati o professionisti
            double durationDays = (endDate - startDate).TotalDays;
            double pricePerDay = 10; // Prezzo giornaliero
            double deliveryCost = distance * 0.5; // Costo di consegna per chilometro

            return basePrice + (pricePerDay * durationDays) + deliveryCost;
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