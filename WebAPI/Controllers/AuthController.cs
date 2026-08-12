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
            // Geeft de basisinformatie van de ingelogde gebruiker terug naar Blazor.
            return Ok(new
            {
                // Haalt het unieke gebruikers-ID uit de JWT claim.
                userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                // Haalt het emailadres uit de JWT claim.
                email = User.FindFirst(ClaimTypes.Email)?.Value,
            });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto registerDto)
        {
            try
            {
                // Roept de authenticatie service aan om een nieuwe gebruiker te registreren.
                await _authService.RegisterAsync(registerDto);
                // Geeft een succesvolle HTTP 200 response terug wanneer de registratie gelukt is.
                return Ok("Account successfully created.");
            }
            catch (InvalidOperationException ex)
            {
                // Wordt gebruikt voor InvalidOperationException fouten vanuit de service.
                // De foutmelding wordt doorgestuurd naar de frontend zodat deze in een toastmelding weergegeven kan worden.
                return BadRequest(ex.Message);
            }
            catch (Exception)
            {
                // Wordt gebruikt bij onverwachte fouten.
                // Geeft een standaard foutmelding terug zodat er geen technische informatie terug gestuurd wordt wat niet voor de gebruiker is.
                return StatusCode(500, "Something went wrong.");
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto loginDto)
        {
            try
            {
                // Roept de authenticatie service aan om de gebruiker te controleren
                // en JWT tokens aan te maken wanneer de gegevens correct zijn.
                var tokens = await _authService.LoginAsync(loginDto);

                // Geeft de tokens terug aan de frontend zodat deze opgeslagen kunnen worden en gebruikt kunnen worden voor geautoriseerde API requests.
                return Ok(tokens);
            }
            catch (InvalidOperationException ex)
            {
                // Wordt gebruikt voor InvalidOperationException fouten vanuit de service, bijvoorbeeld verkeerde email of wachtwoord.
                // De foutmelding wordt doorgestuurd naar de frontend zodat deze in een toastmelding weergegeven kan worden.
                return BadRequest(ex.Message);
            }
            catch (Exception)
            {
                // Wordt gebruikt bij onverwachte fouten.
                // Geeft een standaard foutmelding terug zodat er geen technische informatie terug gestuurd wordt wat niet voor de gebruiker is.
                return StatusCode(500, "Something went wrong.");
            }
        }
    }
}
