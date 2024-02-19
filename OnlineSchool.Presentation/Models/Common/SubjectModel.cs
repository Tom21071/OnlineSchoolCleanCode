
using OnlineSchool.Domain.Entities;

namespace OnlineSchool.Presentation.Models.Common
{
    public class SubjectModel
    {
        public int ClassId { get; set; }
        public List<Subject> Subjects { get; set; } = new List<Subject>();
    }
}
