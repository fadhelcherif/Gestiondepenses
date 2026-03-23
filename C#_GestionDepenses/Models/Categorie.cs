using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace C__GestionDepenses.Models
{
    public enum CategorieType
    {
        Depense,  // 0
        Revenu    // 1
    }

    public class Categorie
    {
        public int Id { get; set; }
        public string Nom { get; set; }           // Name
        public string? Description { get; set; }
        public CategorieType Type { get; set; }   // Depense or Revenu

        public decimal? Seuil { get; set; }

        // Navigation properties
        [ValidateNever]
        public ICollection<Revenu> Revenus { get; set; }
        [ValidateNever]
        public ICollection<Depense> Depenses { get; set; }
    }
}
