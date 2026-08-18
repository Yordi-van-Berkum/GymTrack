using Microsoft.EntityFrameworkCore;
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

        // Haalt alle workouts op van de ingelogde gebruiker en sorteert deze op naam.
        public async Task<List<WorkoutDto>> GetMyWorkoutsAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await _context.Workouts
                .AsNoTracking()
                .Where(w => w.UserId == userId)
                .Select(w => new WorkoutDto
                {
                    Id = w.Id,
                    Name = w.Name,
                    Type = w.Type,
                    ExerciseCount = 0,
                })
                .OrderBy(w => w.Name)
                .ToListAsync(cancellationToken);
        }

        // Haal de workout op die gelijk is aan het meegestuurde WorkoutId.
        public async Task<WorkoutDto?> GetWorkoutByIdAsync(Guid workoutId, Guid userId, CancellationToken cancellationToken = default)
        {
            return await _context.Workouts
                .AsNoTracking()
                .Where(w => w.Id == workoutId && w.UserId == userId)
                .Select(w => new WorkoutDto
                {
                    Id = w.Id,
                    Name = w.Name,
                    Type = w.Type,
                })
                .FirstOrDefaultAsync(cancellationToken);
        }
    }
}
