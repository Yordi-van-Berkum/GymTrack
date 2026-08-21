using System.ComponentModel.DataAnnotations;

namespace Blazor.Models.Workouts
{
    public class WorkoutDto
    {
        [Required(ErrorMessage = "Workout name is required.")]
        [StringLength(20, MinimumLength = 2, ErrorMessage = "Workout name must be between 2 and 20 characters.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Workout type is required.")]
        public WorkoutType Type { get; set; }
    }

    public enum WorkoutType
    {
        Strength,
        Cardio,
        Mixed,
    }
}
