using System.ComponentModel.DataAnnotations;

namespace Blazor.Models.Workouts
{
    public class WorkoutDto
    {
        [Required(ErrorMessage = "Workout name is required.")]
        [StringLength(20, ErrorMessage = "Workout name cannot exceed 20 characters.")]
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
