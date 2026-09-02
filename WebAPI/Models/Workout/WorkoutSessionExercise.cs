using WebAPI.Models.Exercises;
using WebAPI.Models.Workout;

public class WorkoutSessionExercise
{
    public Guid Id { get; set; }

    public Guid WorkoutSessionId { get; set; }

    public int ExerciseId { get; set; }

    public WorkoutSession WorkoutSession { get; set; } = null!;

    public Exercise Exercise { get; set; } = null!;

    public ICollection<WorkoutSet> Sets { get; set; } = new List<WorkoutSet>();
}