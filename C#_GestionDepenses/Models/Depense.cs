namespace C__GestionDepenses.Models
{
    public class Depense
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public decimal Montant { get; set; }
        public DateTime Date { get; set; }

        public int CategorieId { get; set; }
        public Categorie Categorie { get; set; }

        public string UserId { get; set; }
        public User User { get; set; }
    }
}
