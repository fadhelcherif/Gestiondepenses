namespace C__GestionDepenses.Models
    {
        public class Revenu
        {
            public int Id { get; set; }
            public string Description { get; set; }
            public decimal Montant { get; set; }
            public DateTime Date { get; set; }

            public int CategorieId { get; set; }
            [Microsoft.AspNetCore.Mvc.ModelBinding.Validation.ValidateNever]
            public Categorie Categorie { get; set; }

            public string? UserId { get; set; }
            [Microsoft.AspNetCore.Mvc.ModelBinding.Validation.ValidateNever]
            public User? User { get; set; }
        }
    }
