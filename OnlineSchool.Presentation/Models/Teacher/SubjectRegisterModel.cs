using OnlineSchool.Domain.Entities;

namespace OnlineSchool.Presentation.Models.Teacher
{
    public class SubjectRegisterModel
    {
       public List<UserClass> Students { get; set; }
       public List<SubjectDate> Dates { get; set; }
        public List<UserSubjectDateMark> Marks { get; set; }
    }
}
