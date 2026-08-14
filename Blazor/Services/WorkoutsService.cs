using Blazor.Models.Workouts;
using System.Net.Http.Json;

namespace Blazor.Services
{
    public class WorkoutsService
    {
        private readonly HttpClient _httpClient;
        private readonly SafeApiHelper _safeApiHelper;

        public WorkoutsService(HttpClient httpClient, SafeApiHelper safeApiHelper)
        {
            _httpClient = httpClient;
            _safeApiHelper = safeApiHelper;
        }

        // Maakt een nieuwe workout aan via de backend.
        public async Task<string> CreateWorkoutAsync(WorkoutDto workoutDto, CancellationToken cancellationToken = default)
        {
            // Stuurt de workout informatie op naar de backend.
            // SafeActionApiCallAsync voert de HTTP-aanroep veilig uit. Deze functie staat in de SafeApiHelper.cs in de Services map.
            return await _safeApiHelper.SafeActionApiCallAsync(() => _httpClient.PostAsJsonAsync("api/workouts/createworkout", workoutDto, cancellationToken));
        }
    }
}
