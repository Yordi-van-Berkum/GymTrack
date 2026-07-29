namespace WebAPI.Models
{
    public class LoginResponseDto
    {
        // JWT token waarmee de gebruiker toegang krijgt tot beveiligde API endpoints.
        public string AccessToken { get; set; } = string.Empty;

        // Token waarmee later een nieuwe access token kan worden aangevraagd.
        public string RefreshToken { get; set; } = string.Empty;
    }
}