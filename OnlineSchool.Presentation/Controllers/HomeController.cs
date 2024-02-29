using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using OnlineSchool.Application.EncryptionServiceInterface;
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
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IEncryptionService _encryptionService;

        public HomeController(AppDbContext context, UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, IEncryptionService encryptionService)
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

        public async Task<IActionResult> Dashboard()
        {
            return View();
        }
        [Route("/private/{recieverId}")]
        public async Task<IActionResult> PrivateChat(string recieverId)
        {
            var otherUser = await _context.Users.FirstOrDefaultAsync(x => x.Id == recieverId);
            var currentUser = await _context.Users.FirstOrDefaultAsync(x => x.Email == User.Identity.Name);
            if (otherUser == null)
                return RedirectToAction("Custom404", "Home");

            await _context.PrivateMessages.Where(x => x.RecieverId == currentUser.Id && x.SenderId == otherUser.Id && x.IsRead == false).ForEachAsync(x => x.IsRead = true);
            await _context.SaveChangesAsync();

            return View(otherUser);
        }

        public async Task<IActionResult> PersonalCabnet()
        {
            PersonalCabnetModel personalCabnetModel = new PersonalCabnetModel();
            personalCabnetModel.User = await _context.Users.Include(x => x.Classes).FirstOrDefaultAsync(x => x.Email == User.Identity.Name);
            List<UserClass> userClasses = await _context.UserClasses.Include(x => x.Class).Include(x => x.User).Where(x => x.User.Email == User.Identity.Name).ToListAsync();
            personalCabnetModel.UserClasses = userClasses;
            int tommorow = (int)DateTime.Today.DayOfWeek + 1;
            personalCabnetModel.Lessons = await _context.Lessons.Include(s => s.Subject).Where(x => x.DayOfTheWeek == (byte)tommorow).ToListAsync();
            return View(personalCabnetModel);
        }

        public async Task<IActionResult> Custom404()
        {
            return View();
        }

        public async Task<IActionResult> YouDontBelong()
        {
            return View();
        }

        [HttpPost]
        public async Task SawNotification(int Id)
        {
            string userId = (await _context.Users.FirstOrDefaultAsync(x => x.Email == User.Identity.Name)).Id;

            UserNotification un = await _context.UserNotifications.FirstOrDefaultAsync(x => x.NotificationId == Id && x.RecieverId == userId);
            if (un != null)
            {
                un.IsRead = true;
                _context.Update(un);
                _context.SaveChanges();
            }
        }

        public async Task<IActionResult> Library()
        {
            return View();
        }

        public async Task<IActionResult> AlreadyLoggedInTheRoom()
        {
            return View();
        }

        public async Task<IActionResult> RoomIsFull()
        {
            return View();
        }

        [Route("/room/{streamingRoomId}")]
        public async Task<IActionResult> Room(string streamingRoomId)
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


        public async Task<IActionResult> CreateRoom(int subjectId)
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
                if (subject == null) return RedirectToAction("YouDontBelong", "Home");

                if (User.IsInRole("Student"))
                    if (_context.UserClasses.FirstOrDefault(x => x.UserId == user.Id && x.ClassId == subject.ClassId) == null) return RedirectToAction("YouDontBelong", "Home");

                if (User.IsInRole("Teacher"))
                    if (_context.Subjects.FirstOrDefault(x => x.TeacherId == user.Id && x.Id == subject.Id) == null) return RedirectToAction("YouDontBelong", "Home");
            }
            else
                return RedirectToAction("YouDontBelong", "Home");
            ViewBag.SubjectId = subjectId;
            var messages = GetMessages(0, 5, subjectId);
            return View(messages);
        }

        public List<SubjectMessage> GetMessages(int skip, int amount, int subjectId)
        {
            var baseQuery = _context.SubjectMessages.Include(u => u.User).Where(x => x.SubjectId == subjectId).OrderByDescending(x => x.Created);
            var takenMessages = baseQuery.Skip(skip).Take(amount).OrderBy(x => x.Created).ToList();
            foreach (var m in takenMessages)
            {
                m.Text = _encryptionService.DecryptMessage(Convert.FromBase64String(m.Text));
            }
            return takenMessages;
        }
    }
}
