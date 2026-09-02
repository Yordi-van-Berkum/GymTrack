namespace WebAPI.Models.Workout
{
    public class WorkoutSession
    {
        public Guid Id { get; set; }

        public Guid WorkoutId { get; set; }

        public DateTime StartedAt { get; set; }

        public bool IsCompleted { get; set; }
        public DateTime LastActivityAt { get; set; }

        public Workout Workout { get; set; } = null!;

        public ICollection<WorkoutSessionExercise> Exercises { get; set; } = new List<WorkoutSessionExercise>();
    }
}
