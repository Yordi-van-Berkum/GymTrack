namespace Blazor.Models.Workouts
{
    public class Workout
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = "";

        public int ExerciseCount { get; set; }
        public WorkoutType Type { get; set; }
    }
}
