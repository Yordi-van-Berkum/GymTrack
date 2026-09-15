using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using WebAPI.Models.Auth;

namespace WebAPI.Services
{
    public class AccountService : IAccountService
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public AccountService(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }


        // Haalt het profiel van de ingelogde gebruiker op.
        public async Task<UserProfile?> GetMyProfileAsync(ClaimsPrincipal user)
        {
            // Haalt de ingelogde gebruiker op.
            var currentUser = await _userManager.GetUserAsync(user);

            // De gebruiker bestaat niet.
            if (currentUser is null)
                return null;

            // Maakt het gebruikersprofiel aan.
            return new UserProfile
            {
                Id = currentUser.Id,
                Name = currentUser.FirstName,
                Email = currentUser.Email ?? "",
                PhoneNumber = currentUser.PhoneNumber ?? "",
            };
        }
    }
}