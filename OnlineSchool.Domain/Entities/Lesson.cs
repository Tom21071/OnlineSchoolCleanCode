using System.ComponentModel.DataAnnotations;

namespace OnlineSchool.Domain.Entities
{
    public class Lesson
	{
		public DateTime Start { set; get; }
		public DateTime End { set; get; }
		public int ClassId { get; set; }
        public virtual Class Class { get; set; }
        public int SubjectId { get; set; }
		virtual public Subject Subject { get; set; }
		[Range(1,7)]
		public byte DayOfTheWeek { get; set; }
		public string Cabnet { get; set; }
	}
}
