using C__GestionDepenses.Data;
using C__GestionDepenses.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace C__GestionDepenses.Controllers
{
    [Authorize(Roles = "Responsable")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Admin
        public async Task<IActionResult> Index()
        {
            var users = await _context.Users
                .Select(u => new UserFinanceSummaryViewModel
                {
                    UserId = u.Id,
                    Email = u.Email ?? u.UserName ?? string.Empty,
                    FullName = u.FullName ?? string.Empty,
                    TotalRevenus = _context.Revenus
                        .Where(r => r.UserId == u.Id)
                        .Sum(r => (decimal?)r.Montant) ?? 0m,
                    TotalDepenses = _context.Depenses
                        .Where(d => d.UserId == u.Id)
                        .Sum(d => (decimal?)d.Montant) ?? 0m
                })
                .OrderBy(u => u.Email)
                .ToListAsync();

            var vm = new AdminDashboardViewModel { Users = users };
            return View(vm);
        }
    }
}
