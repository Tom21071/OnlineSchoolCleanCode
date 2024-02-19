using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace OnlineSchool.Domain.Entities
{
    public class AppUser : IdentityUser
    {
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        [Required]
        public string Patronym { get; set; }
        [Required]
        public string IDNP { get; set; }
        [Required]
        public DateTime DateOfBirth { get; set; }
        public string ImagePath { get; set; }
        public virtual List<Class> Classes { get; set; }
        public virtual List<IdentityUserRole<string>> Roles { get; set; }
    }
}
