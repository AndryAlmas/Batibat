using Batibatlocation.Data;
using Batibatlocation.Models;
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

namespace Batibatlocation.Utils
{
    public class BaseController : Controller
    {
        protected readonly ApplicationDbContext _context;

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
            }

            _context.SaveChanges();
            return Json(new { success = true, location = localisation }, JsonRequestBehavior.AllowGet);
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
    }
}