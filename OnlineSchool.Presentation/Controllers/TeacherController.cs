using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using OnlineSchool.Application.EncryptionServiceInterface;
using OnlineSchool.Domain.Contexts;
using OnlineSchool.Domain.Entities;
using OnlineSchool.Presentation.Models.Common;
using OnlineSchool.Presentation.Models.Teacher;

namespace OnlineSchool.Presentation.Controllers
{
    [Authorize(Roles = "Teacher")]
    public class TeacherController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IEncryptionService _encryptionService;
        public TeacherController(AppDbContext context, UserManager<AppUser> userManager, SignInManager<AppUser> signInManager,IEncryptionService encryptionService)
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

            SubjectRegisterModel subjectRegisterModel = new SubjectRegisterModel();
            subjectRegisterModel.Students = _context.UserClasses.Where(x => x.ClassId == c.Id).Include(x => x.User).OrderBy(x => x.User.LastName).ToList();
            subjectRegisterModel.Dates = _context.SubjectDates.ToList();
            subjectRegisterModel.Marks = _context.UserSubjectDateMarks.ToList();

            ViewBag.SubjectId = subjectId;
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

        public async Task<IActionResult> AttendanceAndGrades(int subjectId)
        {
            AttendanceAndGradesModel attendanceAndGradesModel = new AttendanceAndGradesModel();
            attendanceAndGradesModel.Subject = await _context.Subjects.Include(x=>x.Class).FirstOrDefaultAsync(x => x.Id == subjectId);
            attendanceAndGradesModel.Students = _context.UserClasses.Where(x => x.ClassId == attendanceAndGradesModel.Subject.ClassId).Include(x => x.User).OrderBy(x=>x.User.LastName).ToList();
            attendanceAndGradesModel.Date = DateTime.Now;
            return View(attendanceAndGradesModel);
        }

        //[HttpPost]
        //public async Task<IActionResult> AttendanceAndGrades()
        //{
        //}

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
