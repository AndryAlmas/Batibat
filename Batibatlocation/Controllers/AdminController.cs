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

namespace Batibatlocation.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        const int pageSize = 3;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
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
            return RedirectToAction("Login", "Account");
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
        public ActionResult Echafaudages(int? page)
        {
            int pageNumber = (page ?? 1);
            var echafaudages = _context.Echafaudages.OrderBy(e=>e.Id).ToPagedList(pageNumber, pageSize);
            return View(echafaudages);
        }

        // GET: Admin/Echafaudage/Create
        [Authorize]
        public ActionResult CreateEchafaudage()
        {
            return View();
        }

        // POST: Admin/Echafaudage/Create
        [HttpPost]
        [Authorize]
        public ActionResult CreateEchafaudage(Echafaudage echafaudage)
        {
            if (ModelState.IsValid)
            {
                _context.Echafaudages.Add(echafaudage);
                _context.SaveChanges();
                return RedirectToAction("Echafaudages");
            }
            return View(echafaudage);
        }

        // GET: Admin/Echafaudage/Edit/{id}
        [Authorize]
        public ActionResult EditEchafaudage(int id)
        {
            var echafaudage = _context.Echafaudages.Find(id);
            if (echafaudage == null)
            {
                return HttpNotFound();
            }
            return View(echafaudage);
        }

        // POST: Admin/Echafaudage/Edit/{id}
        [HttpPost]
        [Authorize]
        public ActionResult EditEchafaudage([Bind(Include = "Id,Nom,Description,Prix,Disponible,ImageUrl,SpecificheTechniques,PeriodiciteId,Visible")] Echafaudage echafaudage, HttpPostedFileBase imageFile)
        {
            if (ModelState.IsValid)
            {
                // Gestisci l'upload dell'immagine
                if (imageFile != null && imageFile.ContentLength > 0)
                {
                    string fileName = $"produit-{echafaudage.Id}.png";
                    string path = Path.Combine(Server.MapPath("~/Content/Images/Echafaudages"), fileName);
                    imageFile.SaveAs(path);
                    echafaudage.ImageUrl = Url.Content($"~/Content/Images/Echafaudages/{fileName}");
                }
                _context.Entry(echafaudage).State = System.Data.Entity.EntityState.Modified;
                _context.SaveChanges();
                return RedirectToAction("Echafaudages");
            }
            return View(echafaudage);
        }

        // GET: Admin/Echafaudage/Delete/{id}
        [Authorize]
        public ActionResult DeleteEchafaudage(int id)
        {
            var echafaudage = _context.Echafaudages.Find(id);
            if (echafaudage == null)
            {
                return HttpNotFound();
            }
            return View(echafaudage);
        }

        // POST: Admin/Echafaudage/Delete/{id}
        [HttpPost, ActionName("DeleteEchafaudage")]
        [Authorize]
        public ActionResult DeleteConfirmed(int id)
        {
            var echafaudage = _context.Echafaudages.Find(id);
            if (echafaudage == null)
            {
                return HttpNotFound();
            }
            _context.Echafaudages.Remove(echafaudage);
            _context.SaveChanges();
            return RedirectToAction("Echafaudages");
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
                return HttpNotFound();
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
                return HttpNotFound();
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
                return HttpNotFound();
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

        // GET: Admin/Reservation/Details/{id}
        [Authorize]

        public ActionResult Details(int id)
        {
            var echafaudage = _context.Echafaudages
                .FirstOrDefault(r => r.Id == id);
            if (echafaudage == null)
            {
                return HttpNotFound();
            }
            return View(echafaudage);
        }

        // GET: Admin/Reservation/Confirm/{id}
        [Authorize]

        public ActionResult Confirm(int id)
        {
            var reservation = _context.Reservations.Find(id);
            if (reservation == null)
            {
                return HttpNotFound();
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
                return HttpNotFound();
            }
            // Logica per annullare la prenotazione
            return RedirectToAction("Reservations");
        }

        // GET: Echafaudage/CreateReservation
        [Authorize]

        public ActionResult CreateReservation(int echafaudageId)
        {
            ViewBag.EchafaudageId = echafaudageId;
            ViewBag.Echafaudages = new SelectList(_context.Echafaudages, "Id", "Nom", echafaudageId);
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
                reservation.EchafaudageId = echafaudageId;
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
                SendConfirmationEmail(reservation.Email, reservation);

                return RedirectToAction("Index", "Admin");
            }

            ViewBag.EchafaudageId = echafaudageId;
            ViewBag.Echafaudages = new SelectList(_context.Echafaudages, "Id", "Nom", echafaudageId);
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