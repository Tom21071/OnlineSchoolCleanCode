using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineSchool.Domain.Entities
{
    public class UserSubjectMark
    {
        [Required, ForeignKey("AppUser")]
        public string UserId { get; set; }
        [Required, ForeignKey("Subject")]
        public int SubjectId { get; set; }
        [Required]
        public int Mark { get; set; }
        [Required]
        public DateTime CreatedAt { get; set; }
    }
}
