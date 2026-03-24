using C__GestionDepenses.Data;
using C__GestionDepenses.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Security.Claims;

namespace C__GestionDepenses.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Dashboard
        public async Task<IActionResult> Index()
        {
            if (User.IsInRole("Responsable"))
                return RedirectToAction("Index", "Admin");

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
                return Challenge();

            var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
                return Challenge();

            var now = DateTime.Now;
            var monthStart = new DateTime(now.Year, now.Month, 1);
            var nextMonthStart = monthStart.AddMonths(1);

            var summary = new UserFinanceSummaryViewModel
            {
                UserId = user.Id,
                Email = user.Email ?? user.UserName ?? string.Empty,
                FullName = user.FullName ?? string.Empty,
                TotalRevenus = await _context.Revenus
                    .Where(r => r.UserId == user.Id)
                    .SumAsync(r => (decimal?)r.Montant) ?? 0m,
                TotalDepenses = await _context.Depenses
                    .Where(d => d.UserId == user.Id)
                    .SumAsync(d => (decimal?)d.Montant) ?? 0m,

                ThisMonthRevenus = await _context.Revenus
                    .Where(r => r.UserId == user.Id && r.Date >= monthStart && r.Date < nextMonthStart)
                    .SumAsync(r => (decimal?)r.Montant) ?? 0m,
                ThisMonthDepenses = await _context.Depenses
                    .Where(d => d.UserId == user.Id && d.Date >= monthStart && d.Date < nextMonthStart)
                    .SumAsync(d => (decimal?)d.Montant) ?? 0m,
            };

            var topDepense = await _context.Depenses
                .AsNoTracking()
                .Include(d => d.Categorie)
                .Where(d => d.UserId == user.Id)
                .OrderByDescending(d => d.Montant)
                .Select(d => new { d.Description, d.Montant, CategorieNom = d.Categorie != null ? d.Categorie.Nom : null })
                .FirstOrDefaultAsync();

            if (topDepense != null)
            {
                summary.TopDepenseDescription = topDepense.Description;
                summary.TopDepenseMontant = topDepense.Montant;
                summary.TopDepenseCategorie = topDepense.CategorieNom;
            }

            var topRevenu = await _context.Revenus
                .AsNoTracking()
                .Include(r => r.Categorie)
                .Where(r => r.UserId == user.Id)
                .OrderByDescending(r => r.Montant)
                .Select(r => new { r.Description, r.Montant, CategorieNom = r.Categorie != null ? r.Categorie.Nom : null })
                .FirstOrDefaultAsync();

            if (topRevenu != null)
            {
                summary.TopRevenuDescription = topRevenu.Description;
                summary.TopRevenuMontant = topRevenu.Montant;
                summary.TopRevenuCategorie = topRevenu.CategorieNom;
            }

            var vm = new AdminDashboardViewModel
            {
                Users = new List<UserFinanceSummaryViewModel> { summary }
            };

            return View(vm);
        }
    }
}
