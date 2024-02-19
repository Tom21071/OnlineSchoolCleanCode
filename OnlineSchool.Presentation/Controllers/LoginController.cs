using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OnlineSchool.Domain.Entities;
using OnlineSchool.Presentation.Models.Login;

namespace OnlineSchool.Presentation.Controllers
{
    public class LoginController : Controller
    {
        public UserManager<AppUser> _userManager;
        public SignInManager<AppUser> _signInManager;

        public LoginController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [AllowAnonymous]
        public IActionResult Login()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user != null && await _userManager.CheckPasswordAsync(user, model.Password))
            {
                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction("Dashboard", "Home");
            }
            else
            {
                ViewBag.LoginError = "login or password is incorrect";
                return View(model);
            }
        }

        [Authorize]
        public IActionResult LogOut()
        {
            _signInManager.SignOutAsync();
            return RedirectToAction("Login", "Login");
        }
    }
}
