using Batibatlocation.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Batibatlocation.Helpers
{
	public static class Calcules
	{
		public static decimal PrixDiscount(decimal prezzo, Promotion promotion)
		{
            if (promotion.IsPercentage)
                prezzo = prezzo - (prezzo * promotion.DiscountValue / 100);
            else
                prezzo = prezzo - promotion.DiscountValue;

            return SansVirgule(prezzo);
        }

        public static decimal SansVirgule(decimal prezzo)
        {
            // Arrotonda a due cifre decimali
            prezzo = Math.Round(prezzo, 0, MidpointRounding.AwayFromZero);

            // Verifica se il numero ha .00 alla fine
            if (Decimal.Remainder(prezzo, 1) == 0)
            {
                // Restituisci solo la parte intera
                return Math.Floor(prezzo);
            }

            // Altrimenti, restituisci il numero con due cifre decimali
            return prezzo;
        }
	}
}