using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using C__GestionDepenses.Data;
using C__GestionDepenses.Models;

namespace C__GestionDepenses.Controllers
{
    public class RevenusController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RevenusController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Revenus
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Revenus.Include(r => r.Categorie).Include(r => r.User);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Revenus/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var revenu = await _context.Revenus
                .Include(r => r.Categorie)
                .Include(r => r.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (revenu == null)
            {
                return NotFound();
            }

            return View(revenu);
        }

        // GET: Revenus/Create
        public IActionResult Create()
        {
            Console.WriteLine("[DEBUG] Revenu Create GET called");
            var revenuCategories = _context.Categories.Where(c => c.Type == CategorieType.Revenu).ToList();
            ViewData["CategorieId"] = new SelectList(revenuCategories, "Id", "Nom");
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "UserName");
            return View();
        }

        // POST: Revenus/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Description,Montant,Date,CategorieId")] Revenu revenu)
        {
            Console.WriteLine("[DEBUG] Revenu Create POST called");
            var revenuCategories = _context.Categories.Where(c => c.Type == CategorieType.Revenu).ToList();

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(currentUserId))
            {
                ModelState.AddModelError(string.Empty, "Not authenticated");
            }
            else
            {
                revenu.UserId = currentUserId;
            }

            if (!ModelState.IsValid)
            {
                var debugMsg = "[DEBUG] ModelState INVALID\n";
                foreach (var key in ModelState.Keys)
                {
                    var errors = ModelState[key].Errors;
                    foreach (var error in errors)
                    {
                        debugMsg += $"[DEBUG] ModelState error for {key}: {error.ErrorMessage}\n";
                    }
                }
                ViewBag.DebugMessage = debugMsg;
                ViewData["CategorieId"] = new SelectList(revenuCategories, "Id", "Nom", revenu.CategorieId);
                return View(revenu);
            }
            Console.WriteLine($"[DEBUG] ModelState VALID - {revenu.Description}, {revenu.Montant}, {revenu.Date}, {revenu.CategorieId}, {revenu.UserId}");
            try
            {
                _context.Add(revenu);
                await _context.SaveChangesAsync();
                Console.WriteLine("[DEBUG] Revenu saved successfully");
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ViewBag.DebugMessage = "[DEBUG] SaveChanges failed: " + ex.Message;
                ModelState.AddModelError(string.Empty, "Database error while saving.");
                ViewData["CategorieId"] = new SelectList(revenuCategories, "Id", "Nom", revenu.CategorieId);
                return View(revenu);
            }
        }

        // GET: Revenus/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var revenu = await _context.Revenus.FindAsync(id);
            if (revenu == null)
            {
                return NotFound();
            }
            var revenuCategories = _context.Categories.Where(c => c.Type == CategorieType.Revenu).ToList();
            ViewData["CategorieId"] = new SelectList(revenuCategories, "Id", "Nom", revenu.CategorieId);
            return View(revenu);
        }

        // POST: Revenus/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Description,Montant,Date,CategorieId")] Revenu revenu)
        {
            if (id != revenu.Id)
            {
                return NotFound();
            }

            var existing = await _context.Revenus.AsNoTracking().FirstOrDefaultAsync(r => r.Id == id);
            if (existing == null)
            {
                return NotFound();
            }
            revenu.UserId = existing.UserId;

            if (!ModelState.IsValid)
            {
                var debugMsg = "[DEBUG] ModelState INVALID\n";
                foreach (var key in ModelState.Keys)
                {
                    var errors = ModelState[key].Errors;
                    foreach (var error in errors)
                    {
                        debugMsg += $"[DEBUG] ModelState error for {key}: {error.ErrorMessage}\n";
                    }
                }
                ViewBag.DebugMessage = debugMsg;
                var revenuCategories = _context.Categories.Where(c => c.Type == CategorieType.Revenu).ToList();
                ViewData["CategorieId"] = new SelectList(revenuCategories, "Id", "Nom", revenu.CategorieId);
                return View(revenu);
            }

            try
            {
                _context.Update(revenu);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!RevenuExists(revenu.Id))
                {
                    return NotFound();
                }
                throw;
            }
            catch (Exception ex)
            {
                ViewBag.DebugMessage = "[DEBUG] SaveChanges failed: " + ex.Message;
                ModelState.AddModelError(string.Empty, "Database error while saving.");
                var revenuCategories = _context.Categories.Where(c => c.Type == CategorieType.Revenu).ToList();
                ViewData["CategorieId"] = new SelectList(revenuCategories, "Id", "Nom", revenu.CategorieId);
                return View(revenu);
            }
        }

        // GET: Revenus/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var revenu = await _context.Revenus
                .Include(r => r.Categorie)
                .Include(r => r.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (revenu == null)
            {
                return NotFound();
            }

            return View(revenu);
        }

        // POST: Revenus/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var revenu = await _context.Revenus.FindAsync(id);
            if (revenu != null)
            {
                _context.Revenus.Remove(revenu);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool RevenuExists(int id)
        {
            return _context.Revenus.Any(e => e.Id == id);
        }
    }
}
