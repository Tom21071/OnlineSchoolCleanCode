using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace OnlineSchool.Domain.Entities
{
    public class UserNotification
    {
        [Key]
        [Required, ForeignKey("AppUser"), Column(TypeName = "nvarchar(450)")]
        public string RecieverId { get; set; }

        [Key]
        [Required, ForeignKey("Notification")]
        public int NotificationId { get; set; }
        public virtual Notification Notification { get; set; }
        public bool IsRead { get; set; }
    }
}
