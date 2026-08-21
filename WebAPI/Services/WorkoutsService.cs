using Microsoft.EntityFrameworkCore;
using WebAPI.Exceptions;
using WebAPI.Models.Exercises;
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
                    ExerciseCount = w.WorkoutExercise.Count,
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

        public async Task AddExerciseToWorkoutAsync(WorkoutExerciseDto workoutExerciseDto, Guid userId, CancellationToken cancellationToken = default)
        {
            // Haalt de workout op wanneer deze bestaat én toebehoort aan de ingelogde gebruiker.
            // Include laadt de bestaande koppelingen met oefeningen zodat we kunnen controleren of de oefening al aan deze workout is toegevoegd en de volgende Order kunnen bepalen.
            var workout = await _context.Workouts
                .Include(w => w.WorkoutExercise)
                .FirstOrDefaultAsync(w => w.Id == workoutExerciseDto.WorkoutId && w.UserId == userId, cancellationToken);

            // De workout bestaat niet of behoort niet toe aan de ingelogde gebruiker.
            if (workout is null)
                throw new NotFoundException("Exercise not found!");

            // Controleert of de opgegeven oefening daadwerkelijk bestaat in de database.
            var exerciseExists = await _context.Exercises.AnyAsync(e => e.Id == workoutExerciseDto.ExerciseId, cancellationToken);

            // De oefening bestaat niet in de database.
            if (!exerciseExists)
                throw new NotFoundException("Exercise not found!");

            // Controleert of de oefening al aan deze workout gekoppeld is om dubbele koppelingen te voorkomen.
            var alreadyAdded = workout.WorkoutExercise.Any(ew => ew.ExerciseId == workoutExerciseDto.ExerciseId);

            // De oefening is al onderdeel van deze workout.
            if (alreadyAdded)
                throw new ConflictException("Exercise already added to workout!");

            // Bepaalt de volgende positie van de oefening binnen de workout.
            // Als de workout nog geen oefeningen bevat, begint de Order bij 1.
            var nextOrder = workout.WorkoutExercise.Count == 0 ? 1 : workout.WorkoutExercise.Max(ew => ew.SortOrder) + 1;

            // Maakt een nieuwe koppeling tussen de workout en de oefening aan.
            workout.WorkoutExercise.Add(new WorkoutExercise
            {
                WorkoutId = workout.Id,
                ExerciseId = workoutExerciseDto.ExerciseId,
                SortOrder = nextOrder
            });

            // Slaat de nieuwe koppeling op in de database.
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<List<ExerciseDto>> GetExercisesByWorkoutIdAsync(Guid workoutId, Guid userId, CancellationToken cancellationToken = default)
        {
            // Haal de workout op en controleert of de workout bestaat en van de ingelogde gebruiker is.
            var workout = await _context.Workouts
                .AsNoTracking()
                .FirstOrDefaultAsync(w => w.Id == workoutId && w.UserId == userId, cancellationToken);

            // De workout bestaat niet of is niet van de ingelogde gebruiker.
            if (workout is null)
                throw new NotFoundException("Workout not found.");

            // Haalt de oefeningen van de workout op, gesorteerd op volgorde.
            var exercises = await _context.WorkoutExercises
                .AsNoTracking()
                .Where(ew => ew.WorkoutId == workoutId)
                .OrderBy(ew => ew.SortOrder)
                .Select(ew => new ExerciseDto
                {
                    Id = ew.Exercise.Id,
                    Name = ew.Exercise.Name,
                    ImageUrl = ew.Exercise.ImageUrl

                })
                .ToListAsync(cancellationToken);

            // Geeft de oefeningen terug.
            return exercises;
        }

        public async Task DeleteWorkoutAsync(Guid workoutId, Guid userId, CancellationToken cancellationToken = default)
        {
            // Haal workout op inclusief gekoppelde oefeningen
            var workout = await _context.Workouts
                .Include(w => w.WorkoutExercise)
                .FirstOrDefaultAsync(w => w.Id == workoutId && w.UserId == userId, cancellationToken);

            // Als workout niet bestaat throw exceptions
            if (workout is null)
                throw new NotFoundException("Workout not found.");

            // Verwijderen van de workout en de oefeningen bij deze workout horen
            _context.Workouts.Remove(workout);

            // Opslaan
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}