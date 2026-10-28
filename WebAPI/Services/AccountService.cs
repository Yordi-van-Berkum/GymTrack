using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebAPI.Exceptions;
using WebAPI.Models.Auth;

namespace WebAPI.Services
{
    public class AccountService : IAccountService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AccountService(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Haalt de profielgegevens op van de ingelogde gebruiker.
        public async Task<UserProfile?> GetMyProfileAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId.ToString(), cancellationToken);

            if (user == null)
                return null;

            return new UserProfile
            {
                UserId = Guid.Parse(user.Id),
                UserName = user.UserName ?? "",
                Email = user.Email ?? "",
                FirstName = user.FirstName,
                LastName = user.LastName,
                DateOfBirth = user.DateOfBirth,
                Height = user.Height,
                Weight = user.Weight
            };
        }

        public async Task UpdateMyProfileAsync(Guid userId, UserProfileDto userProfileDto, CancellationToken cancellationToken = default)
        {
            // Haalt de ingelogde gebruiker op uit de database.
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == userId.ToString(), cancellationToken);

            if (user is null)
                throw new NotFoundException("User not found.");

            // Past de gegevens aan.
            user.UserName = userProfileDto.UserName.Trim();
            user.FirstName = userProfileDto.FirstName.Trim();
            user.LastName = userProfileDto.LastName.Trim();
            user.DateOfBirth = userProfileDto.DateOfBirth;
            user.Email = userProfileDto.Email.Trim();
            user.Height = userProfileDto.Height;
            user.Weight = userProfileDto.Weight;

            // Controleert of het request inmiddels is geannuleerd.
            cancellationToken.ThrowIfCancellationRequested();

            // Slaat de wijzigingen op via Identity.
            // Dit zodat NormalizedUserName en NormalizedEmail ook aangepast worden.
            var result = await _userManager.UpdateAsync(user);

            // Result is niet succeeded.
            // Haalt alle foutmeldingen uit het IdentityResult.
            // Select pakt alleen de Description van elke fout.
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                // Gooit een exception met de foutmeldingenm vanuit IdentityResult.
                throw new ConflictException(errors);
            }
        }
    }
}