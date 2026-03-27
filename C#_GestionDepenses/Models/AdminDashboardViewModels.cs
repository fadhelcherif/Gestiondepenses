using System.Collections.Generic;
using System.Linq;

namespace C__GestionDepenses.Models
{
    public class CategoryBudgetItemViewModel
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public decimal Seuil { get; set; }
        public decimal SpentThisMonth { get; set; }
        public decimal Remaining => Seuil - SpentThisMonth;
        public decimal PercentUsed => Seuil <= 0 ? 0 : (SpentThisMonth / Seuil) * 100m;
        public bool IsOver => SpentThisMonth > Seuil;
    }

    public class UserFinanceSummaryViewModel
    {
        public string UserId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;

        public decimal TotalRevenus { get; set; }
        public decimal TotalDepenses { get; set; }
        public decimal Balance => TotalRevenus - TotalDepenses;

        public decimal ThisMonthRevenus { get; set; }
        public decimal ThisMonthDepenses { get; set; }
        public decimal ThisMonthBalance => ThisMonthRevenus - ThisMonthDepenses;

        public string? TopDepenseDescription { get; set; }
        public decimal? TopDepenseMontant { get; set; }
        public string? TopDepenseCategorie { get; set; }

        public string? TopRevenuDescription { get; set; }
        public decimal? TopRevenuMontant { get; set; }
        public string? TopRevenuCategorie { get; set; }

        public List<CategoryBudgetItemViewModel> BudgetsThisMonth { get; set; } = new();
    }

    public class AdminDashboardViewModel
    {
        public List<UserFinanceSummaryViewModel> Users { get; set; } = new();

        public decimal TotalRevenus => Users.Sum(u => u.TotalRevenus);
        public decimal TotalDepenses => Users.Sum(u => u.TotalDepenses);
        public decimal Balance => TotalRevenus - TotalDepenses;

        public decimal ThisMonthRevenus => Users.Sum(u => u.ThisMonthRevenus);
        public decimal ThisMonthDepenses => Users.Sum(u => u.ThisMonthDepenses);
        public decimal ThisMonthBalance => ThisMonthRevenus - ThisMonthDepenses;
    }
}
