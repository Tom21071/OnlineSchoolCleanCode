using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineSchool.Domain.Contexts;
using OnlineSchool.Domain.Entities;
using OnlineSchool.Presentation.Models.Common;

namespace OnlineSchool.Presentation.Controllers
{
    public class StudentController : Controller
    {
        private readonly AppDbContext _context;
        public UserManager<AppUser> _userManager;
        public SignInManager<AppUser> _signInManager;

        public StudentController(AppDbContext context, UserManager<AppUser> userManager, SignInManager<AppUser> signInManager)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public IActionResult ClassRegister()
        {
            return View();
        }

        public IActionResult Schedule()
        {
            List<ScheduleModel> scheduleModels = new List<ScheduleModel>();
            List<Class> classes = _context.Classes.OrderBy(x => x.Name).ToList();
            foreach (var item in classes)
            {

                List<Lesson> lessons = _context.Lessons.Where(x => x.ClassId == item.Id).Include(x => x.Subject).ToList();
                ScheduleModel sm = new ScheduleModel();
                sm.Class = item;
                sm.Monday = lessons.Where(x => x.DayOfTheWeek == 1).OrderBy(x => x.Start).ToList();
                sm.Tuesday = lessons.Where(x => x.DayOfTheWeek == 2).OrderBy(x => x.Start).ToList();
                sm.Wednsday = lessons.Where(x => x.DayOfTheWeek == 3).OrderBy(x => x.Start).ToList();
                sm.Thursday = lessons.Where(x => x.DayOfTheWeek == 4).OrderBy(x => x.Start).ToList();
                sm.Friday = lessons.Where(x => x.DayOfTheWeek == 5).OrderBy(x => x.Start).ToList();
                scheduleModels.Add(sm);
            }
            return View(scheduleModels);
        }

        public async Task<IActionResult> Classes()
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == User.Identity.Name);

            if (user != null)
            {
                List<UserClass> userClasses = new();
                if (User.IsInRole("Student"))
                {
                    userClasses = _context.UserClasses.Include(u => u.Class).Where(x => x.UserId == user.Id).ToList();
                    return View(userClasses);
                }
                else if (User.IsInRole("Teacher"))
                {
                    var teacherId = _context.Users.FirstOrDefault(x => x.Email == User.Identity.Name).Id;
                    var classes = _context.Classes.Where(x => x.TeacherId == teacherId);
                    foreach (var item in classes)
                    {
                        userClasses.Add(new UserClass { ClassId = item.Id, Class = item, UserId = teacherId });
                    }
                    return View(userClasses);
                }
                return View(userClasses);
            }
            return RedirectToAction("Login");
        }

        public async Task<IActionResult> ClassSubjects(int classId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == User.Identity.Name);
            if (user != null)
            {

                if (User.IsInRole("Student"))
                    if (_context.UserClasses.FirstOrDefault(x => x.UserId == user.Id && x.ClassId == classId) == null)
                        return Forbid();

                if (User.IsInRole("Teacher"))
                    if (_context.Classes.FirstOrDefault(x => x.TeacherId == user.Id && x.Id == classId) == null)
                        return Forbid();

                ClassSubjectsModel classSubjectsModel = new ClassSubjectsModel();
                classSubjectsModel.Subjects = _context.Subjects.Include(u => u.Class).Where(x => x.ClassId == classId).ToList();
                classSubjectsModel.Teacher = _context.Users.FirstOrDefault(x => x.Id == _context.Classes.FirstOrDefault(y => y.Id == classId).TeacherId);
                List<UserClass> userClasses = _context.UserClasses.Include(u => u.User).Where(x => x.ClassId == classId).ToList();
                classSubjectsModel.Students = new List<AppUser>();

                foreach (var item in userClasses)
                {
                    classSubjectsModel.Students.Add(item.User);
                }
                return View(classSubjectsModel);
            }
            return Forbid();
        }
    }
}
