namespace Blazor.Models.Acccount
{
    public class UserProfile
    {
        public Guid UserId { get; set; }
        public string UserName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public DateOnly? DateOfBirth { get; set; }

        public double? Height { get; set; }

        public double? Weight { get; set; }
    }
}
