using Batibatlocation.Enum;
using Batibatlocation.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;

namespace Batibatlocation.Data
{
    internal sealed class Configuration : DbMigrationsConfiguration<Batibatlocation.Data.ApplicationDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(ApplicationDbContext context)
        {
            // Popola la tabella Periodicite
            if (!context.Periodicites.Any())
            {
                context.Periodicites.AddRange(System.Enum.GetValues(typeof(Enum.PeriodicityType))
                    .Cast<Enum.PeriodicityType>()
                    .Select(e => new Models.Periodicite { Id = (int)e, Nom = e.ToString() })
                );

                context.SaveChanges();
            }

            // Popola la tabella Category
            if (!context.Categories.Any())
            {
                context.Categories.AddRange(System.Enum.GetValues(typeof(Enum.CategoryType))
                    .Cast<Enum.CategoryType>()
                    .Select(e => new Models.Category { Id = (int)e, Nom = e.ToString() })
                );

                context.SaveChanges();
            }

            // Popola la tabella CategoryDocuments
            if (!context.CategoriesDocuments.Any())
            {
                context.CategoriesDocuments.AddRange(System.Enum.GetValues(typeof(Enum.CategoryDocument))
                    .Cast<Enum.CategoryDocument>()
                    .Select(e => new Models.CategoryDocument { Id = (int)e, Nom = e.ToString() })
                );

                context.SaveChanges();
            }

            // Popola la tabella CategoryClients
            if (!context.CategoriesClients.Any())
            {
                context.CategoriesClients.AddRange(System.Enum.GetValues(typeof(Enum.CategoryClient))
                    .Cast<Enum.CategoryClient>()
                    .Select(e => new Models.CategoryClient { Id = (int)e, Nom = e.ToString() })
                );

                context.SaveChanges();
            }

            // Popola la tabella EtatFactures
            if (!context.EtatFactures.Any())
            {
                context.EtatFactures.AddRange(System.Enum.GetValues(typeof(Enum.EtatFacture))
                    .Cast<Enum.EtatFacture>()
                    .Select(e => new Models.EtatFacture { Id = (int)e, Nom = e.ToString() })
                );

                context.SaveChanges();
            }
        }
    }
}
