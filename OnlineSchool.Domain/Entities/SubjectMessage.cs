using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace OnlineSchool.Domain.Entities
{
    public class SubjectMessage
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Text { get; set; }

        [Required, ForeignKey("AppUser"), Column(TypeName = "nvarchar(450)")]
        public string UserId { get; set; }
        public virtual AppUser User { get; set; }
        public DateTime Created { get; set; }

        [Required, ForeignKey("Subject")]
        public int SubjectId { get; set; }
        public virtual Subject Subject { get; set; }
    }
}
