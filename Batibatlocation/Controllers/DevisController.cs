using Batibatlocation.Data;
using Batibatlocation.Models;
using Batibatlocation.Utils;
using Batibatlocation.ViewModels;
using Microsoft.Ajax.Utilities;
using MimeKit;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using static System.Net.WebRequestMethods;

namespace Batibatlocation.Controllers
{
    public class DevisController : BaseController
    {
        public DevisController(ApplicationDbContext context) : base(context)
        {
        }

        // GET: Devis
        public ActionResult Index(int prodId)
        {
            var prod = _context.Produits.Where(p => p.Id == prodId).FirstOrDefault();

            ViewBag.ProdID = prodId;
            ViewBag.ProdName = prod.Nom;
            ViewBag.ProdCat = prod.Category.Nom;
            ViewBag.ProdPeriod = prod.Periodicite.Nom;

            return View();
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
                    string devisId = data.DevisId;
                    string telephone = data.Telephone;
                    string StartDate = data.StartDate; // Data in formato gg/mm/yyyy
                    string EndDate = data.EndDate; // Data in formato gg/mm/yyyy
                    DateTime startDate = DateTime.Parse(StartDate);
                    DateTime endDate = DateTime.Parse(EndDate);
                    bool deliveryEnabled = data.DeliveryEnabled;
                    double startLat = latCoord_Rouvray;
                    double startLon = lonCoord_Rouvray;
                    double endLat = deliveryEnabled ? Convert.ToDouble(data.EndLat) : 0;
                    double endLon = deliveryEnabled ? Convert.ToDouble(data.EndLon) : 0;

                    //Registra info sul preventivo:
                    #region Devis Save Info

                    if (!String.IsNullOrEmpty(devisId))
                    {
                        int id = Convert.ToInt32(devisId);
                        var devis = _context.Devis.Where(d => d.Id == id).FirstOrDefault();
                        if (devis != null)
                        {
                            if(devis.DevisCount >= 5)
                            {
                                return Json(new { error = "Vous avez atteint le nombre maximum de devis pour ce produit pour aujourd'hui !" });
                            }
                            else
                            {
                                devis.AdresseEmail = email;
                                devis.Telephone = telephone;
                                devis.DevisProdID = prodID;
                                devis.DevisCount = devis.DevisCount + 1;
                                devis.ClientCatID = userType == "private" ? 1 : 2;
                                devis.NomClient = name;
                                _context.SaveChanges();
                            }
                        }
                    }
                    #endregion

                    //return Json(new { error = ex.Message });


                    // Calcolo della distanza (esempio fittizio)
                    double distance = 0;
                    string placeName = "";
                    if (deliveryEnabled)
                    {
                        // Usa un servizio di routing come GraphHopper o OSRM per calcolare la distanza
                        distance = await CalculateDistanceKm(startLat, startLon, endLat, endLon);
                        placeName = await GetLocalisationFromCoord(endLat, endLon);
                        
                    }

                    // Calcolo del prezzo (esempio fittizio)
                    int catID = _context.Produits.Where(p => p.Id == prodID).FirstOrDefault().CategoryId;
                    decimal priceLivraison = 0; 
                    if (deliveryEnabled)
                        priceLivraison = CalculatePrixLivraison(distance, catID); 
                    decimal priceTotal = CalculatePrice(userType, prodID, startDate, endDate, priceLivraison);

                    // Restituisci il risultato
                    return Json(new { priceLivraison, priceTotal, placeName });
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
        public ActionResult Reserve([Bind(Include = "ProdName,UserType,Name,Email,StartDate,EndDate,DeliveryEnabled,EndLat,EndLon,Localisation,DevisId,Telephone,PrixLivraison,PrixTotal")] ReservationVM model)
        {
            if (ModelState.IsValid)
            {
                #region Salva flag Reserver
                if (!String.IsNullOrEmpty(model.DevisId))
                {
                    int id = Convert.ToInt32(model.DevisId);
                    var devis = _context.Devis.Where(d => d.Id == id).FirstOrDefault();
                    if (devis != null)
                    {
                        devis.DemandeDeReserver = true;
                        _context.SaveChanges();                        
                    }
                }
                #endregion

                #region Invia mail
                // Ottieni i parametri SMTP dal file di configurazione
                string adminEmail = ConfigurationManager.AppSettings["AdminEmail"];
                string smtpHost = ConfigurationManager.AppSettings["SmtpHost"];
                int smtpPort = int.Parse(ConfigurationManager.AppSettings["SmtpPort"]);
                string smtpUserEmail = ConfigurationManager.AppSettings["SmtpUser"];
                string smtpName = ConfigurationManager.AppSettings["SmtpName"];
                string smtpPassword = ConfigurationManager.AppSettings["SmtpPassword"]; // Utilizza la tua password o App Password

                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(smtpName, smtpUserEmail));
                message.To.Add(new MailboxAddress(model.Name, model.Email));
                message.Bcc.Add(new MailboxAddress("Batibat", adminEmail)); // Aggiungi in copia nascosta
                message.Subject = "Demande de Réservation sur le site Batibatlocation.com";

                //// Corpo della mail con il codice OTP
                StringBuilder bodyBuilder = new StringBuilder();
                // Intestazione e informazioni generali
                bodyBuilder.AppendLine($"Bonjour,");
                bodyBuilder.AppendLine($"Le {DateTime.Now:dd/MM/yyyy HH:mm}, vous avez effectué une demande de réservation sur le site Batibatlocation.com.");
                bodyBuilder.AppendLine($"<br>");
                bodyBuilder.AppendLine($"Ci-dessous les détails de la demande de réservation :");
                bodyBuilder.AppendLine($"<br>");
                bodyBuilder.AppendLine($"<br>");

                // Inizio della tabella
                bodyBuilder.AppendLine("<table border='1' cellpadding='5' cellspacing='0' style='border-collapse: collapse; width: 100%;'>");

                // Dettagli del prodotto
                bodyBuilder.AppendLine("<tr><th colspan='2' style='background-color: #f0f0f0; text-align: center;'>Détails de la réservation</th></tr>");
                bodyBuilder.AppendLine($"<tr><th>Produit</th><td>{model.ProdName}</td></tr>");
                bodyBuilder.AppendLine($"<tr><th>Nom</th><td>{model.Name}</td></tr>");
                bodyBuilder.AppendLine($"<tr><th>Téléphone</th><td>{model.Telephone}</td></tr>");
                bodyBuilder.AppendLine($"<tr><th>E-mail</th><td>{model.Email}</td></tr>");
                bodyBuilder.AppendLine($"<tr><th>Date de début de location</th><td>{model.StartDate}</td></tr>");
                bodyBuilder.AppendLine($"<tr><th>Date de fin de location</th><td>{model.EndDate}</td></tr>");

                // Gestione della consegna
                if (model.DeliveryEnabled == true)
                {
                    bodyBuilder.AppendLine($"<tr><th>Avec livraison</th><td>Oui</td></tr>");
                    if (!string.IsNullOrEmpty(model.Localisation))
                    {
                        bodyBuilder.AppendLine($"<tr><th>Adresse de livraison</th><td>{model.Localisation}</td></tr>");
                    }
                    else
                    {
                        bodyBuilder.AppendLine($"<tr><th>Adresse de livraison</th><td>Non spécifiée</td></tr>");
                    }
                }
                else
                {
                    bodyBuilder.AppendLine($"<tr><th>Avec livraison</th><td>Non</td></tr>");
                }

                // Gestione dei prezzi
                if (model.DeliveryEnabled == true && !string.IsNullOrEmpty(model.PrixLivraison))
                {
                    bodyBuilder.AppendLine($"<tr><th>Prix de la livraison</th><td>{model.PrixLivraison}</td></tr>");
                }
                bodyBuilder.AppendLine($"<tr><th>Prix total</th><td>{model.PrixTotal}</td></tr>");

                // Fine della tabella
                bodyBuilder.AppendLine("</table>");

                // Informazioni finali
                bodyBuilder.AppendLine($"<br>");
                bodyBuilder.AppendLine($"<br>");
                bodyBuilder.AppendLine($"Vous serez contacté(e) dès que possible par notre équipe pour <b><u>confirmer</u></b> la réservation.");
                bodyBuilder.AppendLine($"<br>");
                bodyBuilder.AppendLine($"<br>");
                bodyBuilder.AppendLine($"Cordialement,");
                bodyBuilder.AppendLine($"<br>");
                bodyBuilder.AppendLine($"Équipe Bati'Bat");

                message.Body = new TextPart("html") { Text = bodyBuilder.ToString() };

                using (var client = new MailKit.Net.Smtp.SmtpClient())
                {
                    client.Connect(smtpHost, smtpPort, true);

                    // Note: only needed if the SMTP server requires authentication
                    client.Authenticate(smtpUserEmail, smtpPassword);

                    client.Send(message);
                    client.Disconnect(true);
                }
                #endregion

                // Restituisci un oggetto JSON con l'URL di reindirizzamento
                var redirectUrl = Url.Action("Message", "Esito", new { success = true, message = "Demende de Réservation réussie!" });
                return Json(new { success = true, redirectUrl });
            }

            return Json(new { success = false, message = "Erreur lors de la réservation." });
        }
    }
}