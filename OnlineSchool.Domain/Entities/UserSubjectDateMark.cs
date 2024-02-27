using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineSchool.Domain.Entities
{
    public class UserSubjectDateMark
    {
        [Required, ForeignKey("AppUser"), Column(TypeName = "nvarchar(450)")]
        public string UserId { get; set; }

        [Required, ForeignKey("SubjectDate")]
        public int SubjectDateId { get; set; }
        [Required]
        public int Mark { get; set; }
        [Required]
        public DateTime CreatedAt { get; set; }
    }
}
