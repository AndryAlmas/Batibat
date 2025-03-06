using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json; // Assicurati di aggiungere questa libreria per la serializzazione

namespace Batibatlocation.Helpers
{
    public static class AlertData
    {
        // Imposta un messaggio con un tipo di alert
        public static void SetAlert(this TempDataDictionary tempData, string key, string message, string alertType)
        {
            var alertObject = new
            {
                Message = HttpUtility.HtmlEncode(message),
                Type = alertType
            };

            tempData[key] = JsonConvert.SerializeObject(alertObject);
        }

        // Recupera il messaggio di alert e il tipo
        public static string GetAlert(this TempDataDictionary tempData, string key)
        {
            if (tempData.ContainsKey(key) && tempData[key] is string jsonData)
            {
                return jsonData;
            }

            return null;
        }
    }
}