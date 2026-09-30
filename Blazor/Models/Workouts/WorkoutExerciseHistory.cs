namespace Blazor.Models.Workouts
{
    public class WorkoutExerciseHistory
    {
        public int ExerciseId { get; set; }
        public string ExerciseName { get; set; } = null!;
        public List<WorkoutExerciseHistoryEntry> History { get; set; } = new();
    }

    public class WorkoutExerciseHistoryEntry
    {
        public DateTime Date { get; set; }
        public decimal Weight { get; set; }
        public int Reps { get; set; }
    }
}
