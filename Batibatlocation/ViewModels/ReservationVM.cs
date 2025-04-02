using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Batibatlocation.ViewModels
{
	public class ReservationVM
	{
        public string ProdName { get; set; }
        public string UserType { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public bool? DeliveryEnabled { get; set; }
        public double? EndLat { get; set; }
        public double? EndLon { get; set; }
    }
}