using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebAPI.Models.Auth;
using WebAPI.Services;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [Authorize]
        [HttpGet("getuser")]
        public IActionResult GetUser()
        {
            // Geeft de basisinformatie van de ingelogde gebruiker terug.
            return Ok(new
            {
                // Haalt het unieke gebruikers-ID uit de JWT claim.
                userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value,

                // Haalt het e-mailadres uit de JWT claim.
                email = User.FindFirst(ClaimTypes.Email)?.Value
            });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto registerDto)
        {
            // Registreert een nieuwe gebruiker via de authentication service.
            await _authService.RegisterAsync(registerDto);

            // Geeft een succesvolle response wanneer het account is aangemaakt.
            return Ok("Account successfully created.");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto loginDto)
        {
            // Controleert de login en maakt JWT tokens aan.
            var tokens = await _authService.LoginAsync(loginDto);

            // Geeft de tokens terug aan de frontend.
            return Ok(tokens);
        }
    }
}