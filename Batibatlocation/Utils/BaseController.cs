using Batibatlocation.Data;
using Batibatlocation.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Batibatlocation.ViewModels;

namespace Batibatlocation.Utils
{
    public class BaseController : Controller
    {
        protected readonly ApplicationDbContext _context;
        protected static readonly string graphHopperApiKey = "d8982cc1-5af1-4e3d-87b6-911136fcd815";
        protected static readonly double latCoord_Rouvray = 47.893291;
        protected static readonly double lonCoord_Rouvray = 3.6687607;

        public BaseController(ApplicationDbContext context)
        {
            _context = context;
        }
        public static string EncodeDate(DateTime date)
        {
            // Calcola il giorno dell'anno (1-366)
            int dayOfYear = date.DayOfYear;

            // Offset per far partire le lettere da "FA"
            int offset = 5; // "F" è la lettera che corrisponde al 5° indice (A=0, B=1, ..., F=5)
            dayOfYear -= offset; // Ora il giorno 1 è rappresentato da "FA"

            // Codifica il giorno dell'anno in una combinazione di due lettere
            int firstLetterIndex = dayOfYear / 26;  // Prima lettera
            int secondLetterIndex = dayOfYear % 26; // Seconda lettera

            char firstLetter = (char)('A' + firstLetterIndex);
            char secondLetter = (char)('A' + secondLetterIndex);

            // Genera un numero a 4 cifre univoco dall'anno
            int yearCode = GenerateYearCode(date.Year);

            return $"{firstLetter}{secondLetter}{yearCode:D4}";
        }

        public static DateTime? DecodeDate(string code)
        {
            if (code.Length != 6)
                return null; // Codice non valido

            // Decodifica le lettere per ottenere il giorno dell'anno
            int firstLetterIndex = code[0] - 'A';
            int secondLetterIndex = code[1] - 'A';
            int dayOfYear = firstLetterIndex * 26 + secondLetterIndex;

            // Offset per "FA" come primo giorno
            int offset = 5;
            dayOfYear += offset; // Ripristina il giorno originale

            // Decodifica il numero nell'anno originale
            if (int.TryParse(code.Substring(2), out int year))
            {
                // Calcola il mese e il giorno dalla data
                DateTime date = new DateTime(year, 1, 1).AddDays(dayOfYear - 1);  // -1 per partire dal giorno 1
                return date;
            }

            return null; // Errore di conversione
        }

        public static int GenerateYearCode(int year)
        {
            return (year * 7) % 10000; // Codifica l'anno in un numero a 4 cifre
        }

        protected ActionResult TrackVisit(int prodId, double? lat, double? lon)
        {
            string deviceId = Request.Cookies["DeviceId"]?.Value;

            if (string.IsNullOrEmpty(deviceId))
            {
                deviceId = Guid.NewGuid().ToString();
                HttpCookie myCookie = new HttpCookie("DeviceId", deviceId)
                {
                    Expires = DateTime.Now.AddYears(1),
                    HttpOnly = true
                };
                Response.Cookies.Add(myCookie);
            }

            string ipAddress = Request.UserHostAddress;
            string localisation;
            int devisId;

            if (lat != null && lon != null)
            {
                localisation = $"GPS Lat: {lat}, GPS Lon: {lon}, Adresse: " + GetLocationFromCoordinates(lat,lon);
            }
            else
            {
                localisation = "IP Localisation: " + GetLocationFromIP(ipAddress);
            }

            var today = DateTime.Now.Date;

            var existingDevis = _context.Devis
                .Where(d => 
                    (d.CookieAdresse == deviceId || (d.Localisation == localisation && d.IPAdresse == ipAddress))
                    && DbFunctions.TruncateTime(d.DateConnection) == today
                    && d.ProdID == prodId).FirstOrDefault();

            if (existingDevis != null)
            {
                existingDevis.ConnectionsCount += 1;
                existingDevis.DateConnection = DateTime.Now;
                existingDevis.Localisation = localisation;
                devisId = existingDevis.Id;

            }
            else
            {
                var newDevis = new Models.Devis
                {
                    DateConnection = DateTime.Now,
                    IPAdresse = ipAddress,
                    Localisation = localisation,
                    ProdID = prodId,
                    CookieAdresse = deviceId,
                    ConnectionsCount = 1
                };
                _context.Devis.Add(newDevis);
                _context.SaveChanges();
                devisId = newDevis.Id;
            }

            _context.SaveChanges();
            return Json(new { success = true, location = localisation, devisId }, JsonRequestBehavior.AllowGet);
        }

