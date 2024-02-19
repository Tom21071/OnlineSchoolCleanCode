using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using OnlineSchool.Domain.Contexts;
using OnlineSchool.Domain.Entities;
using OnlineSchool.Presentation.Hubs;
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
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            ViewBag.Notifications = _context.UserNotifications.Include(x => x.Notification)
            .Where(x => x.IsRead == false && x.RecieverId == _context.Users.FirstOrDefault(u => u.Email == User.Identity.Name).Id).ToList();
            base.OnActionExecuting(context);
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

        public IActionResult Custom404()
        {
            return View();
        }

        [HttpPost]
        public async Task SawNotification(int Id)
        {
            string userId = (await _context.Users.FirstOrDefaultAsync(x => x.Email == User.Identity.Name)).Id;

            UserNotification un = await _context.UserNotifications.FirstOrDefaultAsync(x => x.NotificationId == Id && x.RecieverId == userId);
            un.IsRead = true;
            _context.Update(un);
            _context.SaveChanges();
        }

        public IActionResult Library()
        {
            return View();
        }

        public IActionResult AlreadyLoggedInTheRoom()
        {
            return View();
        }

        public IActionResult RoomIsFull()
        {
            return View();
        }

        [Route("/room/{streamingRoomId}")]
        public IActionResult Room(string streamingRoomId)
        {
            ViewBag.StreamingRoomId = streamingRoomId;
            List<Room> rooms = StreamingHub.GetGroups();
            ViewBag.StreamingRoomUsersCount = 0;
            int? subjectId = (int?)TempData["SubjectId"];

            if (subjectId == null && rooms.Any(x => x.RoomName == streamingRoomId) == false)
            {
                return RedirectToAction("Custom404");
            }

            Room room = rooms.FirstOrDefault(x => x.RoomName == streamingRoomId);
            if (room != null && room.RoomUsers.Any(x => x.User.Email == User.Identity.Name))
            {
                return RedirectToAction("AlreadyLoggedInTheRoom");
            }

            if (room != null && room.RoomUsers.Count() == 10)
            {
                return RedirectToAction("RoomIsFull");
            }

            ViewBag.SubjectId = subjectId;
            if (subjectId == null)
            {
                ViewBag.SubjectId = 0;
            }
            return View();
        }


        public IActionResult CreateRoom(int subjectId)
        {
            ViewBag.SubjectId = subjectId;
            TempData["SubjectId"] = subjectId;
            string uniqueId = Guid.NewGuid().ToString();
            return Redirect($"/room/{uniqueId}");
        }

        [Route("/subjects/{subjectId}")]
        public async Task<IActionResult> Chat(int subjectId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == User.Identity.Name);
            if (user != null)
            {
                Subject subject = await _context.Subjects.FirstOrDefaultAsync(x => x.Id == subjectId);
                if (subject == null) return Forbid();

                if (User.IsInRole("Student"))
                    if (_context.UserClasses.FirstOrDefault(x => x.UserId == user.Id && x.ClassId == subject.ClassId) == null) return Forbid();

                if (User.IsInRole("Teacher"))
                    if (_context.Subjects.FirstOrDefault(x => x.TeacherId == user.Id && x.Id == subject.Id) == null) return Forbid();

                ViewBag.Rooms = StreamingHub.GetGroups().Where(x => x.SubjectId == subjectId);
            }
            else
                return Forbid();

            var messages = GetMessages(0, 5, subjectId);
            ViewBag.SubjectId = subjectId;
            return View(messages);
        }

        public List<SubjectMessage> GetMessages(int skip, int amount, int subjectId)
        {

            var baseQuery = _context.SubjectMessages.Include(u => u.User).Where(x => x.SubjectId == subjectId).OrderByDescending(x => x.Created);
            return baseQuery.Skip(skip).Take(amount).OrderBy(x => x.Created).ToList();
        }
    }
}
