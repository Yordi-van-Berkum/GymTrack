using WebAPI.Models.Workout;

namespace WebAPI.Services
{
    public interface IWorkoutsService
    {
        Task CreateWorkoutAsync(WorkoutDto workoutDto, Guid userId, CancellationToken cancellationToken = default);
    }
}
