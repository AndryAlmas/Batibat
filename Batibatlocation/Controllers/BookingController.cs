using Batibatlocation.Data;
using Batibatlocation.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Batibatlocation.Controllers
{
    public class BookingController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BookingController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Booking/Create
        public ActionResult Create(int productId)
        {
            ViewBag.ProductId = productId;
            return View();
        }

        // POST: Booking/Create
        [HttpPost]
        public ActionResult Create(Booking booking)
        {
            if (ModelState.IsValid)
            {
                _context.Bookings.Add(booking);
                _context.SaveChanges();

                // Invia conferma via email
                SendConfirmationEmail(booking.Email);

                return RedirectToAction("Index", "Home");
            }
            return View(booking);
        }

        private void SendConfirmationEmail(string email)
        {
            // Logica per inviare email
        }
    }
}