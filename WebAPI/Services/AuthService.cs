using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using WebAPI.Models.Auth;

namespace WebAPI.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;

        public AuthService(UserManager<ApplicationUser> userManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _configuration = configuration;
        }

        public async Task RegisterAsync(RegisterDto registerDto)
        {
            // Maakt een Identity gebruiker aan op basis van de registratiegegevens.
            var user = new ApplicationUser
            {
                UserName = registerDto.UserName,
                Email = registerDto.Email,
                FirstName = registerDto.FirstName,
                LastName = registerDto.LastName,
                DateOfBirth = registerDto.DateOfBirth,
                Height = registerDto.Height,
                Weight = registerDto.Weight
            };

            // Slaat de gebruiker op en laat ASP.NET Identity automatisch valideren.
            // Identity controleert automatischs op vereisten zoals: unieke e-mail en wachtwoordvereisten.
            var result = await _userManager.CreateAsync(user, registerDto.Password);

            // Wanneer de regristratie mislukt worden alle fouten verzameld en terug gestuurd.
            if (!result.Succeeded)
            {
                var errors = string.Join(Environment.NewLine, result.Errors.Select(e => e.Description));
                throw new InvalidOperationException(errors);
            }
        }

        public async Task<LoginResponseDto> LoginAsync(LoginDto loginDto)
        {
            // Zoekt de gebruiker op basis van het ingevoerde emailadres.
            var user = await _userManager.FindByEmailAsync(loginDto.Email);

            // Controleert of de gebruiker bestaat.
            if (user == null)
            {
                throw new InvalidOperationException("Invalid email or password!");
            }

            // Controleert of het ingevoerde wachtwoord correct is.
            var passwordValid = await _userManager.CheckPasswordAsync(user, loginDto.Password);

            // Foutmelding wanneer het wachtwoord niet klopt.
            if (!passwordValid)
            {
                throw new InvalidOperationException("Invalid email or password!");
            }

            // Maakt de claims aan die opgeslagen worden in de JWT token.
            // Claims bevatten informatie over de ingelogde gebruiker.
            var claims = new List<Claim>
            {
                // Slaat het unieke gebruikers-ID op in de token.
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                // Slaat het emailadres van de gebruiker op in de token.
                new Claim(ClaimTypes.Email, user.Email!)
            };


            // Maakt een geheime sleutel waarmee de JWT token wordt ondertekend.
            // De backend gebruikt deze sleutel later om te controleren of de token echt door deze API is aangemaakt.
            // Jwt token staat in de appsettings
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));

            // Bepaalt welke encryptiemethode gebruikt wordt om de token te ondertekenen.
            // Hierdoor kan de API controleren of de token niet aangepast is.
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);


            // Maakt de JWT access token aan.
            // Deze token wordt gebruikt bij toekomstige API requests zodat de backend weet welke gebruiker ingelogd
            var token = new JwtSecurityToken(claims: claims, expires: DateTime.UtcNow.AddMinutes(15), signingCredentials: credentials);

            // Zet de JWT token om naar een string zodat deze naar de frontend gestuurd en opgeslagen kan worden in de LocalStorage.
            var accessToken = new JwtSecurityTokenHandler().WriteToken(token);


            // Maakt een willekeurige refresh token aan.
            // Deze wordt gebruikt om later een nieuwe access token aan te vragen zonder opnieuw het wachtwoord in te voeren.
            var refreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));


            // Stuurt de aangemaakte tokens terug naar de frontend.
            return new LoginResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };
        }
    }
}
