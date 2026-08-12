using System.ComponentModel.DataAnnotations;

namespace WebAPI.Models.Auth
{
    public class RegisterDto
    {
        public string UserName { get; set; }
        public string Email { get; set; } = "";
        public string Password { get; set; } = "";
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public DateOnly? DateOfBirth { get; set; }
        public double? Height { get; set; }
        public double? Weight { get; set; }
    }
}