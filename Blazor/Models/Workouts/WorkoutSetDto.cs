using System.ComponentModel.DataAnnotations;

namespace Blazor.Models.Workouts
{
    public class WorkoutSetDto
    {
        public Guid WorkoutSessionExerciseId { get; set; }
        public int SetNumber { get; set; }
        [Range(1, 400)]
        public decimal Weight { get; set; }
        [Range(1, 40)]
        public int Reps { get; set; }
    }
}
