
using System.ComponentModel.DataAnnotations;

namespace OnlineSchool.Presentation.Models.Admin
{
    public class RegisterViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }
        public string ImageUrl { get; set; }
		[Required]
        public string FirstName { get; set; }
		[Required]
		public string LastName { get; set; }
		[Required]
		public string Patronym { get; set; }

		[Required(ErrorMessage = "The IDNP field is required.")]
		[StringLength(13, ErrorMessage = "The IDNP must be exactly 13 digits.")]
		[MinLength(13)]
		[MaxLength(13)]
        public string IDNP { get; set; }
		[Required]
		public DateTime DateOfBirth { get; set; }

        [Required(ErrorMessage = "Please select a role")]
        public string Role { get; set; }
	}
}
