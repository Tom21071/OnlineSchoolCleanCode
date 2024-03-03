using OnlineSchool.Domain.Entities;

namespace OnlineSchool.Presentation.Models.Teacher
{
    public class AttendanceAndGradesModel
    {
        public Subject Subject {  get; set; }
        public int SubjectId { get; set; }
        public int ClassId { get; set; }
        public List<UserClass> Students { get; set; }
        public List<bool> IsPresent { get; set; }
        public List<int?> Marks { get; set; }
        public DateTime Date { get; set; }
    }
}
