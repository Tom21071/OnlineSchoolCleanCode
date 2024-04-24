using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OnlineSchool.Domain.Contexts;
using OnlineSchool.Domain.Entities;
using OnlineSchool.Presentation.Models.Login;

namespace OnlineSchool.Presentation.Controllers
{
    public class LoginController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly AppDbContext _appDbContext;

        public LoginController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, AppDbContext appDbContext)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _appDbContext = appDbContext;
        }

        [AllowAnonymous]
        public async Task<IActionResult> Login()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user != null)
            {
                var result = await _signInManager.PasswordSignInAsync(user, model.Password, true, true);

                if (result.Succeeded)
                {
                    string VisitorsIPAddr = "not defined";
                    var httpContext = HttpContext;
                    if (httpContext.Request.Headers.TryGetValue("HTTP_X_FORWARDED_FOR", out var forwardedFor))
                    {
                        VisitorsIPAddr = forwardedFor.FirstOrDefault();
                    }
                    else if (!string.IsNullOrEmpty(httpContext.Connection.RemoteIpAddress?.ToString()))
                    {
                        VisitorsIPAddr = httpContext.Connection.RemoteIpAddress.ToString();
                    }

                    Logins login = new();
                    login.Date = DateTime.Now;
                    login.Email = model.Email;
                    login.Ip = VisitorsIPAddr;
                    await _appDbContext.Logins.AddAsync(login);
                    await _appDbContext.SaveChangesAsync();

                    await _signInManager.SignInAsync(user, isPersistent: true);
                    return RedirectToAction("Dashboard", "Home");
                }
                if (result.IsLockedOut)
                {
                    if(user.LockoutEnd == DateTime.MaxValue)
                    {
                        ViewBag.Error = "Your account was blocked, contact the administrator for the additional information";
                    }
                    else
                    {
                        ViewBag.Error = "Too many login attempts, you can try again in 10 minutes";
                    }
                   
                    return View();
                }
            }
            ViewBag.Error = "Wrong credentials";
            return View();
        }

        [Authorize]
        public async Task<IActionResult> LogOut()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login", "Login");
        }
    }
}
