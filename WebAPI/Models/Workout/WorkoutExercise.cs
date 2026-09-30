using ExerciseModel = WebAPI.Models.Exercises.Exercise;

namespace WebAPI.Models.Workout
{
    public class WorkoutExercise
    {
        public Guid WorkoutId { get; set; }

        public Workout Workout { get; set; } = null!;

        public int ExerciseId { get; set; }
        public ExerciseModel Exercise { get; set; } = null!;

        public int SortOrder { get; set; }
    }
}
