using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json; // Assicurati di aggiungere questa libreria per la serializzazione

namespace Batibatlocation.Helpers
{
    public static class SlugName
    {
        // Imposta un messaggio con un tipo di alert

        public static string GenerateSlug(int id, string input)
        {
            if (string.IsNullOrEmpty(input))
                return "";

            // Rimuovi spazi multipli e sostituisci con un trattino
            string slug = input.Trim().ToLower();
            slug = System.Text.RegularExpressions.Regex.Replace(slug, @"\s+", "-");
            slug = System.Text.RegularExpressions.Regex.Replace(slug, @"[^a-z0-9\-]", ""); // Rimuovi caratteri non validi
            return id + "-" + slug;
        }

    }
}