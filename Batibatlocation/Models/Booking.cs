using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Batibatlocation.Models
{
    public class Booking
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public Product Product { get; set; }
    }
}