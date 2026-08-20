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
    }
}