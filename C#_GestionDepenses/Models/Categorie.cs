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

        // Navigation properties
        public ICollection<Revenu> Revenus { get; set; }
        public ICollection<Depense> Depenses { get; set; }
    }
}