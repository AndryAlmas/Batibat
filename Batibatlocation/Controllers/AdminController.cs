using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Security;
using Batibatlocation.Data;
using Batibatlocation.Models;
using MimeKit;
using MailKit.Net.Smtp;
using System.Security.Cryptography;
using Batibatlocation.Filters;
using System.Drawing.Printing;
using PagedList;
using System.Web;
using System.Web.Http;
using HttpGetAttribute = System.Web.Mvc.HttpGetAttribute;
using HttpPostAttribute = System.Web.Mvc.HttpPostAttribute;
using AuthorizeAttribute = System.Web.Mvc.AuthorizeAttribute;
using ActionNameAttribute = System.Web.Mvc.ActionNameAttribute;
using System.Web.UI.WebControls;
using System.Web.Helpers;
using Batibatlocation.Helpers;
using Batibatlocation.Utils;
using static Org.BouncyCastle.Crypto.Engines.SM2Engine;
using Produit = Batibatlocation.Models.Produit;
using System.Globalization;
using Batibatlocation.ViewModels;
using OpenQA.Selenium;
using System.Threading;

namespace Batibatlocation.Controllers
{
    public class AdminController : BaseController
    {
        const int pageSize = 10;

        public AdminController(ApplicationDbContext context) : base(context)
        {
        }

        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [VerificaNumeroTentativi]
        public ActionResult Login(string username, string password)
        {
            // Leggi le credenziali dal file users.txt
            string filePath = Server.MapPath("~/App_Data/users.txt");
            if (!System.IO.File.Exists(filePath))
            {
                ModelState.AddModelError("", "Fichier users.txt non trouvé.");
                return View();
            }

            string[] lines = System.IO.File.ReadAllLines(filePath);
            bool isValidUser = false;

            foreach (string line in lines)
            {
                string[] parts = line.Split(':');
                if (parts.Length == 2 && parts[0].Trim() == username && parts[1].Trim() == password)
                {
                    isValidUser = true;
                    break;
                }
            }

            if (isValidUser)
            {
                FormsAuthentication.SetAuthCookie(username, false);
                return RedirectToAction("Dashboard", "Admin");
            }
            else
            {
                ModelState.AddModelError("", "Invalid username or password.");
                return View();
            }
        }
        [Authorize]
        [VerificaNumeroTentativi]
        public ActionResult Dashboard()
        {
            return View();
        }

        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        [VerificaNumeroTentativi]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [VerificaNumeroTentativi]
        public async Task<ActionResult> ForgotPassword(string username)
        {
            // Esempio di verifica dell'utente
            if (username == "admin")
            {
                // Genera un codice OTP
                string otp = GenerateOTP(6);

                // Salva il codice OTP in un file temporaneo
                SaveOTP(otp);

                // Invia l'email con il codice OTP in modo asincrono
                await SendOTPEmailAsync(otp);

                if(ModelState.IsValid)
                    // Reindirizza alla pagina di verifica OTP
                    return RedirectToAction("VerifyOTP", new { username });
                else
                    return View();
            }
            else
            {
                ModelState.AddModelError("", "Nom d'utilisateur non trouvé.");
                return View();
            }
        }

        private string GenerateOTP(int length)
        {
            // Genera un codice OTP casuale

            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789[]!?-=+#@$€%&()";
            char[] otp = new char[length];

            using (var rng = new RNGCryptoServiceProvider())
            {
                byte[] randomBytes = new byte[length];

                rng.GetBytes(randomBytes); // Genera byte casuali sicuri

                for (int i = 0; i < length; i++)
                {
                    otp[i] = chars[randomBytes[i] % chars.Length]; // Seleziona un carattere casuale
                }
            }

            return new string(otp);
        }

        private void SaveOTP(string otp)
        {
            // Salva il codice OTP in un file temporaneo
            string filePath = Path.Combine(Server.MapPath("~/App_Data"), "otp.txt");
            System.IO.File.WriteAllText(filePath, otp+":0"); // 0 tentativi
        }

        private async Task SendOTPEmailAsync(string otp)
        {
            // Ottieni i parametri SMTP dal file di configurazione
            string adminEmail = ConfigurationManager.AppSettings["AdminEmail"];
            string smtpHost = ConfigurationManager.AppSettings["SmtpHost"];
            int smtpPort = int.Parse(ConfigurationManager.AppSettings["SmtpPort"]);
            string smtpUserEmail = ConfigurationManager.AppSettings["SmtpUser"];
            string smtpName = ConfigurationManager.AppSettings["SmtpName"];
            string smtpPassword = ConfigurationManager.AppSettings["SmtpPassword"]; // Utilizza la tua password o App Password

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(smtpName, smtpUserEmail));
            message.To.Add(new MailboxAddress("Batibat", adminEmail));
            message.Subject = "Changement de Mot de Passe pour le compte Batibatlocation.com";