        private string GetLocationFromCoordinates(double? latitude, double? longitude)
        {
            if (latitude is null && longitude is null || (latitude == 0 || longitude == 0)) return "Unknown";

            try
            {
                using (var client = new System.Net.Http.HttpClient())
                {
                    // Assicurati di formattare latitudine e longitudine con il punto come separatore decimale
                    string latFormatted = latitude.Value.ToString("0.######", System.Globalization.CultureInfo.InvariantCulture);
                    string lonFormatted = longitude.Value.ToString("0.######", System.Globalization.CultureInfo.InvariantCulture);

                    client.DefaultRequestHeaders.Add("User-Agent", "batibat");
                    string apiUrl = $"https://nominatim.openstreetmap.org/reverse?format=json&lat={latFormatted}&lon={lonFormatted}&addressdetails=1";
                    var response = client.GetStringAsync(apiUrl).Result;
                    dynamic result = Newtonsoft.Json.JsonConvert.DeserializeObject(response);

                    if (result != null && result.address != null)
                    {
                        // Estrai l'indirizzo formattato
                        string address = "";
                        address += result.address.house_number != null ? result.address.house_number : "";
                        address += result.address.road != null ? " " + result.address.road : "";
                        address += result.address.city != null ? ", " + result.address.city : "";
                        address += result.address.village != null ? ", " + result.address.village : "";
                        address += result.address.municipality != null ? ", " + result.address.municipality : "";
                        address += result.address.postcode != null ? " " + result.address.postcode : "";
                        address += result.address.country != null ? ", " + result.address.country : "";
                        return address;
                    }
                    else
                    {
                        return "Indirizzo non trovato";
                    }
                }
            }
            catch
            {
                return "Unknown";
            }
        }


        private string GetLocationFromIP(string ipAddress)
        {
            if (string.IsNullOrEmpty(ipAddress)) return "Unknown";

            try
            {
                using (var client = new System.Net.WebClient())
                {
                    string apiUrl = $"https://ipapi.co/{ipAddress}/json/";
                    string response = client.DownloadString(apiUrl);
                    dynamic result = Newtonsoft.Json.JsonConvert.DeserializeObject(response);
                    return $"{result.city}, {result.region}, {result.country_name}";
                }
            }
            catch
            {
                return "Unknown";
            }
        }

