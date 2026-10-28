using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Extensions;
using WebAPI.Models.Auth;
using WebAPI.Services;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService _accountService;

        public AccountController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        // Haalt het profiel op van de ingelogde gebruiker.
        [HttpGet("getmyprofile")]
        public async Task<IActionResult> GetMyProfile(CancellationToken cancellationToken)
        {
            // Controleert of de gebruiker is ingelogd en haalt het user ID uit de claims.
            if (!User.TryGetUserId(out var userId))
                return Unauthorized("Invalid user.");

            // Haalt de profielgegevens op van de ingelogde gebruiker.
            var userProfile = await _accountService.GetMyProfileAsync(userId, cancellationToken);

            // Controleert of de gebruiker bestaat.
            if (userProfile is null)
                return NotFound("User not found.");

            // Geeft de profielgegevens terug naar de frontend.
            return Ok(userProfile);
        }

        [HttpPut("updatemyprofile")]
        public async Task<IActionResult> UpdateMyProfile(UserProfileDto userProfileDto, CancellationToken cancellationToken)
        {
            // Controleert of de gebruiker is ingelogd en haalt het user ID uit de claims.
            if (!User.TryGetUserId(out var userId))
                return Unauthorized("Invalid user.");

            // Werkt het profiel bij van de ingelogde gebruiker.
            await _accountService.UpdateMyProfileAsync(userId, userProfileDto, cancellationToken);

            // Geeft een succesvolle response terug wanneer het profiel is bijgewerkt.
            return Ok("Profile updated successfully!");
        }
    }
}