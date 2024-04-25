using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OnlineSchool.Application.EncryptionServiceInterface;
using OnlineSchool.Domain.Contexts;
using OnlineSchool.Domain.Entities;
using OnlineSchool.Presentation.Models.Admin;
using OnlineSchool.Presentation.Models.Common;
using System.Security.Claims;

namespace OnlineSchool.Presentation.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IEncryptionService _encryptionService;

        public AdminController(AppDbContext context, UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, IEncryptionService encryptionService)
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

        public async Task<IActionResult> Logins()
        {
            LoginsModel loginsModel = new();
            loginsModel.Logins = await _context.Logins.ToListAsync(); 
            return View(loginsModel);
        }


        public async Task<IActionResult> ManageUsers()
        {
            return View(await _context.Users.Include(x => x.Roles).ToListAsync());
        }

        public async Task<IActionResult> BlockUser(string Id)
        {
            var user = _context.Users.FirstOrDefault(x => x.Id == Id);
            if (user != null)
            {
                user.LockoutEnabled = true;
                user.LockoutEnd = DateTime.MaxValue;
                _context.Users.Update(user);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("ManageUsers", "Admin");
        }

        public async Task<IActionResult> UnBlockUser(string Id)
        {
            var user = _context.Users.FirstOrDefault(x => x.Id == Id);
            if (user != null)
            {
                user.LockoutEnd = new DateTime(new DateOnly(2000,01,01),new TimeOnly(11,30));
                _context.Users.Update(user);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("ManageUsers", "Admin");
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> RegisterUser()
        {
            var rolesInDb = await _context.Roles.ToListAsync();
            List<SelectListItem> roles = new List<SelectListItem>();
            foreach (var item in rolesInDb)
            {
                roles.Add(new SelectListItem
                {
                    Value = item.Id,
                    Text = item.Name,
                });
            }
            ViewBag.Roles = roles;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]

        public async Task<IActionResult> RegisterUser(RegisterViewModel model)
        {
            if (_context.Users.Any(u => u.IDNP == model.IDNP))
                ModelState.AddModelError("IDNP", "The IDNP is already in use.");
            if (ModelState.IsValid)
            {
                var user = new AppUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    ImagePath = model.ImageUrl,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Patronym = model.Patronym,
                    IDNP = model.IDNP,
                    DateOfBirth = model.DateOfBirth,
                    TwoFactorEnabled = true,
                    EmailConfirmed = true
                };
                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    await _userManager.AddClaimAsync(user, new Claim("Image", user.ImagePath));
                    _context.UserRoles.Add(new IdentityUserRole<string> { RoleId = model.Role, UserId = _context.Users.FirstOrDefault(x => x.Email == model.Email).Id });
                    _context.SaveChanges();
                    return RedirectToAction("AddedUser", "Admin");
                }
                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);
            }
            var rolesInDb = _context.Roles.ToList();
            List<SelectListItem> roles = new List<SelectListItem>();
            foreach (var item in rolesInDb)
            {
                roles.Add(new SelectListItem
                {
                    Value = item.Id,
                    Text = item.Name,
                });
            }
            ViewBag.Roles = roles;
            return View(model);
        }

        public async Task<IActionResult> AddSubject(int id)
        {
            List<AppUser> appUsers = await _context.Users.Include(x => x.Roles).Where(user => user.Roles.Any(role => role.RoleId == "2")).ToListAsync();
            List<SelectListItem> teachers = new List<SelectListItem>();
            foreach (var item in appUsers)
            {
                teachers.Add(new SelectListItem
                {
                    Value = item.Id,
                    Text = item.FirstName + " " + item.LastName,
                });
            }

            ViewBag.ClassId = id;
            ViewBag.Teachers = teachers;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddSubject(AddSubjectModel subject)
        {
            Subject subject1 = new Subject();
            subject1.Title = subject.SubjectName;
            subject1.TeacherId = subject.TeacherId;
            subject1.ClassId = subject.ClassId;
            subject1.ImagePath = "https://static.thenounproject.com/png/3282617-200.png";
            await _context.Subjects.AddAsync(subject1);
            await _context.SaveChangesAsync();
            return RedirectToAction("AddSubject", "Admin");
        }

        public async Task<IActionResult> EditSubjects(int id)
        {
            SubjectModel subject = new();
            subject.Subjects = await _context.Subjects.Include(x => x.Class).Where(x => x.ClassId == id).ToListAsync();
            subject.ClassId = id;
            return View(subject);
        }

        public async Task<IActionResult> AddStudentsToClass(int id)
        {
            List<AddStudentToClassModel> students = new();
            var studentsIds = await _context.UserRoles.Where(x => x.RoleId == _context.Roles.FirstOrDefault(x => x.Name == "Student").Id).ToListAsync();
            foreach (var item in studentsIds)
            {
                AddStudentToClassModel student = new AddStudentToClassModel();
                student.User = await _context.Users.FirstOrDefaultAsync(x => x.Id == item.UserId);
                student.Classes = await _context.UserClasses.Include(x => x.Class).Where(x => x.UserId == item.UserId).ToListAsync();
                students.Add(student);
            }
            ViewBag.ClassId = id;
            return View(students);
        }

        public async Task<IActionResult> AddStudentsToClassPerform(int classId, string studentId)
        {
            if (await _context.UserClasses.FirstOrDefaultAsync(x => x.UserId == studentId && x.ClassId == classId) == null)
            {
                UserClass userClass = new UserClass();
                userClass.UserId = studentId;
                userClass.ClassId = classId;
                await _context.UserClasses.AddAsync(userClass);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("AddStudentsToClass", "Admin", new { id = classId });
        }

        public async Task<IActionResult> RemoveStudentsFromClassPerform(int classId, string studentId)
        {
            if (await _context.UserClasses.FirstOrDefaultAsync(x => x.UserId == studentId && x.ClassId == classId) != null)
            {
                _context.UserClasses.Remove(_context.UserClasses.FirstOrDefault(x => x.UserId == studentId && x.ClassId == classId));
                _context.SaveChanges();
            }
            return RedirectToAction("AddStudentsToClass", "Admin", new { id = classId });
        }

        public IActionResult RemoveStudentsFromClass()
        {
            return RedirectToAction("Classes", "Home");
        }
        public async Task<IActionResult> ManageClasses()
        {
            List<ClassesModel> classesModels = new List<ClassesModel>();
            var classes = await _context.Classes.ToListAsync();
            foreach (var item in classes)
            {
                ClassesModel cm = new ClassesModel();
                cm.Name = item.Name;
                cm.Teacher = await _context.Users.FirstOrDefaultAsync(x => x.Id == item.TeacherId);
                cm.Id = item.Id;
                classesModels.Add(cm);
            }
            return View(classesModels);
        }

        public IActionResult AddedUser()
        {
            return View();
        }

        public async Task<IActionResult> AddClass()
        {
            List<AppUser> appUsers = await _context.Users.Include(x => x.Roles).Where(user => user.Roles.Any(role => role.RoleId == "2")).ToListAsync();
            List<SelectListItem> teachers = new List<SelectListItem>();
            foreach (var item in appUsers)
            {
                teachers.Add(new SelectListItem
                {
                    Value = item.Id,
                    Text = item.FirstName + " " + item.LastName,
                });
            }

            ViewBag.Teachers = teachers;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddClass(AddClassModel model)
        {
            Class classToDatabase = new Class();
            classToDatabase.TeacherId = model.TeacherId;
            classToDatabase.Name = model.ClassName;
            await _context.Classes.AddAsync(classToDatabase);
            await _context.SaveChangesAsync();
            return View();
        }
    }
}
