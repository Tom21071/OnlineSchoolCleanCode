using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineSchool.Domain.Entities
{
    public class UserClass
    {
        [Key]
        [Required, ForeignKey("AppUser"), Column(TypeName = "nvarchar(450)")]
        public string UserId { get; set; }
        public virtual AppUser User { get; set; }

        [Key]
        [ForeignKey("Class")]
        public int ClassId { get; set; }
        public virtual Class Class { get; set; }

    }
}
