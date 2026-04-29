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
            var currentUser = _userManager.GetUserAsync(User).Result;
            if (currentUser == null)
                return NotFound();

            var reviews = _db.Reviews
                .Where(r => r.RevieweeId == currentUser.Id)
                .Include(r => r.Reviewer)
                .OrderByDescending(r => r.CreatedAt)
                .ToList();

            var wallet = _db.Wallets.FirstOrDefault(w => w.UserId == currentUser.Id);

            var vm = new ProfileViewModel
            {
                User = currentUser,
                Reviews = reviews,
                WalletAvailable = wallet?.AvailableBalance ?? 0m,
                WalletEscrow = wallet?.EscrowBalance ?? 0m,
                WalletCurrency = wallet?.Currency ?? "EGP",
            };
            return View(vm);
        }

        //-------EditProfile--------------//
        [HttpGet]
        [Authorize]
        public IActionResult EditProfile()
        {
            var currentUser = _userManager.GetUserAsync(User).Result;
            if (currentUser == null)
                return NotFound();

            var vm = new EditProfileViewModel
            {
                FullName = currentUser.FullName,
                Bio = currentUser.Bio,
                Skills = currentUser.Skills,
                HourlyRate = currentUser.HourlyRate,
                Country = currentUser.Country,
                AvatarUrl = currentUser.AvatarUrl
            };
            return View(vm);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public IActionResult EditProfile(EditProfileViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var currentUser = _userManager.GetUserAsync(User).Result;
            if (currentUser == null)
                return NotFound();

            currentUser.FullName = model.FullName;
            currentUser.Bio = model.Bio;
            currentUser.Skills = model.Skills;
            currentUser.HourlyRate = model.HourlyRate;
            currentUser.Country = model.Country;
            currentUser.AvatarUrl = model.AvatarUrl;

            var result = _userManager.UpdateAsync(currentUser).Result;
            if (result.Succeeded)
            {
                TempData["Success"] = "Profile updated successfully!";
                return RedirectToAction("Profile");
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            return View(model);
        }
    }
}