using WebAPI.Models.Exercises;
using WebAPI.Models.Workout;

namespace WebAPI.Services
{
    public interface IWorkoutsService
    {
        Task CreateWorkoutAsync(WorkoutDto workoutDto, Guid userId, CancellationToken cancellationToken = default);
        Task<List<WorkoutDto>> GetMyWorkoutsAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<WorkoutDto> GetWorkoutByIdAsync(Guid workoutId, Guid userId, CancellationToken cancellationToken = default);
        Task AddExerciseToWorkoutAsync(WorkoutExerciseDto workoutExerciseDto,Guid userId, CancellationToken cancellationToken = default);
        Task<List<ExerciseDto>> GetExercisesByWorkoutIdAsync(Guid workoutId, Guid userId, CancellationToken cancellationToken = default);
        Task DeleteWorkoutAsync(Guid workoutId, Guid userId, CancellationToken cancellationToken = default);
        Task UpdateWorkoutAsync(WorkoutDto workoutDto, Guid userId, CancellationToken cancellationToken = default);
        Task<bool> IsExerciseInWorkoutAsync(Guid workoutId, int exerciseId, Guid userId, CancellationToken cancellationToken = default);
        Task DeleteExerciseFromWorkoutAsync(WorkoutExerciseDto exerciseWorkoutDto, Guid userId, CancellationToken cancellationToken = default);
        Task<WorkoutSessionDto> StartWorkoutAsync(Guid workoutId, Guid userId, CancellationToken cancellationToken = default);
        Task<List<ExerciseDto>> GetWorkoutExercisesAsync(Guid workoutSessionId, Guid userId, CancellationToken cancellationToken = default);
        Task<Guid> AddWorkoutSessionExerciseAsync(WorkoutSessionExerciseDto workoutSessionExerciseDto, Guid userId, CancellationToken cancellationToken = default);
        Task AddWorkoutSetAsync(WorkoutSetDto workoutSetDto, Guid userId, CancellationToken cancellationToken = default);
        Task<WorkoutSummaryDto> GetWorkoutSummaryAsync(Guid workoutSessionId, Guid userId, CancellationToken cancellationToken = default);
        Task DeleteWorkoutSessionAsync(Guid workoutSessionId, Guid userId, CancellationToken cancellationToken = default);
        Task CompleteWorkoutSessionAsync(Guid workoutSessionId, Guid userId, CancellationToken cancellationToken = default);
        Task DeleteInactiveWorkoutSessionsAsync(CancellationToken cancellationToken = default);
        Task<bool> WorkoutSessionExistsAsync(Guid workoutSessionId, Guid userId, CancellationToken cancellationToken = default);
    }
}