            //// Corpo della mail con il codice OTP
            StringBuilder bodyBuilder = new StringBuilder();
            bodyBuilder.AppendLine($"Bonjour,");
            bodyBuilder.AppendLine($"Vous avez demandé un changement de mot de passe pour votre compte Bati'Bat.");
            bodyBuilder.AppendLine($"Voici votre code OTP pour changer votre mot de passe:");
            bodyBuilder.AppendLine($"<br>");
            bodyBuilder.AppendLine($"<br>");
            bodyBuilder.AppendLine($"<div style='background-color: black; color: yellow; padding: 3px; font-size: 24px; text-align: center;'>");
            bodyBuilder.AppendLine($"<strong>{otp}</strong>");
            bodyBuilder.AppendLine($"</div>");
            bodyBuilder.AppendLine($"<br>");
            bodyBuilder.AppendLine($"<br>");
            bodyBuilder.AppendLine($"Entrez ce code sur la page de vérification OTP pour continuer.");
            bodyBuilder.AppendLine($"<br>");
            bodyBuilder.AppendLine($"<br>");
            bodyBuilder.AppendLine($"Cordialement,");
            bodyBuilder.AppendLine($"<br>");
            bodyBuilder.AppendLine($"Équipe Bati'Bat");

            message.Body = new TextPart("html") { Text = bodyBuilder.ToString()};

            using (var client = new MailKit.Net.Smtp.SmtpClient())
            {
                client.Connect(smtpHost, smtpPort, true);
                
                // Note: only needed if the SMTP server requires authentication
                client.Authenticate(smtpUserEmail, smtpPassword);

                client.Send(message);
                client.Disconnect(true);
            }            
        }

