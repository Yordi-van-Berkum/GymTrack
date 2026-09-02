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
            // Maakt een WorkoutExerciseDto aan met de oefeningen die toegevoegd moet worden in een workout.
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
            // SafeDataApiCallAsync voert de HTTP-aanroep veilig uit. Deze functie staat in de SafeApiHelper.cs in de Services map.
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

        // Controleert of een oefening al aan de workout is toegevoegd.
        public async Task<bool> IsExerciseInWorkoutAsync(Guid workoutId, int exerciseId, CancellationToken cancellationToken = default)
        {
            // SafeActionApiCallAsync voert de HTTP-aanroep veilig uit. Deze functie staat in de SafeApiHelper.cs in de Services map.
            return await _safeApiHelper.SafeDataApiCallAsync<bool>(() => _httpClient.GetAsync($"api/workouts/{workoutId}/exercises/{exerciseId}", cancellationToken));
        }

        // Verwijdert een oefening uit een workout.
        public async Task<string?> DeleteExerciseFromWorkoutAsync(Guid workoutId, int exerciseId,CancellationToken cancellationToken = default)
        {
            // Maakt een WorkoutExerciseDto aan met de oefeningen die verwijderd moet worden uit een workout.
            var exerciseWorkoutDto = new WorkoutExerciseDto
            {
                ExerciseId = exerciseId,
                WorkoutId = workoutId
            };

            // SafeActionApiCallAsync voert de HTTP-aanroep veilig uit. Deze functie staat in de SafeApiHelper.cs in de Services map.
            return await _safeApiHelper.SafeActionApiCallAsync(() => _httpClient.PostAsJsonAsync("api/workouts/deleteexercisefromworkout", exerciseWorkoutDto, cancellationToken));
        }

        // Start een workout en maakt een nieuwe workout sessie aan.
        public async Task<WorkoutSessionDto> StartWorkoutAsync(Guid workoutId, CancellationToken cancellationToken = default)
        {
            // SafeDataApiCallAsync voert de HTTP-aanroep veilig uit. Deze functie staat in de SafeApiHelper.cs in de Services map.
            // Ik gebruik hier niet de SafeActionApiCallAsync, omdat ik een json object terug wil krijgen. En SafeActionApiCallAsync alleen een bericht terug stuurt.
            return await _safeApiHelper.SafeDataApiCallAsync<WorkoutSessionDto>(() => _httpClient.PostAsync($"api/workouts/startworkout/{workoutId}", null, cancellationToken));
        }

        // Haalt alle oefeningen van de actieve workout sessie op.
        public async Task<List<Exercise>> GetWorkoutExercisesAsync(Guid workoutSessionId, CancellationToken cancellationToken = default)
        {
            // SafeDataApiCallAsync voert de HTTP-aanroep veilig uit. Deze functie staat in de SafeApiHelper.cs in de Services map.
            return await _safeApiHelper.SafeDataApiCallAsync<List<Exercise>>(() => _httpClient.GetAsync($"api/workouts/getexercises/{workoutSessionId}", cancellationToken));
        }

        // Voegt de oefening toe aan de actieve workout sessie.
        public async Task<Guid> AddWorkoutSessionExerciseAsync(Guid workoutSessionId, int exerciseId, CancellationToken cancellationToken = default)
        {
            // Maakt een WorkoutSessionExerciseDto aan.
            var workoutSessionExerciseDto = new WorkoutSessionExerciseDto
            {
                WorkoutSessionId = workoutSessionId,
                ExerciseId = exerciseId
            };

            // SafeDataApiCallAsync voert de HTTP-aanroep veilig uit. Deze functie staat in de SafeApiHelper.cs in de Services map.
            // We gebruiken hier SafeDataApiCallAsync en niet SafeActionApiCallAsync omdat we een guid ontvangen.
            return await _safeApiHelper.SafeDataApiCallAsync<Guid>(() => _httpClient.PostAsJsonAsync("api/workouts/addworkoutsessionexercise", workoutSessionExerciseDto, cancellationToken));
        }

        // Maakt een nieuwe workout set aan bij een workout session.
        public async Task<string> AddWorkoutSetAsync(Guid workoutSessionExerciseId, int setNumber, decimal weight, int reps, CancellationToken cancellationToken = default)
        {
            // Maakt een WorkoutSetDto aan met de gegevens van de nieuwe set.
            var workoutSetDto = new WorkoutSetDto
            {
                WorkoutSessionExerciseId = workoutSessionExerciseId,
                SetNumber = setNumber,
                Weight = weight,
                Reps = reps
            };

            // SafeActionApiCallAsync voert de HTTP-aanroep veilig uit. Deze functie staat in de SafeApiHelper.cs in de Services map.
            return await _safeApiHelper.SafeActionApiCallAsync(() => _httpClient.PostAsJsonAsync("api/workouts/addworkoutset", workoutSetDto, cancellationToken));
        }

        // Haalt de workout summary op van een workout session.
        public async Task<WorkoutSummaryDto> GetWorkoutSummaryAsync(Guid workoutSessionId, CancellationToken cancellationToken = default)
        {
            // SafeDataApiCallAsync voert de HTTP-aanroep veilig uit. Deze functie staat in de SafeApiHelper.cs in de Services map.
            return await _safeApiHelper.SafeDataApiCallAsync<WorkoutSummaryDto>(() => _httpClient.GetAsync($"api/workouts/summary/{workoutSessionId}", cancellationToken));
        }

        // Verwijdert een actieve workout sessie van de ingelogde gebruiker.
        public async Task<string> DeleteWorkoutSessionAsync(Guid workoutSessionId, CancellationToken cancellationToken = default)
        {

            // SafeActionApiCallAsync voert de HTTP-aanroep veilig uit. Deze functie staat in de SafeApiHelper.cs in de Services map.
            return await _safeApiHelper.SafeActionApiCallAsync(() => _httpClient.DeleteAsync($"api/workouts/deleteworkoutsession/{workoutSessionId}",cancellationToken));
        }

        // Rondt de actieve workout sessie af.
        public async Task<string> CompleteWorkoutSessionAsync(Guid workoutSessionId, CancellationToken cancellationToken = default)
        {
            // SafeActionApiCallAsync voert de HTTP-aanroep veilig uit. Deze functie staat in de SafeApiHelper.cs in de Services map.
            return await _safeApiHelper.SafeActionApiCallAsync(() => _httpClient.PostAsync($"api/workouts/completeworkoutsession/{workoutSessionId}", null, cancellationToken));
        }

        // Controleert of de workout session nog bestaat.
        public async Task<bool> WorkoutSessionExistsAsync(Guid workoutSessionId, CancellationToken cancellationToken = default)
        {
            // SafeApiCallAsync voert de HTTP-aanroep veilig uit. Deze functie staat in de SafeApiHelper.cs in de Services map.
            return await _safeApiHelper.SafeDataApiCallAsync<bool>(() => _httpClient.GetAsync($"api/workouts/workoutsessionexists/{workoutSessionId}", cancellationToken));
        }
    }
}
