using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using C__GestionDepenses.Data;
using C__GestionDepenses.Models;

namespace C__GestionDepenses.Controllers
{
    [Authorize]
    public class DepensesController : Controller
    {
        private readonly ApplicationDbContext _context;

        private bool IsResponsable => User.IsInRole("Responsable");

        private string? CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier);

        public DepensesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Depenses
        public async Task<IActionResult> Index()
        {
            var query = _context.Depenses
                .Include(d => d.Categorie)
                .Include(d => d.User)
                .AsQueryable();

            if (!IsResponsable)
            {
                var uid = CurrentUserId;
                if (string.IsNullOrWhiteSpace(uid))
                    return Challenge();

                query = query.Where(d => d.UserId == uid);
            }

            return View(await query.ToListAsync());
        }

        // GET: Depenses/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var depense = await _context.Depenses
                .Include(d => d.Categorie)
                .Include(d => d.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (depense == null)
            {
                return NotFound();
            }

            if (!IsResponsable)
            {
                var uid = CurrentUserId;
                if (string.IsNullOrWhiteSpace(uid))
                    return Challenge();

                if (!string.Equals(depense.UserId, uid, StringComparison.Ordinal))
                    return Forbid();
            }

            return View(depense);
        }

        // GET: Depenses/Create
        public IActionResult Create()
        {
            Console.WriteLine("[DEBUG] Depense Create GET called");
            var depenseCategories = _context.Categories.Where(c => c.Type == CategorieType.Depense).ToList();
            ViewData["CategorieId"] = new SelectList(depenseCategories, "Id", "Nom");
            return View();
        }

        // POST: Depenses/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Description,Montant,Date,CategorieId")] Depense depense)
        {
            Console.WriteLine("[DEBUG] Depense Create POST called");
            var depenseCategories = _context.Categories.Where(c => c.Type == CategorieType.Depense).ToList();

            var selectedCategorie = await _context.Categories.AsNoTracking().FirstOrDefaultAsync(c => c.Id == depense.CategorieId);
            if (selectedCategorie == null || selectedCategorie.Type != CategorieType.Depense)
            {
                ModelState.AddModelError(nameof(depense.CategorieId), "Invalid category.");
            }
            else if (selectedCategorie.Seuil.HasValue && depense.Montant > selectedCategorie.Seuil.Value)
            {
                ModelState.AddModelError(nameof(depense.Montant), $"Montant exceeds the allowed threshold ({selectedCategorie.Seuil.Value}).");
            }

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(currentUserId))
            {
                ModelState.AddModelError(string.Empty, "Not authenticated");
            }
            else
            {
                depense.UserId = currentUserId;
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
                ViewData["CategorieId"] = new SelectList(depenseCategories, "Id", "Nom", depense.CategorieId);
                return View(depense);
            }
            Console.WriteLine($"[DEBUG] ModelState VALID - {depense.Description}, {depense.Montant}, {depense.Date}, {depense.CategorieId}, {depense.UserId}");
            try
            {
                _context.Add(depense);
                await _context.SaveChangesAsync();
                Console.WriteLine("[DEBUG] Depense saved successfully");
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ViewBag.DebugMessage = "[DEBUG] SaveChanges failed: " + ex.Message;
                ModelState.AddModelError(string.Empty, "Database error while saving.");
                ViewData["CategorieId"] = new SelectList(depenseCategories, "Id", "Nom", depense.CategorieId);
                return View(depense);
            }
        }

        // GET: Depenses/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var depense = await _context.Depenses.FindAsync(id);
            if (depense == null)
            {
                return NotFound();
            }

            if (!IsResponsable)
            {
                var uid = CurrentUserId;
                if (string.IsNullOrWhiteSpace(uid))
                    return Challenge();

                if (!string.Equals(depense.UserId, uid, StringComparison.Ordinal))
                    return Forbid();
            }

            var depenseCategories = _context.Categories.Where(c => c.Type == CategorieType.Depense).ToList();
            ViewData["CategorieId"] = new SelectList(depenseCategories, "Id", "Nom", depense.CategorieId);
            return View(depense);
        }

        // POST: Depenses/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Description,Montant,Date,CategorieId")] Depense depense)
        {
            if (id != depense.Id)
            {
                return NotFound();
            }

            var selectedCategorie = await _context.Categories.AsNoTracking().FirstOrDefaultAsync(c => c.Id == depense.CategorieId);
            if (selectedCategorie == null || selectedCategorie.Type != CategorieType.Depense)
            {
                ModelState.AddModelError(nameof(depense.CategorieId), "Invalid category.");
            }
            else if (selectedCategorie.Seuil.HasValue && depense.Montant > selectedCategorie.Seuil.Value)
            {
                ModelState.AddModelError(nameof(depense.Montant), $"Montant exceeds the allowed threshold ({selectedCategorie.Seuil.Value}).");
            }

            var existing = await _context.Depenses.AsNoTracking().FirstOrDefaultAsync(d => d.Id == id);
            if (existing == null)
            {
                return NotFound();
            }

            if (!IsResponsable)
            {
                var uid = CurrentUserId;
                if (string.IsNullOrWhiteSpace(uid))
                    return Challenge();

                if (!string.Equals(existing.UserId, uid, StringComparison.Ordinal))
                    return Forbid();
            }

            depense.UserId = existing.UserId;

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

                var depenseCategories = _context.Categories.Where(c => c.Type == CategorieType.Depense).ToList();
                ViewData["CategorieId"] = new SelectList(depenseCategories, "Id", "Nom", depense.CategorieId);
                return View(depense);
            }

            try
            {
                _context.Update(depense);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DepenseExists(depense.Id))
                {
                    return NotFound();
                }
                throw;
            }
            catch (Exception ex)
            {
                ViewBag.DebugMessage = "[DEBUG] SaveChanges failed: " + ex.Message;
                ModelState.AddModelError(string.Empty, "Database error while saving.");
                var depenseCategories = _context.Categories.Where(c => c.Type == CategorieType.Depense).ToList();
                ViewData["CategorieId"] = new SelectList(depenseCategories, "Id", "Nom", depense.CategorieId);
                return View(depense);
            }
        }

        // GET: Depenses/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var depense = await _context.Depenses
                .Include(d => d.Categorie)
                .Include(d => d.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (depense == null)
            {
                return NotFound();
            }

            if (!IsResponsable)
            {
                var uid = CurrentUserId;
                if (string.IsNullOrWhiteSpace(uid))
                    return Challenge();

                if (!string.Equals(depense.UserId, uid, StringComparison.Ordinal))
                    return Forbid();
            }

            return View(depense);
        }

        // POST: Depenses/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var depense = await _context.Depenses.FindAsync(id);
            if (depense == null)
                return NotFound();

            if (!IsResponsable)
            {
                var uid = CurrentUserId;
                if (string.IsNullOrWhiteSpace(uid))
                    return Challenge();

                if (!string.Equals(depense.UserId, uid, StringComparison.Ordinal))
                    return Forbid();
            }

            if (depense != null)
            {
                _context.Depenses.Remove(depense);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DepenseExists(int id)
        {
            return _context.Depenses.Any(e => e.Id == id);
        }
    }
}
