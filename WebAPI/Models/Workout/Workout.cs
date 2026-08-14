namespace WebAPI.Models.Workout
{
    public class Workout
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }

        public string Name { get; set; } = string.Empty;

        public WorkoutType Type { get; set; }

        public ICollection<WorkoutExercise> WorkoutExercise { get; set; } = new List<WorkoutExercise>();

    }

    public enum WorkoutType
    {
        Strength,
        Cardio,
        Mixed,
    }
}
