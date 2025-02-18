using Batibatlocation.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Batibatlocation.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Admin/Products
        public ActionResult Products()
        {
            var products = _context.Products.ToList();
            return View(products);
        }

        // GET: Admin/Bookings
        public ActionResult Bookings()
        {
            var bookings = _context.Bookings.ToList();
            return View(bookings);
        }
    }
}