using Batibatlocation.Enum;
using Batibatlocation.Models;
using System;
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

        protected override void Seed(Batibatlocation.Data.ApplicationDbContext context)
        {
            //  This method will be called after migrating to the latest version.

            // Popola la tabella Periodicites
            if (!context.Periodicites.Any())
            {
                context.Periodicites.AddRange(System.Enum.GetValues(typeof(PeriodicityType))
                    .Cast<PeriodicityType>()
                    .Select(e => new Periodicite { Id = (int)e, Nom = e.ToString() })
                );

                context.SaveChanges();
            }

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method
            //  to avoid creating duplicate seed data.
        }
    }
}
