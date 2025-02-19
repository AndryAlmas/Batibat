using System.Collections.Generic;
using System;
using System.Linq;
using System.Net.Mail;
using System.Web.Mvc;
using Batibatlocation.Data;
using Batibatlocation.Models;

namespace Batibatlocation.Controllers
{
    public class EchafaudageController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EchafaudageController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Echafaudage/Details/{id}
        public ActionResult Details(int id)
        {
            var echafaudage = _context.Echafaudages.Find(id);
            if (echafaudage == null)
            {
                return HttpNotFound();
            }
            return View(echafaudage);
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