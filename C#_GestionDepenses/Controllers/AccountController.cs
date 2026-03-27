using C__GestionDepenses.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace C__GestionDepenses.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AccountController(UserManager<User> userManager, SignInManager<User> signInManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }


        // GET: /Account/Register
        [Authorize(Roles = "Responsable")]
        public IActionResult Register()
        {
            ViewBag.Roles = _roleManager.Roles
                .Select(r => r.Name)
                .Where(n => !string.IsNullOrWhiteSpace(n))
                .ToList();
            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        [Authorize(Roles = "Responsable")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Roles = _roleManager.Roles
                    .Select(r => r.Name)
                    .Where(n => !string.IsNullOrWhiteSpace(n))
                    .ToList();
                return View(model);
            }

            var user = new User { UserName = model.Email, Email = model.Email, FullName = model.FullName };
            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                if (!await _roleManager.RoleExistsAsync(model.Role))
                {
                    ModelState.AddModelError(nameof(model.Role), "Invalid role.");
                    ViewBag.Roles = _roleManager.Roles
                        .Select(r => r.Name)
                        .Where(n => !string.IsNullOrWhiteSpace(n))
                        .ToList();
                    return View(model);
                }

                await _userManager.AddToRoleAsync(user, model.Role);
                // Do not sign in the new user automatically
                return RedirectToAction("Index", "Home");
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);

            ViewBag.Roles = _roleManager.Roles
                .Select(r => r.Name)
                .Where(n => !string.IsNullOrWhiteSpace(n))
                .ToList();
            return View(model);
        }

        // GET: /Account/Login
        public IActionResult Login()
        {
            if (User?.Identity?.IsAuthenticated == true)
                return User.IsInRole("Responsable")
                    ? RedirectToAction("Index", "Admin")
                    : RedirectToAction("Index", "Dashboard");
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, false, false);

            if (result.Succeeded)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user != null && await _userManager.IsInRoleAsync(user, "Responsable"))
                    return RedirectToAction("Index", "Admin");

                return RedirectToAction("Index", "Dashboard");
            }

            ModelState.AddModelError("", "Invalid login attempt");
            return View(model);
        }

        // POST: /Account/Logout
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login", "Account");
        }
    }
}