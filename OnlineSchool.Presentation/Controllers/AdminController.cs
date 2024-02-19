using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
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
        public UserManager<AppUser> _userManager;
        public SignInManager<AppUser> _signInManager;

        public AdminController(AppDbContext context, UserManager<AppUser> userManager, SignInManager<AppUser> signInManager)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [HttpGet]
        public IActionResult RegisterUser()
        {
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
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
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
                    DateOfBirth = model.DateOfBirth
                };
                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    _context.UserClaims.Add(new IdentityUserClaim<string> { UserId = user.Id, ClaimType = "Image", ClaimValue = user.ImagePath });
                    await _userManager.AddClaimAsync(user, new Claim("Image", user.ImagePath));
                    //await _signInManager.SignInAsync(user, isPersistent: false);
                    //return RedirectToAction("Classes", "Home"); // Redirect to the desired page after registration
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



        public IActionResult AddSubject(int id)
        {
            List<AppUser> appUsers = _context.Users.Include(x => x.Roles).Where(user => user.Roles.Any(role => role.RoleId == "2")).ToList();
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
        public IActionResult AddSubject(AddSubjectModel subject)
        {
            Subject subject1 = new Subject();
            subject1.Title = subject.SubjectName;
            subject1.TeacherId = subject.TeacherId;
            subject1.ClassId = subject.ClassId;

            _context.Subjects.Add(subject1);
            _context.SaveChanges();
            return RedirectToAction("AddSubject", "Admin");
        }

        public IActionResult EditSubjects(int id)
        {
            SubjectModel subject = new();
            subject.Subjects = _context.Subjects.Include(x => x.Class).Where(x => x.ClassId == id).ToList();
            subject.ClassId = id;
            return View(subject);
        }

        public IActionResult AddStudentsToClass(int id)
        {
            List<AddStudentToClassModel> students = new();
            var studentsIds = _context.UserRoles.Where(x => x.RoleId == _context.Roles.FirstOrDefault(x => x.Name == "Student").Id).ToList();
            foreach (var item in studentsIds)
            {
                AddStudentToClassModel student = new AddStudentToClassModel();
                student.User = _context.Users.FirstOrDefault(x => x.Id == item.UserId);
                student.Classes = _context.UserClasses.Include(x => x.Class).Where(x => x.UserId == item.UserId).ToList();
                students.Add(student);
            }
            ViewBag.ClassId = id;
            return View(students);
        }

        public IActionResult AddStudentsToClassPerform(int classId, string studentId)
        {
            if (_context.UserClasses.FirstOrDefault(x => x.UserId == studentId && x.ClassId == classId) == null)
            {
                UserClass userClass = new UserClass();
                userClass.UserId = studentId;
                userClass.ClassId = classId;
                _context.UserClasses.Add(userClass);
                _context.SaveChanges();
            }

            return RedirectToAction("AddStudentsToClass", "Admin", new { id = classId });
        }

        public IActionResult RemoveStudentsFromClassPerform(int classId, string studentId)
        {
            if (_context.UserClasses.FirstOrDefault(x => x.UserId == studentId && x.ClassId == classId) != null)
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

        public IActionResult ManageClasses()
        {
            List<ClassesModel> classesModels = new List<ClassesModel>();
            var classes = _context.Classes.ToList();
            foreach (var item in classes)
            {
                ClassesModel cm = new ClassesModel();
                cm.Name = item.Name;
                cm.Teacher = _context.Users.FirstOrDefault(x => x.Id == item.TeacherId);
                cm.Id = item.Id;
                classesModels.Add(cm);
            }
            return View(classesModels);
        }

        public IActionResult AddedUser()
        {
            return View();
        }

    }
}
