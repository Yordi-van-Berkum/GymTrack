using Blazor.Models.Exercises;
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

        // Haalt alle workouts op van de ingelogde gebruiker.
        public async Task<List<Workout>> GetMyWorkoutsAsync(CancellationToken cancellationToken = default)
        {
            // SafeDataApiCallAsync voert de HTTP-aanroep veilig uit. Deze functie staat in de SafeApiHelper.cs in de Services map.
            return await _safeApiHelper.SafeDataApiCallAsync<List<Workout>>(() => _httpClient.GetAsync("api/workouts/myworkouts", cancellationToken));
        }

        // Haalt één workout op van de ingelogde gebruiker.
        public async Task<Workout> GetWorkoutByIdAsync(Guid workoutId, CancellationToken cancellationToken = default)
        {
            // SafeDataApiCallAsync voert de HTTP-aanroep veilig uit. Deze functie staat in de SafeApiHelper.cs in de Services map.
            return await _safeApiHelper.SafeDataApiCallAsync<Workout>(() => _httpClient.GetAsync($"api/workouts/getworkoutbyid/{workoutId}",cancellationToken));
        }

        // Voegt een oefening toe aan een workout.
        public async Task<string> AddExerciseToWorkoutAsync(Guid workoutId, int exerciseId, CancellationToken cancellationToken = default)
        {
            // Maak het object aan.
            var workoutExerciseDto = new WorkoutExerciseDto
            {
                WorkoutId = workoutId,
                ExerciseId = exerciseId
            };

            // SafeActionApiCallAsync voert de HTTP-aanroep veilig uit. Deze functie staat in de SafeApiHelper.cs in de Services map.
            return await _safeApiHelper.SafeActionApiCallAsync(() => _httpClient.PostAsJsonAsync("api/workouts/addexercisetoworkout", workoutExerciseDto, cancellationToken));
        }

        // Haalt alle oefeningen van een workout op.
        public async Task<List<Exercise>> GetExercisesByWorkoutIdAsync(Guid workoutId, CancellationToken cancellationToken = default)
        {
            // SafeActionApiCallAsync voert de HTTP-aanroep veilig uit. Deze functie staat in de SafeApiHelper.cs in de Services map.
            return await _safeApiHelper.SafeDataApiCallAsync<List<Exercise>>(() => _httpClient.GetAsync($"api/workouts/getexercisesbyworkoutid/{workoutId}", cancellationToken));
        }

        // Verwijdert een workout van de ingelogde gebruiker.
        public async Task<string> DeleteWorkoutAsync(Guid workoutId, CancellationToken cancellationToken = default)
        {
            // SafeActionApiCallAsync voert de HTTP-aanroep veilig uit. Deze functie staat in de SafeApiHelper.cs in de Services map.
            return await _safeApiHelper.SafeActionApiCallAsync(() => _httpClient.DeleteAsync($"api/workouts/deleteworkout/{workoutId}", cancellationToken));
        }

        // Past een bestaande workout aan.
        public async Task<string> UpdateWorkoutAsync(WorkoutDto workoutDto, CancellationToken cancellationToken = default)
        {
            // SafeActionApiCallAsync voert de HTTP-aanroep veilig uit. Deze functie staat in de SafeApiHelper.cs in de Services map.
            return await _safeApiHelper.SafeActionApiCallAsync(() => _httpClient.PutAsJsonAsync("api/workouts/updateworkout", workoutDto, cancellationToken));
        }

    }
}