        [HttpGet]
        [VerificaNumeroTentativi]
        public ActionResult VerifyOTP(string username)
        {
            ViewBag.Username = username;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [VerificaNumeroTentativi]
        public ActionResult VerifyOTP(string username, string otp)
        {
            // Leggi il codice OTP dal file temporaneo
            string filePath = Path.Combine(Server.MapPath("~/App_Data"), "otp.txt");

            string[] lines = System.IO.File.ReadAllLines(filePath);

            string[] parts = lines.Last().Split(':');
            string otpFile = parts[0].Trim();   
            int otpCount = int.Parse(parts[1].Trim());

            if (otpFile == otp)
            {
                // Reindirizza alla pagina di cambio password
                return RedirectToAction("ResetPassword", new { username });
            }
            else
            {
                AumentaNumeroTentativi(); // Incrementa il count nel file otp
                ModelState.AddModelError("", "Code OTP invalide.");
                ModelState.AddModelError("", "Nombre maximum de tentatives atteint: " + "[" + (otpCount+1) + "/5].");
                ViewBag.Username = username;
                return View();
            }            
        }

        private void AumentaNumeroTentativi()
        {
            string filePath = Server.MapPath("~/App_Data/otp.txt");
            int tentativiCorrenti = 0;
            string passw = "";

            // Controlla se il file esiste e legge l'ultima riga
            if (System.IO.File.Exists(filePath))
            {
                var lines = System.IO.File.ReadAllLines(filePath);
                if (lines.Length > 0)
                {
                    string lastLine = lines.Last(); // Ottieni l'ultima riga
                    string[] parts = lastLine.Split(':');
                    passw = parts[0].Trim();

                    if (parts.Length == 2 && int.TryParse(parts[1].Trim(), out tentativiCorrenti))
                    {
                        // Aumenta il numero di tentativi
                        tentativiCorrenti += 1;
                    }
                }
            }

            // Scrivi il nuovo numero di tentativi nel file
            using (StreamWriter writer = new StreamWriter(filePath, false))
            {
                // Scrivi l'ultima riga con il nuovo numero di tentativi
                writer.WriteLine($"{passw}:{tentativiCorrenti}");
            }
        }

        [HttpGet]
        [VerificaNumeroTentativi]
        public ActionResult ResetPassword(string username)
        {
            ViewBag.Username = username;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [VerificaNumeroTentativi]
        public ActionResult ResetPassword(string username, string newPassword)
        {
            // Esempio di salvataggio della nuova password in un file protetto
            string filePath = Path.Combine(Server.MapPath("~/App_Data"), "users.txt");
            string userData = $"{username}:{newPassword}";
            System.IO.File.WriteAllText(filePath, userData);

            // Reindirizza alla pagina di login
            return RedirectToAction("Login");
        }

        // GET: Admin/Echafaudages
        [Authorize]
        [HttpGet]
        public ActionResult Produits(int? page, int? categoryId)
        {
            int pageNumber = (page ?? 1);

            IPagedList<Produit> produits = null;

            if (categoryId.HasValue && categoryId.Value != 0)
            {
                produits = _context.Produits.Where(e => e.CategoryId == categoryId).OrderByDescending(e => e.Id).ToPagedList(pageNumber, pageSize);
            }
            else
            {
                produits = _context.Produits.OrderByDescending(e => e.Id).ToPagedList(pageNumber, pageSize);
                categoryId = 0;
            }
            var categories = _context.Categories.ToList();
            categories.Insert(0, new Category { Id = 0, Nom = "Tout" });
            ViewBag.CategoryList = new SelectList(categories, "Id", "Nom", categoryId);

            return View(produits);
        }

        // GET: Admin/Echafaudage/Create
        [Authorize]
        public ActionResult CreateProduit()
        {
            ViewBag.PeriodiciteList = new SelectList(_context.Periodicites.ToList(), "Id", "Nom",2);
            ViewBag.CategoryList = new SelectList(_context.Categories.ToList(), "Id", "Nom");
            return View();
        }

        // POST: Admin/Echafaudage/Create
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult CreateProduit([Bind(Exclude = "Id,ImageUrl,LastMod")] Produit produit, HttpPostedFileBase imageFile, List<HttpPostedFileBase> fileInput)
        {
            if (imageFile != null && imageFile.ContentLength > 0)
            {
                ModelState.Remove("ImageUrl");
            }
            else
            {
                ModelState.AddModelError("ImageUrl", "L'ImageUrl est requis.");
            }
            if (ModelState.IsValid)
            {
                produit.LastMod = DateTime.UtcNow;
                _context.Produits.Add(produit);
                _context.SaveChanges();

                // Genera un nuovo ID per l'échafaudage
                produit.Id = _context.Produits.Max(e => e.Id);

                // Gestisci l'upload dell'immagine
                if (imageFile != null && imageFile.ContentLength > 0)
                {
                    string fileName = $"produit-{produit.Id}.png";
                    string path = Path.Combine(Server.MapPath("~/Content/Images/Produits"), fileName);
                    imageFile.SaveAs(path);
                    produit.ImageUrl = Url.Content($"~/Content/Images/Produits/{fileName}");
                }

                // Creazione della directory por gli altri file
                string virtualPath = "~/Content/Images/Produits/SlideGallery/" + $"produit-{produit.Id}";
                string physicalPath = Server.MapPath(virtualPath);
                Directory.CreateDirectory(physicalPath);

                if (fileInput != null && fileInput.Count > 0)
                {
                    foreach (var photo in fileInput)
                    {
                        if (photo != null && photo.ContentLength > 0)
                        {
                            string fileName = $"{(fileInput.IndexOf(photo) + 1)}.png";
                            string produitID = $"produit-{produit.Id}";
                            string path = Path.Combine(Server.MapPath("~/Content/Images/Produits/SlideGallery/" + produitID + "/"), fileName);
                            photo.SaveAs(path);
                        }
                    }

                }

                _context.Entry(produit).State = System.Data.Entity.EntityState.Modified;
                _context.SaveChanges();

                return RedirectToAction("Produits");
            }
            ViewBag.PeriodiciteList = new SelectList(_context.Periodicites.ToList(), "Id", "Nom", produit.PeriodiciteId);
            ViewBag.CategoryList = new SelectList(_context.Categories.ToList(), "Id", "Nom", produit.CategoryId);

            return View(produit);
        }

        // GET: Admin/Reservation/Details/{id}
        [Authorize]

        public ActionResult DetailsProduit(int id)
        {
            return RedirectToAction("EditProduit", new { id = id, visualizza = true});
        }

        // GET: Admin/Echafaudage/Edit/{id}
        [Authorize]
        public ActionResult EditProduit(int id, bool visualizza = false)
        {
            if (visualizza)
            {
                ViewBag.IsReadOnly = true;
            }

            var produit = _context.Produits.Find(id);
            if (produit == null)
            {
                TempData.SetAlert("Alert", "Aucun produit trouvé.", "danger");
                return RedirectToAction("Produits");
            }

            var imageUrl = produit.ImageUrl.Split('/').LastOrDefault().Split('.').FirstOrDefault();
            string folderPath = Server.MapPath("~/Content/Images/Produits/SlideGallery/" + imageUrl + "/");

            string[] imagePaths = {};
            // Leggi tutti i file nella cartella
            if (Directory.Exists(folderPath))
            {
                imagePaths = Directory.GetFiles(folderPath); // Ottiene i percorsi completi dei file
            }

            List<int> posizioniImg = new List<int>();
            for (int i = 0; i < imagePaths.Length; i++)
            {
                var nomeImg = imagePaths[i].Split('\\').LastOrDefault();
                posizioniImg.Add(int.Parse(nomeImg.Split('.').First()));
                imagePaths[i] = "~/Content/Images/Produits/SlideGallery/" + imageUrl + "/" + nomeImg;
            }
            // Passa i percorsi alla vista tramite ViewBag
            ViewBag.Images = imagePaths;
            ViewBag.PosizioniDisp = posizioniImg;

            ViewBag.PeriodiciteList = new SelectList(_context.Periodicites.ToList(), "Id", "Nom", produit.PeriodiciteId);
            ViewBag.CategoryList = new SelectList(_context.Categories.ToList(), "Id", "Nom", produit.CategoryId);

            return View(produit);
        }

        // POST: Admin/Echafaudage/Edit/{id}
        [HttpPost]
        [Authorize]
        public ActionResult EditProduit([Bind(Include = "Id,Nom,Description,Prix,Disponible,ImageUrl,SpecifiquesTechniques,PeriodiciteId,CategoryId,Visible")] Produit produit, HttpPostedFileBase imageFile, List<HttpPostedFileBase> fileInput)
        {
            if ((imageFile != null && imageFile.ContentLength > 0) || !string.IsNullOrEmpty(produit.ImageUrl))
            {
                ModelState.Remove("ImageUrl");
            }
            else
            {
                ModelState.AddModelError("ImageUrl", "L'ImageUrl est requis.");
            }
            if (ModelState.IsValid)
            {
                // Gestisci l'upload dell'immagine
                if (imageFile != null && imageFile.ContentLength > 0)
                {
                    string fileName = $"produit-{produit.Id}.png";
                    string path = Path.Combine(Server.MapPath("~/Content/Images/Produits"), fileName);
                    imageFile.SaveAs(path);
                    produit.ImageUrl = Url.Content($"~/Content/Images/Produits/{fileName}");
                }
                if (fileInput != null && fileInput.Count > 0)
                {
                    foreach (var photo in fileInput)
                    {
                        if(photo != null && photo.ContentLength > 0)
                        {
                            string fileName = $"{(fileInput.IndexOf(photo) + 1)}.png";
                            string produitID = $"produit-{produit.Id}";
                            string path = Path.Combine(Server.MapPath("~/Content/Images/Produits/SlideGallery/" + produitID + "/"), fileName);
                            photo.SaveAs(path);
                        }
                    }

                }

                produit.LastMod = DateTime.UtcNow;
                _context.Entry(produit).State = System.Data.Entity.EntityState.Modified;
                _context.SaveChanges();
                return RedirectToAction("Produits");
            }
            ViewBag.PeriodiciteList = new SelectList(_context.Periodicites.ToList(), "Id", "Nom", produit.PeriodiciteId);
            ViewBag.CategoryList = new SelectList(_context.Categories.ToList(), "Id", "Nom", produit.CategoryId);

            return View(produit);
        }

        [HttpPost]
        [Authorize]
        public JsonResult ToggleVisibility(int id)
        {
            using (var db = new ApplicationDbContext()) // Usa il tuo DbContext
            {
                var echafaudage = db.Produits.Find(id);
                if (echafaudage == null)
                {
                    return Json(new { success = false });
                }

                // Inverti lo stato
                echafaudage.Visible = !echafaudage.Visible;
                db.SaveChanges();

                return Json(new { success = true });
            }
        }

        [HttpPost]
        [Authorize]
        public JsonResult DeleteImage([FromBody] dynamic imagePath)
        {
            try
            {
                //string imagePath = data?.imagePath;
                if (!string.IsNullOrEmpty(imagePath))
                {
                    string fullPath = Server.MapPath(imagePath);

                    if (System.IO.File.Exists(fullPath))
                    {
                        System.IO.File.Delete(fullPath);
                        return Json(new { success = true });
                    }
                    return Json(new { success = false, message = "Fichier introuvable." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }

            return Json(new { success = false, message = "Paramètre invalide." });
        }

        // POST: Admin/Echafaudage/Delete/{id}
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteProduit(int? id, int? page)
        {
            var produit = _context.Produits.Find(id);
            if (produit == null)
            {
                TempData.SetAlert("Alert", "Aucun produit trouvé.", "danger");
                return RedirectToAction("Produits");
            }
            _context.Produits.Remove(produit);
            _context.SaveChanges();

            string virtualPathAutreImages = "~/Content/Images/Produits/SlideGallery/" + $"produit-{produit.Id}";
            if (System.IO.Directory.Exists(Server.MapPath(virtualPathAutreImages)))
            {
                Directory.Delete(Server.MapPath(virtualPathAutreImages), true);
            }

            string fileName = $"produit-{produit.Id}.png";
            string fullPath = Path.Combine(Server.MapPath("~/Content/Images/Produits"), fileName);

            if (System.IO.File.Exists(fullPath))
            {
                System.IO.File.Delete(fullPath);
            }

            TempData.SetAlert("Alert", "Produit supprimé avec succès.", "success");
            return RedirectToAction("Produits", new {page});
        }

        // GET: Admin/Accessoires
        [Authorize]

        public ActionResult Accessoires()
        {
            var accessoires = _context.Accessoires.ToList();
            return View(accessoires);
        }

        // GET: Admin/Accessoire/Create
        [Authorize]

        public ActionResult CreateAccessoire()
        {
            return View();
        }

        // POST: Admin/Accessoire/Create
        [Authorize]
        [HttpPost]
        public ActionResult CreateAccessoire(Accessoire accessoire)
        {
            if (ModelState.IsValid)
            {
                _context.Accessoires.Add(accessoire);
                _context.SaveChanges();
                return RedirectToAction("Accessoires");
            }
            return View(accessoire);
        }

        // GET: Admin/Accessoire/Edit/{id}
        [Authorize]

        public ActionResult EditAccessoire(int id)
        {
            var accessoire = _context.Accessoires.Find(id);
            if (accessoire == null)
            {
                TempData.SetAlert("Alert", "Aucun accessoire trouvé.", "danger");

                return RedirectToAction("Accessoires");
            }
            return View(accessoire);
        }

        // POST: Admin/Accessoire/Edit/{id}
        [HttpPost]
        [Authorize]

        public ActionResult EditAccessoire(Accessoire accessoire)
        {
            if (ModelState.IsValid)
            {
                _context.Entry(accessoire).State = System.Data.Entity.EntityState.Modified;
                _context.SaveChanges();
                return RedirectToAction("Accessoires");
            }
            return View(accessoire);
        }

        // GET: Admin/Accessoire/Delete/{id}
        [Authorize]

        public ActionResult DeleteAccessoire(int id)
        {
            var accessoire = _context.Accessoires.Find(id);
            if (accessoire == null)
            {
                TempData.SetAlert("Alert", "Aucun accessoire trouvé.", "danger");
                return RedirectToAction("Accessoires");
            }
            return View(accessoire);
        }

        // POST: Admin/Accessoire/Delete/{id}
        [HttpPost, ActionName("DeleteAccessoire")]
        [Authorize]
        public ActionResult DeleteAccessoireConfirmed(int id)
        {
            var accessoire = _context.Accessoires.Find(id);
            if (accessoire == null)
            {
                TempData.SetAlert("Alert", "Aucun accessoire trouvé.", "danger");
                return RedirectToAction("Accessoires");
            }
            _context.Accessoires.Remove(accessoire);
            _context.SaveChanges();
            return RedirectToAction("Accessoires");
        }

        // GET: Admin/Reservations
        [Authorize]

        public ActionResult Reservations()
        {
            var reservations = _context.Reservations
                .Include("Echafaudage")
                .Include("ReservationAccessoires.Accessoire")
                .ToList();
            return View(reservations);
        }



        // GET: Admin/Reservation/Confirm/{id}
        [Authorize]

        public ActionResult Confirm(int id)
        {
            var reservation = _context.Reservations.Find(id);
            if (reservation == null)
            {
                TempData.SetAlert("Alert", "Aucun reservation trouvé.", "danger");
                return RedirectToAction("Reservations");
            }
            // Logica per confermare la prenotazione
            return RedirectToAction("Reservations");
        }

        // GET: Admin/Reservation/Cancel/{id}
        [Authorize]

        public ActionResult Cancel(int id)
        {
            var reservation = _context.Reservations.Find(id);
            if (reservation == null)
            {
                TempData.SetAlert("Alert", "Aucun reservation trouvé.", "danger");
                return RedirectToAction("Reservations");
            }
            // Logica per annullare la prenotazione
            return RedirectToAction("Reservations");
        }

        // GET: Echafaudage/CreateReservation
        [Authorize]

        public ActionResult CreateReservation(int echafaudageId)
        {
            ViewBag.EchafaudageId = echafaudageId;
            ViewBag.Echafaudages = new SelectList(_context.Produits, "Id", "Nom", echafaudageId);
            ViewBag.Accessoires = new MultiSelectList(_context.Accessoires, "Id", "Nom");
            return View();
        }

        // POST: Echafaudage/CreateReservation
        [HttpPost]
        [Authorize]

        public ActionResult CreateReservation(Reservation reservation, int echafaudageId, int[] selectedAccessoires, int[] quantites)
        {
            if (ModelState.IsValid)
            {
                reservation.ProduitId = echafaudageId;
                reservation.ReservationAccessoires = new List<ReservationAccessoire>();

                if (selectedAccessoires != null && quantites != null && selectedAccessoires.Length == quantites.Length)
                {
                    for (int i = 0; i < selectedAccessoires.Length; i++)
                    {
                        var accessoireId = selectedAccessoires[i];
                        var quantite = quantites[i];

                        var accessoire = _context.Accessoires.Find(accessoireId);
                        if (accessoire != null)
                        {
                            reservation.ReservationAccessoires.Add(new ReservationAccessoire
                            {
                                AccessoireId = accessoireId,
                                Quantite = quantite
                            });
                        }
                    }
                }

                _context.Reservations.Add(reservation);
                _context.SaveChanges();

                // Invia conferma via email
                SendConfirmationEmail(reservation.Client.Email, reservation);

                return RedirectToAction("Index", "Admin");
            }

            ViewBag.EchafaudageId = echafaudageId;
            ViewBag.Echafaudages = new SelectList(_context.Produits, "Id", "Nom", echafaudageId);
            ViewBag.Accessoires = new MultiSelectList(_context.Accessoires, "Id", "Nom");
            return View(reservation);
        }


        private void SendConfirmationEmail(string email, Reservation reservation)
        {
            //try
            //{
            //    using (var mail = new MailMessage())
            //    {
            //        mail.From = new MailAddress("your_email@example.com");
            //        mail.To.Add(email);
            //        mail.Subject = "Confirmation de Réservation";

            //        var body = $"Cher(e) {reservation.Nom},\n\nVotre réservation a été confirmée.\n\nDétails de la réservation:\nDate Début: {reservation.DateDebut.ToShortDateString()}\nDate Fin: {reservation.DateFin.ToShortDateString()}\n\nÉchafaudage: {reservation.Echafaudage.Nom}\n\nAccessoires Réservés:\n";

            //        foreach (var accessoire in reservation.ReservationAccessoires)
            //        {
            //            var accessoireObj = _context.Accessoires.Find(accessoire.AccessoireId);
            //            if (accessoireObj != null)
            //            {
            //                body += $"{accessoireObj.Nom} - Quantité: {accessoire.Quantite}\n";
            //            }
            //        }

            //        mail.Body = body;

            //        using (var smtp = new SmtpClient())
            //        {
            //            smtp.Host = "smtp.yourhost.com"; // Sostituisci con l'host SMTP fornito da Aruba
            //            smtp.Port = 587; // Porta SMTP
            //            smtp.EnableSsl = true;
            //            smtp.Credentials = new System.Net.NetworkCredential("your_username", "your_password"); // Credenziali SMTP
            //            smtp.Send(mail);
            //        }
            //    }
            //}
            //catch (Exception ex)
            //{
            //    // Gestisci eventuali eccezioni
            //    // Puoi registrare l'errore in un file di log o inviarlo tramite altri mezzi
            //}
        }

        // GET: Admin/Echafaudages
        [Authorize]
        [HttpGet]
        public ActionResult Categories(int? page)
        {
            int pageNumber = (page ?? 1);

            IPagedList<Category> categories = _context.Categories.OrderByDescending(c => c.Id).ToPagedList(pageNumber, pageSize);

            return View(categories); 
        }

        // GET: Admin/Promotions
        [Authorize]
        [HttpGet]
        public ActionResult Promotions(int? page)
        {
            // Numero di pagina corrente (default: 1)
            int pageNumber = (page ?? 1);

            // Recupera tutte le categorie ordinate per ID decrescente
            List<Category> categories = _context.Categories
                .OrderByDescending(c => c.Id)
                .ToList();

            // Recupera tutti i prodotti ordine decrescente per ID
            List<Produit> products = _context.Produits
                .OrderByDescending(p => p.Id)
                .ToList();

            // Recupera tutte le promozioni dal database
            List<Promotion> promotions = _context.Promotions
                .OrderByDescending(p => p.Id)
                .ToList();

            // Mappa le promozioni in PromotionVM
            var promotionVMs = promotions.Select(promotion => new PromotionVM
            {
                Id = promotion.Id,
                CategoryId = promotion.CategoryId,
                ProductId = promotion.ProductId,
                StartDate = promotion.StartDate ?? new DateTime(),
                EndDate = promotion.EndDate ?? new DateTime(),
                DiscountValue = promotion.DiscountValue,
                IsPercentage = promotion.IsPercentage,
                Categories = categories.Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Nom
                }).ToList(),
                Products = products.Select(p => new SelectListItem
                {
                    Value = p.Id.ToString(),
                    Text = p.Nom
                }).ToList()
            }).ToList();

            ViewBag.Categories = categories.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.Nom
            }).ToList();

            ViewBag.Products = products.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.Nom
            }).ToList();
            

