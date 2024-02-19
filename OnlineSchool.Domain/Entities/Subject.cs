using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineSchool.Domain.Entities
{
    public class Subject
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [ForeignKey("Class")]
        public int ClassId { get; set; }

        [Required, ForeignKey("AppUser"), Column(TypeName = "nvarchar(450)")]
        public string TeacherId { get; set; }
        public virtual AppUser Teacher { get; set; }
        public virtual Class Class { get; set; }
        public string Title { get; set; }
        public string ImagePath { get; set; } = "";
    }
}
