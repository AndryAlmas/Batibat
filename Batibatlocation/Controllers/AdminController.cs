using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Web.Mvc;
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