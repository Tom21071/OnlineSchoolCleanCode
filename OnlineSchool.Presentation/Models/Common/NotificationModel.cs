using OnlineSchool.Domain.Entities;

namespace OnlineSchool.Presentation.Models.Common
{
    public class NotificationModel
    {
        public List<Class> Classes { get; set; }
        public List<bool> ClassesChecked { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
    }
}
