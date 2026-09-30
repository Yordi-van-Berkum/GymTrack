namespace WebAPI.Models.Workout
{
    public class WorkoutExerciseHistoryDto
    {
        public int ExerciseId { get; set; }
        public string ExerciseName { get; set; } = null!;
        public List<WorkoutExerciseHistoryEntryDto> History { get; set; } = new();
    }

    public class WorkoutExerciseHistoryEntryDto
    {
        public DateTime Date { get; set; }
        public decimal Weight { get; set; }
        public int Reps { get; set; }
    }
}