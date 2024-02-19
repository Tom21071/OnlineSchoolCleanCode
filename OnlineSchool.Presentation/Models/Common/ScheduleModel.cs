
using OnlineSchool.Domain.Entities;

namespace OnlineSchool.Presentation.Models.Common
{
    public class ScheduleModel
    {
        public Class Class { get; set; }
        public List<Lesson> Monday { get; set; }
        public List<Lesson> Tuesday { get; set; }
        public List<Lesson> Wednsday { get; set; }
        public List<Lesson> Thursday { get; set; }
        public List<Lesson> Friday { get; set; }
    }
}
