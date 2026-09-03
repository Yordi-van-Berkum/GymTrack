using System.Net.Http.Json;
using Blazor.Models;
using Blazor.Models.Planning;

namespace Blazor.Services
{
    public class PlanningService
    {
        private readonly HttpClient _httpClient;
        private readonly SafeApiHelper _safeApiHelper;

        public PlanningService(HttpClient httpClient, SafeApiHelper safeApiHelper)
        {
            _httpClient = httpClient;
            _safeApiHelper = safeApiHelper;
        }


        // Haalt de weekplanning van de ingelogde gebruiker op.
        public async Task<List<WeekPlanning>> GetMyWeekPlanningAsync(CancellationToken cancellationToken = default)
        {
            // SafeDataApiCallAsync voert de HTTP-aanroep veilig uit. Deze functie staat in de SafeApiHelper.cs in de Services map.
            return await _safeApiHelper.SafeDataApiCallAsync<List<WeekPlanning>>(() => _httpClient.GetAsync("api/planning/getmyweekplanning", cancellationToken));
        }


        // Verwijdert een workout uit de weekplanning.
        public async Task<string> DeleteDayPlanningAsync(Guid planningId, CancellationToken cancellationToken = default)
        {
            // SafeActionApiCallAsync voert de HTTP-aanroep veilig uit. Deze functie staat in de SafeApiHelper.cs in de Services map.
            return await _safeApiHelper.SafeActionApiCallAsync(() => _httpClient.DeleteAsync($"api/planning/deletedayplanning/{planningId}", cancellationToken));
        }


        // Voegt een workout toe aan een dag in de weekplanning.
        public async Task<string> AddWorkoutToPlanningAsync(Guid workoutId, DayOfWeek selectedDay, CancellationToken cancellationToken = default)
        {
            // SafeActionApiCallAsync voert de HTTP-aanroep veilig uit. Deze functie staat in de SafeApiHelper.cs in de Services map.
            return await _safeApiHelper.SafeActionApiCallAsync(() => _httpClient.PostAsync($"api/planning/adddayplanning/{workoutId}/{selectedDay}", null, cancellationToken));
        }
    }
}