using Microsoft.AspNetCore.Identity;

namespace WebAPI.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public DateOnly? DateOfBirth { get; set; }

        public double? Height { get; set; }
        public double? Weight { get; set; }
    }
}
