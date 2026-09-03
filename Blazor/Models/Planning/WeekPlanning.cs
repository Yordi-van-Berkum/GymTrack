namespace Blazor.Models.Planning
{
    public class WeekPlanning
    {
        public Guid Id { get; set; }

        public DayOfWeek Day { get; set; }

        public Guid WorkoutId { get; set; }

        public string WorkoutName { get; set; } = string.Empty;
    }
}
