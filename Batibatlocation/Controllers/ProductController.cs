using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Batibatlocation.Data;

namespace Batibatlocation.Controllers
{
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _context;
        public ProductController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Product/Index
        public ActionResult Index()
        {
            var products = _context.Products.ToList();
            return View(products);
        }

        // GET: Product/Details/{id}
        public ActionResult Details(int id)
        {
            var product = _context.Products.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            return View(product);
        }
    }
}