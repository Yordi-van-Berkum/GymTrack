using Blazor.Models.Exercises;
using System;

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

        public async Task<List<Exercise>> GetExercisesByMuscleGroupIdAsync(int muscleGroupId, CancellationToken cancellationToken = default)
        {
            // Haalt alle oefeningen op die bij de opgegeven spiergroep horen.
            return await _safeApiHelper.SafeDataApiCallAsync<List<Exercise>>(() => _httpClient.GetAsync($"api/exercises/getexercisesbymusclegroupid/{muscleGroupId}",cancellationToken));
        }

        public async Task<Exercise> GetExerciseByIdAsync(int exerciseId, CancellationToken cancellationToken = default)
        {
            // Haalt de informatie op van een oefening.
            return await _safeApiHelper.SafeDataApiCallAsync<Exercise>(() => _httpClient.GetAsync($"api/exercises/getexercise/{exerciseId}", cancellationToken));
        }

        // Haalt het persoonlijke record op van een oefening.
        public async Task<ExercisePersonalRecord?> GetExercisePersonalRecordAsync(int exerciseId, CancellationToken cancellationToken = default)
        {
            // SafeDataApiCallAsync voert de HTTP-aanroep veilig uit. Deze functie staat in de SafeApiHelper.cs in de Services map.
            return await _safeApiHelper.SafeDataApiCallAsync<ExercisePersonalRecord?>(() => _httpClient.GetAsync($"api/exercises/exercisepersonalrecord/{exerciseId}", cancellationToken));
        }

        // Haalt de laatste uitgevoerde set op van een oefening.
        public async Task<ExerciseLastPerformed?> GetExerciseLastPerformedAsync(int exerciseId, CancellationToken cancellationToken = default)
        {
            // SafeDataApiCallAsync voert de HTTP-aanroep veilig uit. Deze functie staat in de SafeApiHelper.cs in de Services map.
            return await _safeApiHelper.SafeDataApiCallAsync<ExerciseLastPerformed?>(() => _httpClient.GetAsync($"api/exercises/exerciselastperformed/{exerciseId}", cancellationToken));
        }
    }
}
