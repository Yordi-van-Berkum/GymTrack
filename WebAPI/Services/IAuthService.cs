using WebAPI.Models;

namespace WebAPI.Services
{
    public interface IAuthService
    {
        // Registreert een nieuwe gebruiker in het systeem.
        Task RegisterAsync(RegisterDto registerDto);

        // Controleert de login gegevens en geeft tokens terug wanneer de gebruiker geldig is.
        Task<LoginResponseDto> LoginAsync(LoginDto loginDto);
    }
}
