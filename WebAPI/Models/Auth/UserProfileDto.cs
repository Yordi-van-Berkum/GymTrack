using System.ComponentModel.DataAnnotations;

namespace WebAPI.Models.Auth
{
    public class UserProfileDto
    {
        [Required(ErrorMessage = "User Name is required!")]
        public string UserName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required!")]
        [EmailAddress(ErrorMessage = "Invalid email address!")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "First Name is required!")]
        [MaxLength(50, ErrorMessage = "First Name cannot exceed 50 characters!")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last Name is required!")]
        [MaxLength(50, ErrorMessage = "Last Name cannot exceed 50 characters!")]
        public string LastName { get; set; } = string.Empty;

        public DateOnly? DateOfBirth { get; set; }

        [Range(50, 250, ErrorMessage = "Height must be between 50 and 250 cm!")]
        public double? Height { get; set; }

        [Range(20, 500, ErrorMessage = "Weight must be between 20 and 500 kg!")]
        public double? Weight { get; set; }
    }
}