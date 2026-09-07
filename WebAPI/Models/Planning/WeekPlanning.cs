namespace WebAPI.Models.Planning
{
    public class WeekPlanning
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }

        public Guid WorkoutId { get; set; }

        public DayOfWeek Day { get; set; }

        public WebAPI.Models.Workout.Workout Workout { get; set; } = null!;
    }
}