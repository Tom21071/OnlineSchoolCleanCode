using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace OnlineSchool.Domain.Entities
{
    public class SubjectDate
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required, ForeignKey("Subject")]
        public int SubjectId { get; set; }
        [Required]
        public DateTime Date { get; set; }
    }
}
