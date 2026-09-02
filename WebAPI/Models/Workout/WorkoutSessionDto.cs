namespace WebAPI.Models.Workout
{
    public class WorkoutSessionDto
    {
        public Guid Id { get; set; }

        public Guid WorkoutId { get; set; }

        public DateTime StartedAt { get; set; }
    }
}
