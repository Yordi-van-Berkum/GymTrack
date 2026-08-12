using WebAPI.Models.Exercises;

namespace WebAPI.Services
{
    public interface IExercisesService
    {
        Task<List<MuscleGroupDto>> GetMuscleGroupsAsync(CancellationToken cancellationToken = default);
    }
}
