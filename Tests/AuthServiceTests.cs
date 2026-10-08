using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Moq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using WebAPI.Exceptions;
using WebAPI.Models.Auth;
using WebAPI.Services;

namespace Tests
{
    public class AuthServiceTests
    {
        private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
        private readonly AuthService _authService;

        public AuthServiceTests()
        {
            // Maak een neppe UserStore zodat we geen echte database gebruiken.
            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            // Geef de neppe UserStore aan een neppe UserManager.
            _userManagerMock = new Mock<UserManager<ApplicationUser>>(userStoreMock.Object, null, null, null, null, null, null, null, null);
            // Geef de neppe UserManager aan de echte AuthService.
            // Zo kunnen we de echte AuthService testen zonder een echte database.
            _authService = new AuthService(_userManagerMock.Object, null!);
        }

        [Fact]
        public async Task LoginAsync_InvalidPassword_ThrowsInvalidCredentialsException()
        {
            // Maak een neppe gebruiker aan.
            var user = new ApplicationUser
            {
                Id = "1",
                Email = "test@example.com",
                UserName = "test@example.com"
            };

            // Maak de login gegevens aan met een verkeerd wachtwoord.
            var loginDto = new LoginDto
            {
                Email = "test@example.com",
                Password = "wrongpassword"
            };

            // Zeg tegen de neppe UserManager dat email bestaat dus er een gebruiker is.
            _userManagerMock.Setup(x => x.FindByEmailAsync(loginDto.Email))
                .ReturnsAsync(user);

            // Zeg tegen de neppe UserManager dat het wachtwoord fout is.
            _userManagerMock.Setup(x => x.CheckPasswordAsync(user, loginDto.Password))
                .ReturnsAsync(false);

            // Voer de echte login uit met onze neppe gegevens.
            // De neppe usermanager geeft false terug wanneer de service het wachtwoord controleert.
            // De service moet dan de exception terug geven met de bijbehorende melding.
            var exception = await Assert.ThrowsAsync<InvalidCredentialsException>(() => _authService.LoginAsync(loginDto));

            // Controleer of de juiste foutmelding wordt teruggegeven.
            Assert.Equal("Invalid email or password!", exception.Message);
        }

        [Fact]
        public async Task LoginAsync_UnknownEmail_ThrowsInvalidCredentialsException()
        {
            // Maak de login gegevens aan met een e-mailadres dat niet bestaat.
            var loginDto = new LoginDto
            {
                Email = "unknown@example.com",
                Password = "password123"
            };

            // Zeg tegen de neppe UserManager dat het e-mailadres niet bestaat.
            _userManagerMock.Setup(x => x.FindByEmailAsync(loginDto.Email))
                .ReturnsAsync((ApplicationUser?)null);

            // Voer de echte login uit met onze neppe gegevens.
            // De neppe UserManager geeft null terug wanneer de service de gebruiker zoekt.
            // De service moet dan een exception geven met de juiste melding.
            var exception = await Assert.ThrowsAsync<InvalidCredentialsException>(() => _authService.LoginAsync(loginDto));

            // Controleer of de juiste foutmelding wordt teruggegeven.
            Assert.Equal("Invalid email or password!", exception.Message);
        }

        [Fact]
        public async Task LoginAsync_ValidCredentials_ReturnsLoginResponseWithCorrectClaims()
        {
            // Maak een neppe gebruiker aan.
            var user = new ApplicationUser
            {
                Id = "1",
                Email = "test@example.com",
                UserName = "test@example.com"
            };

            // Maak de login gegevens aan met het juiste wachtwoord.
            var loginDto = new LoginDto
            {
                Email = "test@example.com",
                Password = "correctpassword"
            };

            // Zeg tegen de neppe UserManager dat het e-mailadres bestaat.
            _userManagerMock.Setup(x => x.FindByEmailAsync(loginDto.Email))
                .ReturnsAsync(user);

            // Zeg tegen de neppe UserManager dat het wachtwoord goed is.
            _userManagerMock.Setup(x => x.CheckPasswordAsync(user, loginDto.Password))
                .ReturnsAsync(true);

            // Maak een neppe configuration aan met een test JWT-key.
            var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Jwt:Key"] = "this-is-a-test-key-that-is-long-enough"
                })
                .Build();

            // Stuur de neppe gegevens naar de echte service.
            var authService = new AuthService(_userManagerMock.Object, configuration);

            // Voer de echte login uit met onze neppe gegevens.
            var result = await authService.LoginAsync(loginDto);

            // Controleer of er een resultaat is.
            Assert.NotNull(result);
            // Controleer of er een access token is.
            Assert.NotEmpty(result.AccessToken);
            // Controleer of er een refresh token is.
            Assert.NotEmpty(result.RefreshToken);

            // Maak een JWT-handler om de token te kunnen lezen.
            var tokenHandler = new JwtSecurityTokenHandler();
            // Lees de access token uit.
            var token = tokenHandler.ReadJwtToken(result.AccessToken);

            // Zoek de UserId op in de token.
            var userIdClaim = token.Claims.First(x => x.Type == ClaimTypes.NameIdentifier);
            // Zoek het e-mailadres op in de token.
            var emailClaim = token.Claims.First(x => x.Type == ClaimTypes.Email);

            // Controleer of de UserId in de token klopt.
            Assert.Equal(user.Id, userIdClaim.Value);
            // Controleer of het e-mailadres in de token klopt.
            Assert.Equal(user.Email, emailClaim.Value);
        }

        [Fact]
        public async Task RegisterAsync_ValidData_CreatesUser()
        {
            // Maak de registratie gegevens aan.
            var registerDto = new RegisterDto
            {
                UserName = "test@example.com",
                Email = "test@example.com",
                FirstName = "Test",
                LastName = "User",
                Password = "Password123!",
                DateOfBirth = new DateOnly(2000, 1, 1),
                Height = 180,
                Weight = 80
            };

            // Zeg tegen de neppe UserManager dat het aanmaken van de gebruiker lukt.
            _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), registerDto.Password))
                .ReturnsAsync(IdentityResult.Success);

            // Voer de echte registratie uit met onze neppe gegevens.
            await _authService.RegisterAsync(registerDto);
        }

        [Fact]
        public async Task RegisterAsync_CreateUserFails_ThrowsConflictException()
        {
            // Maak de registratie gegevens aan.
            var registerDto = new RegisterDto
            {
                UserName = "test@example.com",
                Email = "test@example.com",
                FirstName = "Test",
                LastName = "User",
                Password = "Password123!",
                DateOfBirth = new DateOnly(2000, 1, 1),
                Height = 180,
                Weight = 80
            };

            // Maak een foutmelding aan die de neppe UserManager teruggeeft.
            var errors = new[]
            {
                new IdentityError
                {
                    Description = "Email is already taken."
                }
            };

            // Zeg tegen de neppe UserManager dat het aanmaken van de gebruiker mislukt.
            _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), registerDto.Password))
                .ReturnsAsync(IdentityResult.Failed(errors));

            // Voer de echte registratie uit met onze neppe gegevens.
            // De neppe UserManager geeft aan dat het aanmaken van de gebruiker mislukt.
            // De service moet dan een ConflictException geven.
            var exception = await Assert.ThrowsAsync<ConflictException>(() => _authService.RegisterAsync(registerDto));

            // Controleer of de juiste foutmelding wordt teruggegeven.
            Assert.Equal("Email is already taken.", exception.Message);
        }

    }
}