namespace WebAPI.Models.Workout
{
    public class WorkoutSetDto
    {
        public Guid WorkoutSessionExerciseId { get; set; }
        public int SetNumber { get; set; }
        public decimal Weight { get; set; }
        public int Reps { get; set; }
    }
}
