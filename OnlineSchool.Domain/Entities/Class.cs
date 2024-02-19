using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace OnlineSchool.Domain.Entities
{
    public class Class
    {
        public int Id { get; set; }
        public string Name { get; set; }

        [Required, ForeignKey("AppUser"), Column(TypeName = "nvarchar(450)")]
        public string TeacherId { get; set; }
        public virtual AppUser Teacher { get; set; }
    }
}
