using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using C__GestionDepenses.Models;

namespace C__GestionDepenses.Data
{
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Categorie> Categories { get; set; }    
        public DbSet<Revenu> Revenus { get; set; }
        public DbSet<Depense> Depenses { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Seed default categories
            builder.Entity<Categorie>().HasData(
    new Categorie { Id = 1, Nom = "Salaire", Description = "Revenu du travail", Type = CategorieType.Revenu, Seuil = null },
    new Categorie { Id = 2, Nom = "Loyer", Description = "Paiement mensuel du logement", Type = CategorieType.Depense, Seuil = 2000m },
    new Categorie { Id = 3, Nom = "Alimentation", Description = "Courses et repas", Type = CategorieType.Depense, Seuil = 800m }
);
        }
    }
}
