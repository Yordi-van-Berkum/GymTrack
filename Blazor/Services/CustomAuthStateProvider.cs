using Blazor.Models.Auth;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;

namespace Blazor.Services
{
    public class CustomAuthStateProvider : AuthenticationStateProvider
    {
        private readonly HttpClient httpClient;
        private readonly ISyncLocalStorageService localStorage;
        private readonly SafeApiHelper _safeApiHelper;

        public CustomAuthStateProvider(HttpClient httpClient, ISyncLocalStorageService localStorage, SafeApiHelper safeApiHelper)
        {
            this.httpClient = httpClient;
            this.localStorage = localStorage;
            _safeApiHelper = safeApiHelper;

            // Haalt de opgeslagen JWT access token op uit de browser LocalStorage.
            // Deze token blijft bestaan wanneer de gebruiker de pagina vernieuwt
            var accesToken = localStorage.GetItem<string>("accessToken");

            // Controleert of er een token aanwezig is.
            if (accesToken != null)
            {
                // Zet automatisch de Authorization header op alle toekomstige API requests.
                // Hierdoor weet de backend bij beveiligde endpoints wie de gebruiker is.
                this.httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accesToken);
            }
        }

        public async Task<string> RegisterAsync(RegisterDto registerDto)
        {
            // Stuurt de regristratiegegevens op naar de backend via de AuthController.
            // SafeActionApiCallAsync voert de HTTP-aanroep veilig uit. Deze functie staat in de SafeApiHelper.cs in de Services map.
            return await _safeApiHelper.SafeActionApiCallAsync(() => httpClient.PostAsJsonAsync("api/auth/register", registerDto));
        }

        public async Task LoginAsync(LoginDto loginDto)
        {
            //Stuurt de logingegevens op naar de backend via de AuthController.
            //SafeDataApiCallAsync voert de HTTP-aanroep veilig uit. Deze functie staat in de SafeApiHelper.cs in de Services map.
            var result = await _safeApiHelper.SafeDataApiCallAsync<LoginResponseDto>(() => httpClient.PostAsJsonAsync("api/auth/login", loginDto));

            // Slaat tokens lokaal op zodat de gebruiker ingelogd blijft.
            localStorage.SetItem("accessToken", result.AccessToken);
            localStorage.SetItem("refreshToken", result.RefreshToken);

            // Zorgt ervoor dat volgende API requests automatisch de JWT token gebruiken.
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", result.AccessToken);

            // Laat Blazor weten dat de gebruiker nu ingelogd is.
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }

        public void Logout()
        {
            // Verwijdert de access token uit de lokale opslag.
            // Deze token wordt gebruikt om de gebruiker bij de API te authenticeren.
            localStorage.RemoveItem("accessToken");


            // Verwijdert de refresh token uit de lokale opslag.
            // Hierdoor kan de gebruiker geen nieuwe access token meer aanvragen.
            localStorage.RemoveItem("refreshToken");

            // Verwijdert de JWT token uit de standaard headers van HttpClient.
            // Nieuwe API requests worden hierdoor niet meer als ingelogde gebruiker verstuurd.
            httpClient.DefaultRequestHeaders.Authorization = null;

            // Laat Blazor weten dat de gebruiker uitgelogd is.
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            // Standaard gebruiker zonder identiteit.
            // Wanneer er geen geldige token is blijft de gebruiker anoniem.
            var anonymousUser = new ClaimsPrincipal(new ClaimsIdentity());


            var accessToken = localStorage.GetItem<string>("accessToken");

            if (string.IsNullOrWhiteSpace(accessToken))
            {
                return new AuthenticationState(anonymousUser);
            }

            try
            {
                // Haalt de ingelogde gebruiker op via de backend.
                // De JWT token wordt automatisch meegestuurd via de Authorization header.
                var response = await httpClient.GetAsync("api/auth/getuser");

                // Wanneer de gebruiker niet ingelogd is of de token ongeldig is, blijft de gebruiker anoniem.
                if (!response.IsSuccessStatusCode)
                {
                    return new AuthenticationState(anonymousUser);
                }

                // Leest de gebruiker response vanuit de backend.
                var userResponse = await response.Content.ReadFromJsonAsync<UserResponseDto>();

                // Controleert of de backend daadwerkelijk een gebruiker heeft teruggestuurd.
                if (userResponse == null)
                {
                    return new AuthenticationState(anonymousUser);
                }

                // Maakt de claims aan die Blazor gebruikt om te weten wie ingelogd is.
                var claims = new List<Claim>
                {
                    // Slaat het id op van de gebruiker op.
                    new Claim(ClaimTypes.NameIdentifier, userResponse.UserId),

                    // Slaat het emailadres op als naam en email claim.
                    new Claim(ClaimTypes.Name, userResponse.Email),
                    new Claim(ClaimTypes.Email, userResponse.Email)
                };

                // Maakt een identity op basis van de ontvangen claims.
                var identity = new ClaimsIdentity(claims, "Token");

                // Maakt een ingelogde gebruiker aan.
                var user = new ClaimsPrincipal(identity);

                // Geeft de huidige authentication state terug.
                return new AuthenticationState(user);
            }
            catch (HttpRequestException)
            {
                // Wanneer de API niet bereikbaar is blijft de gebruiker anoniem.
                return new AuthenticationState(anonymousUser);
            }
        }
    }
}
