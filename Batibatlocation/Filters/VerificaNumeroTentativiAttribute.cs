using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;

namespace Batibatlocation.Filters
{
	public class VerificaNumeroTentativiAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            string filePath = HttpContext.Current.Server.MapPath("~/App_Data/otp.txt");

            if (File.Exists(filePath))
            {
                // Legge tutte le righe e prende l'ultima
                var lines = File.ReadAllLines(filePath);
                if (lines.Length > 0)
                {
                    string lastLine = lines.Last(); // Ultima riga del file

                    string[] parts = lastLine.Split(':');
                    if (parts.Length == 2 && int.TryParse(parts[1].Trim(), out int tentativi))
                    {
                        if (tentativi >= 5)
                        {
                            // Blocca l'accesso e reindirizza con messaggio di errore
                            var controller = filterContext.Controller as Controller;
                            if (controller != null)
                            {
                                controller.TempData["ErrorMessage"] = "Nombre maximum de tentatives atteint.";
                                controller.TempData["ErrorMessageDetail"] = "Veuillez contacter l'administrateur.";
                            }

                            filterContext.Result = new RedirectToRouteResult(
                                new System.Web.Routing.RouteValueDictionary
                                {
                                    { "controller", "Admin" },
                                    { "action", "Login" }
                                });

                            return;
                        }
                    }
                }
            }

            base.OnActionExecuting(filterContext);
        }
    }
}