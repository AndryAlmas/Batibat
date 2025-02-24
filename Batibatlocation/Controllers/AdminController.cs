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

namespace Batibatlocation.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

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
        public ActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(string username)
        {
            // Esempio di verifica dell'utente
            if (username == "admin")
            {
                // Genera un codice OTP
                string otp = GenerateOTP();

                // Salva il codice OTP in un file temporaneo
                SaveOTP(otp);

                // Invia l'email con il codice OTP in modo asincrono
                await SendOTPEmailAsync(otp);

                if(ModelState.IsValid)
                    // Reindirizza alla pagina di verifica OTP
                    return RedirectToAction("VerifyOTP", new { username = username });
                else
                    return View();
            }
            else
            {
                ModelState.AddModelError("", "Nom d'utilisateur non trouvé.");
                return View();
            }
        }

        private string GenerateOTP()
        {
            // Genera un codice OTP casuale
            return "123456"; // Esempio fisso per semplicità
        }

        private void SaveOTP(string otp)
        {
            // Salva il codice OTP in un file temporaneo
            string filePath = Path.Combine(Server.MapPath("~/App_Data"), "otp.txt");
            System.IO.File.WriteAllText(filePath, otp);
        }

        private async Task SendOTPEmailAsync(string otp)
        {
            // Ottieni i parametri SMTP dal file di configurazione
            string adminEmail = ConfigurationManager.AppSettings["AdminEmail"];
            string smtpHost = ConfigurationManager.AppSettings["SmtpHost"];
            int smtpPort = int.Parse(ConfigurationManager.AppSettings["SmtpPort"]);
            string smtpUser = ConfigurationManager.AppSettings["SmtpUser"];
            string smtpName = ConfigurationManager.AppSettings["SmtpName"];
            string smtpPassword = ConfigurationManager.AppSettings["SmtpPassword"]; // Utilizza la tua password o App Password

            // Configura il client SMTP
            SmtpClient smtpClient = new SmtpClient(smtpHost, smtpPort);
            smtpClient.Credentials = new System.Net.NetworkCredential(smtpUser, smtpPassword);
            smtpClient.UseDefaultCredentials = true;
            smtpClient.EnableSsl = true; // Richiede SSL per la porta 465
                      

            // Crea il messaggio email
            MailMessage mailMessage = new MailMessage();
            mailMessage.From = new MailAddress(smtpUser, smtpName);
            mailMessage.To.Add(adminEmail);
            mailMessage.Subject = "Changement de Mot de Passe";

            // Corpo della mail con il codice OTP
            StringBuilder bodyBuilder = new StringBuilder();
            bodyBuilder.AppendLine("Bonjour,");
            bodyBuilder.AppendLine("Vous avez demandé un changement de mot de passe pour votre compte Bati'Bat.");
            bodyBuilder.AppendLine("Voici votre code OTP pour changer votre mot de passe:");
            bodyBuilder.AppendLine("<div style='background-color: black; color: yellow; font-size: 24px; text-align: center;'>");
            bodyBuilder.AppendLine($"<strong>{otp}</strong>");
            bodyBuilder.AppendLine("</div>");
            bodyBuilder.AppendLine("Entrez ce code sur la page de vérification OTP pour continuer.");
            bodyBuilder.AppendLine("Cordialement,");
            bodyBuilder.AppendLine("Équipe Bati'Bat");

            mailMessage.Body = bodyBuilder.ToString();
            mailMessage.IsBodyHtml = true;

            // Invia l'email in modo asincrono
            try
            {
                await smtpClient.SendMailAsync(mailMessage);
                smtpClient.Dispose();
            }
            catch (SmtpException ex)
            {
                smtpClient.Dispose();
                // Gestisci l'eccezione
                ModelState.AddModelError("", $"Erreur lors de l'envoi de l'email.");
            }
        }

        [HttpGet]
        public ActionResult VerifyOTP(string username)
        {
            ViewBag.Username = username;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult VerifyOTP(string username, string otp)
        {
            // Leggi il codice OTP dal file temporaneo
            string filePath = Path.Combine(Server.MapPath("~/App_Data"), "otp.txt");
            string savedOtp = System.IO.File.ReadAllText(filePath);

            if (otp == savedOtp)
            {
                // Reindirizza alla pagina di cambio password
                return RedirectToAction("ResetPassword", new { username = username });
            }
            else
            {
                ModelState.AddModelError("", "Code OTP non valido.");
                ViewBag.Username = username;
                return View();
            }
        }

        [HttpGet]
        public ActionResult ResetPassword(string username)
        {
            ViewBag.Username = username;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
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
        public ActionResult ListeEchafaudages()
        {
            var echafaudages = _context.Echafaudages.ToList();
            return View(echafaudages);
        }

        // GET: Admin/Echafaudage/Create
        public ActionResult CreateEchafaudage()
        {
            return View();
        }

        // POST: Admin/Echafaudage/Create
        [HttpPost]
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
        public ActionResult EditEchafaudage(Echafaudage echafaudage)
        {
            if (ModelState.IsValid)
            {
                _context.Entry(echafaudage).State = System.Data.Entity.EntityState.Modified;
                _context.SaveChanges();
                return RedirectToAction("Echafaudages");
            }
            return View(echafaudage);
        }

        // GET: Admin/Echafaudage/Delete/{id}
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
        public ActionResult Accessoires()
        {
            var accessoires = _context.Accessoires.ToList();
            return View(accessoires);
        }

        // GET: Admin/Accessoire/Create
        public ActionResult CreateAccessoire()
        {
            return View();
        }

        // POST: Admin/Accessoire/Create
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
        public ActionResult Reservations()
        {
            var reservations = _context.Reservations
                .Include("Echafaudage")
                .Include("ReservationAccessoires.Accessoire")
                .ToList();
            return View(reservations);
        }

        // GET: Admin/Reservation/Details/{id}
        public ActionResult Details(int id)
        {
            var reservation = _context.Reservations
                .Include("Echafaudage")
                .Include("ReservationAccessoires.Accessoire")
                .FirstOrDefault(r => r.Id == id);
            if (reservation == null)
            {
                return HttpNotFound();
            }
            return View(reservation);
        }

        // GET: Admin/Reservation/Confirm/{id}
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
        public ActionResult CreateReservation(int echafaudageId)
        {
            ViewBag.EchafaudageId = echafaudageId;
            ViewBag.Echafaudages = new SelectList(_context.Echafaudages, "Id", "Nom", echafaudageId);
            ViewBag.Accessoires = new MultiSelectList(_context.Accessoires, "Id", "Nom");
            return View();
        }

        // POST: Echafaudage/CreateReservation
        [HttpPost]
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
            try
            {
                using (var mail = new MailMessage())
                {
                    mail.From = new MailAddress("your_email@example.com");
                    mail.To.Add(email);
                    mail.Subject = "Confirmation de Réservation";

                    var body = $"Cher(e) {reservation.Nom},\n\nVotre réservation a été confirmée.\n\nDétails de la réservation:\nDate Début: {reservation.DateDebut.ToShortDateString()}\nDate Fin: {reservation.DateFin.ToShortDateString()}\n\nÉchafaudage: {reservation.Echafaudage.Nom}\n\nAccessoires Réservés:\n";

                    foreach (var accessoire in reservation.ReservationAccessoires)
                    {
                        var accessoireObj = _context.Accessoires.Find(accessoire.AccessoireId);
                        if (accessoireObj != null)
                        {
                            body += $"{accessoireObj.Nom} - Quantité: {accessoire.Quantite}\n";
                        }
                    }

                    mail.Body = body;

                    using (var smtp = new SmtpClient())
                    {
                        smtp.Host = "smtp.yourhost.com"; // Sostituisci con l'host SMTP fornito da Aruba
                        smtp.Port = 587; // Porta SMTP
                        smtp.EnableSsl = true;
                        smtp.Credentials = new System.Net.NetworkCredential("your_username", "your_password"); // Credenziali SMTP
                        smtp.Send(mail);
                    }
                }
            }
            catch (Exception ex)
            {
                // Gestisci eventuali eccezioni
                // Puoi registrare l'errore in un file di log o inviarlo tramite altri mezzi
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