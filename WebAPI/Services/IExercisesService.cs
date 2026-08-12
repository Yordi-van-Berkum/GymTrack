using WebAPI.Models.Exercises;

namespace WebAPI.Services
{
    public interface IExercisesService
    {
        Task<List<MuscleGroupDto>> GetMuscleGroupsAsync(CancellationToken cancellationToken = default);
        Task<List<ExerciseDto>> GetExercisesByMuscleGroupIdAsync(int muscleGroupId, CancellationToken cancellationToken = default);

        Task<ExerciseDto?> GetExerciseByIdAsync(int exerciseId, CancellationToken cancellationToken = default);
    }
}
