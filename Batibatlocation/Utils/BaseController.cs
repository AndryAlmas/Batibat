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
        protected string GenerateUniqueCode(DateTime date)
        {
            // Trasforma la data in formato "ddMMyyyy"
            string dateString = date.ToString("ddMMyyyy");

            // Converti la data in un hash SHA256
            string hash = GetHash(dateString).ToUpper();

            // Estrai le prime due lettere dall'hash
            string firstTwoLetters = new string(hash.Where(char.IsLetter).Take(2).ToArray());

            // Estrai i primi quattro numeri dall'hash
            string lastFourNumbers = new string(hash.Where(char.IsDigit).Take(4).ToArray());

            // Se mancano lettere o numeri, rimpiazza con valori di default
            firstTwoLetters = firstTwoLetters.PadRight(2, 'X'); // "X" se mancano lettere
            lastFourNumbers = lastFourNumbers.PadRight(4, '0'); // "0" se mancano numeri

            return firstTwoLetters + lastFourNumbers;
        }

        static string GetHash(string dateTime)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(dateTime));
                return BitConverter.ToString(hashBytes).Replace("-", "").Substring(0, 16);
            }
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