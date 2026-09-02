namespace Blazor.Models.Workouts
{
    public class WorkoutSummaryDto
    {
        public Guid WorkoutSessionId { get; set; }

        public string WorkoutName { get; set; } = string.Empty;

        public DateTime Date { get; set; }

        public List<WorkoutSummaryExerciseDto> Exercises { get; set; } = new();
    }

    public class WorkoutSummaryExerciseDto
    {
        public string ExerciseName { get; set; } = string.Empty;

        public List<WorkoutSummarySetDto> Sets { get; set; } = new();
    }

    public class WorkoutSummarySetDto
    {
        public int SetNumber { get; set; }

        public decimal Weight { get; set; }

        public int Reps { get; set; }
    }
}
