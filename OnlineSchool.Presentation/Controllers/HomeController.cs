using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineSchool.Domain.Contexts;
using OnlineSchool.Domain.Entities;
using OnlineSchool.Presentation.Models;
using OnlineSchool.Presentation.Models.Common;
using System.Diagnostics;

namespace OnlineSchool.Presentation.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;
        public UserManager<AppUser> _userManager;
        public SignInManager<AppUser> _signInManager;

        public HomeController(AppDbContext context, UserManager<AppUser> userManager, SignInManager<AppUser> signInManager)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public IActionResult Index()
        {
            return View(_context.Notifications.FirstOrDefault(x => x.Title == "Work"));
        }

        public IActionResult Dashboard()
        {
            return View();
        }

        public IActionResult PersonalCabnet()
        {
            PersonalCabnetModel personalCabnetModel = new PersonalCabnetModel();
            personalCabnetModel.User = _context.Users.Include(x => x.Classes).FirstOrDefault(x => x.Email == User.Identity.Name);
            List<UserClass> userClasses = _context.UserClasses.Include(x => x.Class).Include(x => x.User).Where(x => x.User.Email == User.Identity.Name).ToList();
            personalCabnetModel.UserClasses = userClasses;
            int tommorow = (int)DateTime.Today.DayOfWeek + 1;
            personalCabnetModel.Lessons = _context.Lessons.Include(s => s.Subject).Where(x => x.DayOfTheWeek == (byte)tommorow).ToList();
            return View(personalCabnetModel);
        }
    }
}