            // Pagina i risultati
            int pageSize = 10; // Numero di elementi per pagina
            IPagedList<PromotionVM> pagedPromotions = promotionVMs.ToPagedList(pageNumber, pageSize);

            // Passa i dati alla vista
            return View(pagedPromotions);
        }

        [HttpPost]
        public ActionResult CreatePromotion([FromBody] PromotionVM promotionVM)
        {
            if (!ModelState.IsValid)
            {
                // Restituisci gli errori di validazione
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return Json(new { success = false, message = string.Join(" ", errors) });
            }

            try
            {
                // Verifica che sia selezionata almeno una categoria o un prodotto
                if (!promotionVM.CategoryId.HasValue && !promotionVM.ProductId.HasValue)
                {
                    return Json(new { success = false, message = "Vous devez sélectionner une catégorie ou un produit." });
                }

                // Verifica che la data di inizio sia precedente alla data di fine
                if (promotionVM.StartDate >= promotionVM.EndDate)
                {
                    return Json(new { success = false, message = "La date de début doit être antérieure à la date de fin." });
                }

                // Verifica che le date non si sovrappongano ad altre promozioni
                bool isOverlapping = _context.Promotions.Any(p =>
                    (
                        // Sovrapposizione nella stessa categoria
                        (p.CategoryId == promotionVM.CategoryId && promotionVM.CategoryId.HasValue &&
                            (
                                (promotionVM.StartDate >= p.StartDate && promotionVM.StartDate <= p.EndDate) || // Inizia durante un'altra promozione
                                (promotionVM.EndDate >= p.StartDate && promotionVM.EndDate <= p.EndDate) ||     // Termina durante un'altra promozione
                                (promotionVM.StartDate <= p.StartDate && promotionVM.EndDate >= p.EndDate)      // Avvolge completamente un'altra promozione
                            )
                        )
                    ) ||
                    (
                        // Sovrapposizione nello stesso prodotto
                        (p.ProductId == promotionVM.ProductId && promotionVM.ProductId.HasValue &&
                            (
                                (promotionVM.StartDate >= p.StartDate && promotionVM.StartDate <= p.EndDate) || // Inizia durante un'altra promozione
                                (promotionVM.EndDate >= p.StartDate && promotionVM.EndDate <= p.EndDate) ||     // Termina durante un'altra promozione
                                (promotionVM.StartDate <= p.StartDate && promotionVM.EndDate >= p.EndDate)      // Avvolge completamente un'altra promozione
                            )
                        )
                    )
                );

                if (isOverlapping)
                {
                    return Json(new { success = false, message = "Les dates de la promotion se chevauchent avec une autre promotion existante." });
                }

                // Crea una nuova entità Promotion
                var newPromotion = new Promotion
                {
                    CategoryId = promotionVM.CategoryId,
                    ProductId = promotionVM.ProductId,
                    StartDate = promotionVM.StartDate,
                    EndDate = promotionVM.EndDate,
                    DiscountValue = promotionVM.DiscountValue,
                    IsPercentage = promotionVM.IsPercentage
                };

                // Aggiungi la promozione al database
                _context.Promotions.Add(newPromotion);
                _context.SaveChanges();

                // Restituisci una risposta di successo
                return Json(new { success = true, message = "La promotion a été créée avec succès." });
            }
            catch (Exception ex)
            {
                // Gestisci eventuali errori
                return Json(new { success = false, message = $"Une erreur est survenue : {ex.Message}" });
            }
        }

        [HttpPost]
        [Authorize]
        public ActionResult CreateCategorie(string Nom, string PrixKm, string PrixMin, int? page)
        {
            if (string.IsNullOrWhiteSpace(Nom))
            {
                TempData.SetAlert("Alert", "Le nom de la catégorie est requis.", "warning");
                return RedirectToAction("Categories", new {page});
            }
            if (string.IsNullOrWhiteSpace(PrixKm))
            {
                TempData.SetAlert("Alert", "Le Prix au Km est requis.", "warning");
                return RedirectToAction("Categories", new { page });
            }
            if (string.IsNullOrWhiteSpace(Nom))
            {
                TempData.SetAlert("Alert", "Le Prix Minimum est requis.", "warning");
                return RedirectToAction("Categories", new { page });
            }

            var categoryExist = _context.Categories.Where(c => c.Nom.Equals(Nom)).Any();
            
            if (categoryExist)
            {
                // Mostra un messaggio di errore se ci sono prodotti associati
                TempData.SetAlert("Alert", "Impossible de créer cette catégorie car elle existe déjà.", "danger");
                return RedirectToAction("Categories", new { page });
            }

            // Sostituisci il punto con la virgola
            PrixKm = PrixKm.Replace('.', ',');
            PrixMin = PrixMin.Replace('.', ',');

            // Usa una cultura che supporta la virgola come separatore decimale (es. it-IT)
            CultureInfo culture = new CultureInfo("it-IT");

            // Converti i valori in decimal
            decimal prixAuKm = Convert.ToDecimal(PrixKm, culture);
            decimal prixMin = Convert.ToDecimal(PrixMin, culture);

            var newCategory = new Category { Nom = Nom };
            _context.Categories.Add(newCategory);
            _context.SaveChanges();

            var costiLivraison = new CostiLivraison
            {
                PrixAuKm = prixAuKm,
                PrixMin = prixMin,
                CategoryId = newCategory.Id
            };

            _context.CostiLivraisons.Add(costiLivraison);

            _context.SaveChanges();

            return RedirectToAction("Categories", new { page });

        }

        [HttpPost]
        [Authorize]
        //[ValidateAntiForgeryToken]
        public ActionResult EditCategorie(Category category, int? page)
        {
            if (ModelState.IsValid)
            {
                var existingCategory = _context.Categories.Include("CostiLivraisons").FirstOrDefault(c => c.Id == category.Id);
                if (existingCategory != null)
                {
                    existingCategory.Nom = category.Nom;

                    // Recupera i valori dal form
                    string prixAuKmString = Request.Form[$"PrixAuKm_{category.Id}"];
                    string prixMinString = Request.Form[$"PrixMin_{category.Id}"];

                    // Sostituisci il punto con la virgola
                    prixAuKmString = prixAuKmString.Replace('.', ',');
                    prixMinString = prixMinString.Replace('.', ',');

                    // Usa una cultura che supporta la virgola come separatore decimale (es. it-IT)
                    CultureInfo culture = new CultureInfo("it-IT");

                    // Converti i valori in decimal
                    decimal prixAuKm = Convert.ToDecimal(prixAuKmString, culture);
                    decimal prixMin = Convert.ToDecimal(prixMinString, culture);

                    var costiLivraison = existingCategory.CostiLivraisons.FirstOrDefault();
                    if (costiLivraison != null)
                    {
                        costiLivraison.PrixAuKm = prixAuKm;
                        costiLivraison.PrixMin = prixMin;
                    }
                    else
                    {
                        costiLivraison = new CostiLivraison
                        {
                            PrixAuKm = prixAuKm,
                            PrixMin = prixMin,
                            CategoryId = existingCategory.Id
                        };
                        existingCategory.CostiLivraisons.Add(costiLivraison);
                    }

                    _context.SaveChanges();
                    return RedirectToAction("Categories", new { page });
                }

                //_context.Entry(category).State = System.Data.Entity.EntityState.Modified;
                //_context.SaveChanges();
                TempData.SetAlert("Alert", "Error d'association entre Categories et Prix de Livraison", "error");
                return RedirectToAction("Categories", new {page});
            }            
            TempData.SetAlert("Alert", ModelState.Values.SelectMany(e => e.Errors).FirstOrDefault()?.ErrorMessage, "warning");
            return RedirectToAction("Categories", new { page });
        }

        [HttpPost]
        [Authorize]
        //[ValidateAntiForgeryToken]
        public ActionResult DeleteCategorie(int id, int? page)
        {
            // Trova la categoria da eliminare
            var category = _context.Categories.Find(id);

            if (category == null)
            {
                TempData.SetAlert("Alert", "Catégorie non trouvée.", "warning");
                return RedirectToAction("Categories", new { page });
            }

            if (category.Id == 14 || category.Id == 1 )
            {
                TempData.SetAlert("Alert", "Impossible de supprimer cette catégorie car elles sont des catégoriees principales.", "warning");
                return RedirectToAction("Categories", new { page });
            }
            // Controlla se ci sono prodotti associati alla categoria
            var produitsAssociés = _context.Produits.Any(p => p.CategoryId == id);

            if (produitsAssociés)
            {
                // Mostra un messaggio di errore se ci sono prodotti associati
                TempData.SetAlert("Alert", "Impossible de supprimer cette catégorie car elle est associée à des produits.", "danger");
                return RedirectToAction("Categories", new { page });
            }

            // Se non ci sono prodotti associati, elimina la categoria
            _context.Categories.Remove(category);
            _context.SaveChanges();

            return RedirectToAction("Categories", new { page });
        }

        [HttpPost]
        [Authorize]
        //[ValidateAntiForgeryToken]
        public ActionResult DeletePromotion(int id, int? page)
        {
            // Trova la promotion da eliminare
            var promotion = _context.Promotions.Find(id);

            if (promotion == null)
            {
                TempData.SetAlert("Alert", "La promotion n'existe pas.", "warning");
                return RedirectToAction("Promotions", new { page });
            }
            
            // Se non ci sono prodotti associati, elimina la categoria
            _context.Promotions.Remove(promotion);
            _context.SaveChanges();

            TempData.SetAlert("Alert", "Promotions supprimé avec succès.", "success");
            return RedirectToAction("Promotions", new { page });
        }

        public ActionResult RicercaGruppiFB()
        {
            SeleniumHelper.RicercaGruppiCostruzione(60);
            return View("Dashboard");
        }

        public void PostaNelGruppo(string gruppoUrl, string messaggio)
        {
            using (var driver = SeleniumHelper.GetDriver())
            {
                driver.Navigate().GoToUrl(gruppoUrl);
                Thread.Sleep(6000);

                // Trova la textarea per postare
                var postBox = driver.FindElement(By.XPath("//div[@role='textbox']"));
                postBox.Click();
                Thread.Sleep(2000);
                postBox.SendKeys(messaggio);
                Thread.Sleep(2000);

                // Trova il pulsante "Pubblica"
                var buttons = driver.FindElements(By.XPath("//div[@aria-label='Pubblica']"));
                if (buttons.Any())
                {
                    buttons.First().Click();
                    Console.WriteLine("Post pubblicato con successo.");
                }

                Thread.Sleep(5000);
            }
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