using Blazor.Models.Exercises;

namespace Blazor.Services
{
    public class ExercisesService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ExercisesService> _logger;
        private readonly SafeApiHelper _safeApiHelper;

        public ExercisesService(HttpClient httpClient, ILogger<ExercisesService> logger, SafeApiHelper safeApiHelper)
        {
            _httpClient = httpClient;
            _logger = logger;
            _safeApiHelper = safeApiHelper;
        }

        public async Task<List<MuscleGroup>> GetMuscleGroupsAsync(CancellationToken cancellationToken = default)
        {
            // Haalt alle spiergroepen op vanuit de backend.
            // SafeDataApiCallAsync voert de HTTP-aanroep veilig uit. Deze functie staat in de SafeApiHelper.cs in de Services map.
            return await _safeApiHelper.SafeDataApiCallAsync<List<MuscleGroup>>(() => _httpClient.GetAsync("api/exercises/musclegroups",cancellationToken));
        }
    }
}
