namespace WebAPI.Models.Planning
{
    public class WeekPlanningDto
    {
        public Guid Id { get; set; }

        public DayOfWeek Day { get; set; }

        public Guid WorkoutId { get; set; }

        public string WorkoutName { get; set; } = string.Empty;
    }
}
