using WebAPI.Models.Workout;

namespace WebAPI.Services
{
    public class WorkoutsService : IWorkoutsService
    {
        private readonly ApplicationDbContext _context;

        public WorkoutsService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task CreateWorkoutAsync(WorkoutDto workoutDto, Guid userId, CancellationToken cancellationToken = default)
        {
            // Maakt een nieuwe workout aan voor de ingelogde gebruiker.
            var workout = new Workout
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Name = workoutDto.Name.Trim(),
                Type = workoutDto.Type,
            };

            // Voegt de workout toe aan de database.
            _context.Workouts.Add(workout);

            // Slaat de nieuwe workout op in de database.
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
