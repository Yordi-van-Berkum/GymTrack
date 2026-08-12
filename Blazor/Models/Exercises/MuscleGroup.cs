namespace Blazor.Models.Exercises
{
    public class MuscleGroup
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!;

        public string? Description { get; set; }

        public string? ImageUrl { get; set; }

        public int ExerciseCount { get; set; }
    }
}
