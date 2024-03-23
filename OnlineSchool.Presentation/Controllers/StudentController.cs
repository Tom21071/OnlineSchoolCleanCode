using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using OnlineSchool.Application.EncryptionServiceInterface;
using OnlineSchool.Domain.Contexts;
using OnlineSchool.Domain.Entities;
using OnlineSchool.Infrastructure.EncryptionServiceImplementation;
using OnlineSchool.Presentation.Models.Common;
using OnlineSchool.Presentation.Models.Teacher;

namespace OnlineSchool.Presentation.Controllers
{
    [Authorize(Roles = "Student")]
    public class StudentController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IEncryptionService _encryptionService;


        public StudentController(AppDbContext context, UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, IEncryptionService encryptionService)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _encryptionService = encryptionService;

        }

        public override void OnActionExecuted(ActionExecutedContext context)
        {
            var userId = _context.Users.FirstOrDefault(u => u.Email == User.Identity.Name).Id;
            List<PrivateMessage> messages = _context.PrivateMessages.Where(x => x.RecieverId == userId && x.IsRead == false).Include(x => x.Sender).OrderByDescending(x => x.Created).ToList();
            List<string> ids = messages.Select(x => x.SenderId).Distinct().ToList();
            List<PrivateMessage> uniqueMessages = new List<PrivateMessage>();
            foreach (var item in ids)
            {
                var uniqueMessage = (messages.FirstOrDefault(x => x.SenderId == item));
                uniqueMessage.Text = _encryptionService.DecryptMessage(Convert.FromBase64String(uniqueMessage.Text));
                uniqueMessages.Add(uniqueMessage);
            }
            ViewBag.Messages = uniqueMessages;
            ViewBag.Notifications = _context.UserNotifications.Include(x => x.Notification)
       .Where(x => x.IsRead == false && x.RecieverId == userId).ToList();
            base.OnActionExecuted(context);
        }


        public IActionResult ClassRegister()
        {
            var user = _context.Users.FirstOrDefault(x => x.Email == User.Identity.Name);
            var class1 = _context.UserClasses.FirstOrDefault(x => x.UserId == user.Id);
            List<Subject> subjects = _context.Subjects.Include(x=>x.Class).Where(x => x.ClassId == class1.ClassId).ToList();
            return View(subjects);
        }

        public async Task<IActionResult> SubjectRegister(int subjectId, int? skip)
        {
            if (skip < 0 || skip == null)
                skip = 0;

            var student = await _context.Users.FirstOrDefaultAsync(x => x.Email == User.Identity.Name);
            Subject? subject = await _context.Subjects.FirstOrDefaultAsync(x => x.Id == subjectId);
           
            ViewBag.Skip = skip;
            Class? c = await _context.Classes.FirstOrDefaultAsync(x => x.Id == subject.ClassId);

            SubjectRegisterModel subjectRegisterModel = new SubjectRegisterModel();
            subjectRegisterModel.Students = new List<UserClass>() { _context.UserClasses.FirstOrDefault(x=>x.UserId == student.Id) };
            subjectRegisterModel.Dates = _context.SubjectDates.Where(x => x.SubjectId == subjectId).OrderByDescending(x => x.Date).Skip((int)skip).Take(5).OrderBy(x => x.Date).ToList();
            subjectRegisterModel.Marks = _context.UserSubjectDateMarks.Where(x=>x.UserId == student.Id).ToList();

            ViewBag.SubjectId = subjectId;
            ViewBag.Skip = skip;
            return View(subjectRegisterModel);
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
                if (User.IsInRole("Student"))
                {
                    userClasses = await _context.UserClasses.Include(u => u.Class).Where(x => x.UserId == user.Id).ToListAsync();
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
                    if (await _context.UserClasses.FirstOrDefaultAsync(x => x.UserId == user.Id && x.ClassId == classId) == null)
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
            return Forbid();
        }
    }
}
