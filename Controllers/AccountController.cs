using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Workify_Full.Data;
using Workify_Full.Models;
using Workify_Full.ViewModels;

namespace Workify_Full.Controllers
{
    public class AccountController : Controller
    {

        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private ApplicationUser user;

        //Controller
        public AccountController(
            ApplicationDbContext db,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _db = db;
            _userManager = userManager;
            _signInManager = signInManager;
        }
        //-------Login--------------//
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);
            var result = _signInManager.PasswordSignInAsync(
                model.Email, model.Password, model.RememberMe, lockoutOnFailure: false
                ).Result;

            if (result.Succeeded)
            {
                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return View(model);
        }

        //-------Register--------------//
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);
            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName,
                Role = model.Role
            };
            var result = _userManager.CreateAsync(user, model.Password).Result;
            if (result.Succeeded)
            {
                _signInManager.SignInAsync(user, isPersistent: true).Wait();
                return RedirectToAction("Index", "Home");
            }
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return View(model);
        }


        //-------Logout--------------//

        [HttpPost]
        public IActionResult Logout()
        {
            _signInManager.SignOutAsync().Wait();
            return RedirectToAction("Login", "Account");

        }

        //-------Profile--------------//
        [HttpGet]
        [Authorize]
        public IActionResult Profile()
        {
            var userId = _userManager.GetUserId(User); ///GEt users id from the current logged in user
            if (userId == null)
            {
                return NotFound();
            }
            var reviews = _db.Reviews
                .Where(r => r.RevieweeId == userId)
                .Include(r => r.Reviewer)
                .OrderByDescending(r => r.CreatedAt)
                .ToList();

            var wallet = _db.Wallets.FirstOrDefault(w => w.UserId == userId);

            var vm = new ProfileViewModel
            {
                User = user,
                Reviews = reviews,
                WalletAvailable = wallet?.AvailableBalance ?? 0m,
                WalletEscrow = wallet?.EscrowBalance ?? 0m,
                WalletCurrency = wallet?.Currency ?? "EGP",
            };
            return View(vm);
        }






















    }
}