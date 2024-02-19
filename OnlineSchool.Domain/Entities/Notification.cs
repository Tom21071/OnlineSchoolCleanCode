using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace OnlineSchool.Domain.Entities
{
    public class Notification
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt {  get; set; }

        [Required, ForeignKey("AppUser"), Column(TypeName = "nvarchar(450)")]
        public string SenderId { get; set;}
    }
}
