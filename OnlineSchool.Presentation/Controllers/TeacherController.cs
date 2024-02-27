using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineSchool.Domain.Contexts;
using OnlineSchool.Domain.Entities;
using OnlineSchool.Presentation.Models.Common;

namespace OnlineSchool.Presentation.Controllers
{
    [Authorize(Roles = "Teacher")]
    public class TeacherController : Controller
    {
        private readonly AppDbContext _context;
        public UserManager<AppUser> _userManager;
        public SignInManager<AppUser> _signInManager;

        public TeacherController(AppDbContext context, UserManager<AppUser> userManager, SignInManager<AppUser> signInManager)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public async Task<IActionResult> SendNotification()
        {
            NotificationModel notificationModel = new NotificationModel();
            notificationModel.Classes = await _context.Classes.ToListAsync();
            notificationModel.ClassesChecked = new List<bool>(new bool[notificationModel.Classes.Count]);
            return View(notificationModel);
        }

        [HttpPost]
        public async Task<IActionResult> SendNotification(NotificationModel notificationModel)
        {
            Notification notification = new Notification();
            notification.SenderId = (await _context.Users.FirstOrDefaultAsync(x => x.Email == User.Identity.Name)).Id;
            notification.CreatedAt = DateTime.Now;
            notification.Title = notificationModel.Title;
            notification.Description = notificationModel.Description;

            await _context.Notifications.AddAsync(notification);
            await _context.SaveChangesAsync();
            for (int i = 0; i < notificationModel.Classes.Count; i++)
            {
                if (notificationModel.ClassesChecked[i] == true)
                {
                    var students = _context.UserClasses.Where(x => x.ClassId == notificationModel.Classes[i].Id);
                    foreach (var student in students)
                    {
                        UserNotification un = new UserNotification();
                        un.IsRead = false;
                        un.NotificationId = notification.Id;
                        un.RecieverId = student.UserId;
                        await _context.UserNotifications.AddAsync(un);
                    }
                }
            }
            await _context.SaveChangesAsync();
            return RedirectToAction("AddedNotification", "Teacher");
        }

        public async Task<IActionResult> AddedNotification()
        {
            return View();
        }

        public async Task<IActionResult> TeacherRegisters()
        {
            var teacher = await _context.Users.FirstOrDefaultAsync(x => x.Email == User.Identity.Name);
            if (teacher != null)
            {
                List<Subject> subjects = _context.Subjects.Where(x => x.TeacherId == teacher.Id).Include(x => x.Class).ToList();
                return View(subjects);
            }
            return RedirectToAction("Custom404", "Home");
        }
        public async Task<IActionResult> SubjectRegister(int subjectId)
        {
            var teacher = await _context.Users.FirstOrDefaultAsync(x => x.Email == User.Identity.Name);
            Subject? subject = await _context.Subjects.FirstOrDefaultAsync(x => x.Id == subjectId);
            if (subject == null || subject.TeacherId != teacher.Id)
            {
                return RedirectToAction("Custom404", "Home");
            }
            Class? c = await _context.Classes.FirstOrDefaultAsync(x => x.Id == subject.ClassId);
            List<UserClass> classUsers = _context.UserClasses.Where(x => x.ClassId == c.Id).Include(x => x.User).OrderBy(x => x.User.LastName).ToList();
            ViewBag.SubjectId = subjectId;
            return View(classUsers);
        }
        public async Task<IActionResult> Schedule()
        {
            List<ScheduleModel> scheduleModels = new List<ScheduleModel>();
            List<Class> classes = await _context.Classes.OrderBy(x => x.Name).ToListAsync();
            foreach (var item in classes)
            {

                List<Lesson> lessons = await _context.Lessons.Where(x => x.ClassId == item.Id).Include(x => x.Subject).ToListAsync();
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

                if (User.IsInRole("Teacher"))
                {
                    var teacherId = (await _context.Users.FirstOrDefaultAsync(x => x.Email == User.Identity.Name)).Id;
                    var classes = _context.Classes.Where(x => x.TeacherId == teacherId);
                    foreach (var item in classes)
                    {
                        userClasses.Add(new UserClass { ClassId = item.Id, Class = item, UserId = teacherId });
                    }
                    return View(userClasses);
                }
                return View(userClasses);
            }
            return RedirectToAction("Login", "Login");
        }

        public async Task<IActionResult> ClassSubjects(int classId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == User.Identity.Name);
            if (user != null)
            {
                if (User.IsInRole("Teacher"))
                    if (await _context.Classes.FirstOrDefaultAsync(x => x.TeacherId == user.Id && x.Id == classId) == null)
                        return RedirectToAction("YouDontBelong", "Home");

                ClassSubjectsModel classSubjectsModel = new ClassSubjectsModel();
                classSubjectsModel.Subjects = await _context.Subjects.Include(u => u.Class).Where(x => x.ClassId == classId).ToListAsync();
                classSubjectsModel.Teacher = await _context.Users.FirstOrDefaultAsync(x => x.Id == _context.Classes.FirstOrDefault(y => y.Id == classId).TeacherId);
                List<UserClass> userClasses = await _context.UserClasses.Include(u => u.User).Where(x => x.ClassId == classId).ToListAsync();
                classSubjectsModel.Students = new List<AppUser>();

                foreach (var item in userClasses)
                {
                    classSubjectsModel.Students.Add(item.User);
                }
                return View(classSubjectsModel);
            }
            return RedirectToAction("YouDontBelong", "Home");
        }
    }
}
