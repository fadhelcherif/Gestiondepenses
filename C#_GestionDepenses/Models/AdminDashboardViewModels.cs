using System.Collections.Generic;
using System.Linq;

namespace C__GestionDepenses.Models
{
    public class UserFinanceSummaryViewModel
    {
        public string UserId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;

        public decimal TotalRevenus { get; set; }
        public decimal TotalDepenses { get; set; }
        public decimal Balance => TotalRevenus - TotalDepenses;
    }

    public class AdminDashboardViewModel
    {
        public List<UserFinanceSummaryViewModel> Users { get; set; } = new();

        public decimal TotalRevenus => Users.Sum(u => u.TotalRevenus);
        public decimal TotalDepenses => Users.Sum(u => u.TotalDepenses);
        public decimal Balance => TotalRevenus - TotalDepenses;
    }
}
