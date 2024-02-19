using OnlineSchool.Domain.Entities;

namespace OnlineSchool.Presentation.Models.Common
{
	public class ClassSubjectsModel
	{
		public List<Subject> Subjects { get; set; }
		public AppUser Teacher { get; set; }
		public List<AppUser> Students { get; set; }
	}
}
