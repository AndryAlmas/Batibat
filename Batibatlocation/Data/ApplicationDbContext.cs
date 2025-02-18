using Batibatlocation.Models;
using System.Data.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Batibatlocation.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext()
            : base("ArubaDB")
        {
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<Booking> Bookings { get; set; }
    }
}