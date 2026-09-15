using Blazor.Models.Acccount;

namespace Blazor.Services
{
    public class AccountService
    {
        private readonly HttpClient _httpClient;
        private readonly SafeApiHelper _safeApiHelper;

        public AccountService(HttpClient httpClient, SafeApiHelper safeApiHelper)
        {
            _httpClient = httpClient;
            _safeApiHelper = safeApiHelper;
        }


        // Haalt het profiel op van de ingelogde gebruiker.
        public async Task<UserProfile> GetMyProfileAsync(CancellationToken cancellationToken = default)
        {
            // SafeDataApiCallAsync voert de HTTP-aanroep veilig uit. Deze functie staat in de SafeApiHelper.cs in de Services map.
            return await _safeApiHelper.SafeDataApiCallAsync<UserProfile>(() => _httpClient.GetAsync("api/account/getmyprofile", cancellationToken));
        }
    }
}