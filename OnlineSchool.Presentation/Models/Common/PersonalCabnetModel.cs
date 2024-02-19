using OnlineSchool.Domain.Entities;

namespace OnlineSchool.Presentation.Models.Common
{
	public class PersonalCabnetModel
	{
		public AppUser User { get; set; }
        public List<UserClass> UserClasses { get; set; }
        public List<Lesson> Lessons { get; set; }
	}
}
