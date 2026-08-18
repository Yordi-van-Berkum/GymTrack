using System.ComponentModel.DataAnnotations;

namespace WebAPI.Models.Workout
{
    public class WorkoutDto
    {
        public Guid Id { get; set; }

        [Required]
        [StringLength(20, MinimumLength = 2)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public WorkoutType Type { get; set; }

        public int ExerciseCount { get; set; }

    }
}