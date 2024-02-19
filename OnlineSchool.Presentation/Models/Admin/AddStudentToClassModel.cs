
using OnlineSchool.Domain.Entities;

namespace OnlineSchool.Presentation.Models.Admin
{
    public class AddStudentToClassModel
    {
        public AppUser User { get; set; }
        public List<UserClass> Classes { get; set; }
    }
}
