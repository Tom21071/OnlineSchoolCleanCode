using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace OnlineSchool.Domain.Entities { 
    public class PrivateMessage
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Text { get; set; }

        [Required, ForeignKey("AppUser"), Column(TypeName = "nvarchar(450)")]
        public string SenderId { get; set; }
        public virtual AppUser Sender { get; set; }
        public DateTime Created { get; set; }

        [Required, ForeignKey("AppUser"), Column(TypeName = "nvarchar(450)")]
        public string RecieverId { get; set; }
        public virtual AppUser Reciever { get; set; }
    }
}
