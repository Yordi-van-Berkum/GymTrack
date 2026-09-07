using WebAPI.Models.Planning;

namespace WebAPI.Services
{
    public interface IPlanningService
    {
        Task<List<WeekPlanningDto>> GetMyWeekPlanningAsync(Guid userId, CancellationToken cancellationToken = default);
        Task DeleteDayPlanningAsync(Guid planningId, Guid userId, CancellationToken cancellationToken = default);
        Task AddWorkoutToPlanningAsync(Guid workoutId,DayOfWeek day, Guid userId, CancellationToken cancellationToken = default);
    }
}