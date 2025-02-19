using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Web.Mvc;
using Batibatlocation.Data;
using Batibatlocation.Models;

namespace Batibat.Controllers
{
    public class ReservationController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReservationController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Reservation/Create
        public ActionResult Create()
        {
            ViewBag.Echafaudages = new SelectList(_context.Echafaudages, "Id", "Nom");
            ViewBag.Accessoires = new MultiSelectList(_context.Accessoires, "Id", "Nom");
            return View();
        }

        // POST: Reservation/Create
        [HttpPost]
        public ActionResult Create(Reservation reservation, int echafaudageId, int[] selectedAccessoires, int[] quantites)
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

                return RedirectToAction("Index", "Home");
            }

            ViewBag.Echafaudages = new SelectList(_context.Echafaudages, "Id", "Nom");
            ViewBag.Accessoires = new MultiSelectList(_context.Accessoires, "Id", "Nom");
            return View(reservation);
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

                return RedirectToAction("Index", "Home");
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