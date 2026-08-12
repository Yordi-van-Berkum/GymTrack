namespace WebAPI.Models.Exercises
{
    public class Exercise
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Description { get; set; }
        public string? ImageUrl { get; set; }

        public ICollection<ExerciseMuscleGroup> ExerciseMuscleGroups { get; set; }
    }
}