        // Azione per calcolare la distanza
        [HttpGet]
        protected async Task<ActionResult> CalculateDistance(double startLat, double startLon, double endLat, double endLon)
        {
            try
            {
                string startLatF = startLat.ToString("0.######", System.Globalization.CultureInfo.InvariantCulture);
                string startLonF = startLon.ToString("0.######", System.Globalization.CultureInfo.InvariantCulture);
                string endLatF = endLat.ToString("0.######", System.Globalization.CultureInfo.InvariantCulture);
                string endLonF = endLon.ToString("0.######", System.Globalization.CultureInfo.InvariantCulture);

                string url = $"https://graphhopper.com/api/1/route?point={startLatF},{startLonF}&point={endLatF},{endLonF}&vehicle=car&key={graphHopperApiKey}";

                using (var client = new HttpClient())
                {
                    var response = await client.GetStringAsync(url);
                    var result = JObject.Parse(response);

                    if (result["paths"].HasValues)
                    {
                        var distance = result["paths"][0]["distance"].Value<double>() / 1000; // Converti metri in chilometri
                        distance = Math.Round(distance, 0, MidpointRounding.AwayFromZero);
                        return Json(new { distance }, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { error = "Errore nel calcolo della distanza." }, JsonRequestBehavior.AllowGet);
        }

        protected async Task<int> CalculateDistanceKm(double startLat, double startLon, double endLat, double endLon)
        {
            try
            {
                string startLatF = startLat.ToString("0.######", System.Globalization.CultureInfo.InvariantCulture);
                string startLonF = startLon.ToString("0.######", System.Globalization.CultureInfo.InvariantCulture);
                string endLatF = endLat.ToString("0.######", System.Globalization.CultureInfo.InvariantCulture);
                string endLonF = endLon.ToString("0.######", System.Globalization.CultureInfo.InvariantCulture);

                string url = $"https://graphhopper.com/api/1/route?point={startLatF},{startLonF}&point={endLatF},{endLonF}&vehicle=car&key={graphHopperApiKey}";

                using (var client = new HttpClient())
                {
                    var response = await client.GetStringAsync(url);
                    var result = JObject.Parse(response);

                    if (result["paths"].HasValues)
                    {
                        var distance = result["paths"][0]["distance"].Value<double>() / 1000; // Converti metri in chilometri
                        distance = Math.Round(distance, 0, MidpointRounding.AwayFromZero);
                        return Convert.ToInt32(distance);
                    }
                }
            }
            catch (Exception)
            {
                return -1;
            }

            return -2;
        }
        protected async Task<string> GetLocalisationFromCoord(double Lat, double Lon)
        {
            try
            {
                string latitude = Lat.ToString("0.######", System.Globalization.CultureInfo.InvariantCulture);
                string longitude = Lon.ToString("0.######", System.Globalization.CultureInfo.InvariantCulture);

                string url = $"https://nominatim.openstreetmap.org/reverse?lat={latitude}&lon={longitude}&format=json&zoom=18&addressdetails=1";
                using (var client = new HttpClient())
                {
                    // Aggiungi un'intestazione per rispettare i termini di utilizzo di Nominatim
                    client.DefaultRequestHeaders.Add("User-Agent", "YourAppName");

                    // Effettua la richiesta HTTP GET
                    var response = await client.GetStringAsync(url);

                    // Analizza la risposta JSON
                    var result = JObject.Parse(response);

                    // Estrai il nome del luogo principale o l'indirizzo completo
                    string displayName = result["display_name"]?.ToString();

                    if (!string.IsNullOrEmpty(displayName))
                    {
                        return displayName; // Restituisci il nome completo del luogo
                    }
                    else
                    {
                        return "Luogo non trovato"; // Messaggio di fallback
                    }
                }
            }
            catch (Exception ex)
            {
                // Gestisci eventuali errori
                Console.WriteLine($"Errore durante la richiesta di reverse geocoding: {ex.Message}");
                return "Errore nella ricerca del luogo";
            }
        }
        public static int GetMonthsBetweenDates(DateTime startDate, DateTime endDate)
        {
            endDate.AddDays(1);
            int months = (endDate.Year - startDate.Year) * 12 + (endDate.Month - startDate.Month);
            if (endDate.Day < startDate.Day)
            {
                months--;
            }
            return months;
        }

        public static int GetFullWeeksBetweenDates(DateTime startDate, DateTime endDate)
        {
            // Calcola la differenza totale in giorni
            int totalDays = (int)(endDate - startDate).TotalDays+1;

            // Calcola il numero di settimane complete
            return totalDays / 7;
        }

        protected decimal CalculatePrixLivraison(double distance, int catID)
        {
            distance = distance * 4; // aller-retour
            var costi = _context.CostiLivraisons.ToList()
                .Where(c => c.CategoryId == catID
                        && ((c.DateDebutValidite ?? DateTime.Now) <= DateTime.Now
                        && (c.DateFinValidite ?? DateTime.Now) >= DateTime.Now))
                .FirstOrDefault();
            if (costi != null)
            {
                decimal prezzoPerDistanza = (costi.PrixAuKm ?? 0) * Convert.ToDecimal(distance);
                return Math.Max((costi.PrixMin ?? 0), prezzoPerDistanza);
            }
            return Math.Max(80, Convert.ToDecimal(distance)); // default nel caso in cui non trova la categoria o il prodotto
        }

        protected decimal CalculatePrice(string userType, int prodID, DateTime startDate, DateTime endDate, decimal priceLivraison)
        {
            var prodotto = _context.Produits.Where(p => p.Id == prodID).FirstOrDefault();
            if (prodotto != null)
            {
                int duration = 0;
                switch (prodotto.Periodicite.Id)
                {
                    case (int)Enum.PeriodicityType.Jour:
                        duration = (int)(endDate - startDate).TotalDays + 1;
                        break;
                    case (int)Enum.PeriodicityType.Semaine:
                        duration = GetFullWeeksBetweenDates(startDate, endDate);
                        break;
                    case (int)Enum.PeriodicityType.Mois:
                        duration = GetMonthsBetweenDates(startDate, endDate);
                        break;
                    default:
                        duration = 0;
                        break;
                }

                var prezzo = prodotto.Prix;

                if (prodotto.CategoryId == 14 && duration >= 7) // TODO: logica bloccante per questa tipologia Bétonnière
                    prezzo = 20;
                return (prezzo * duration + priceLivraison);

            }
            return -1;
        }

    }
}