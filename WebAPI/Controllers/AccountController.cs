using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Services;

namespace WebAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService _accountService;

        public AccountController(IAccountService accountService)
        {
            _accountService = accountService;
        }


        // Haalt het profiel op van de ingelogde gebruiker.
        [HttpGet("getmyprofile")]
        public async Task<IActionResult> GetMyProfile()
        {
            // Haalt het profiel op van de ingelogde gebruiker.
            var userProfile = await _accountService.GetMyProfileAsync(User);

            // De gebruiker bestaat niet.
            if (userProfile is null)
                return NotFound("User not found.");

            // Geeft het gebruikersprofiel terug.
            return Ok(userProfile);
        }
    }
}