using Microsoft.EntityFrameworkCore;
using WebAPI.Models.Exercise;
using WebAPI.Models.Exercises;

namespace WebAPI.Services
{
    public class ExercisesService : IExercisesService
    {
        private readonly ApplicationDbContext _context;
        public ExercisesService(ApplicationDbContext context)
        {
            _context = context;
        }

        // Haalt alle spiergroepen op uit de database en sorteerd deze op naam.
        public async Task<List<MuscleGroupDto>> GetMuscleGroupsAsync(CancellationToken cancellationToken = default)
        {
            return await _context.MuscleGroups.AsNoTracking()
                .Select(mg => new MuscleGroupDto
                {
                    Id = mg.Id,
                    Name = mg.Name,
                    Description = mg.Description,
                    ImageUrl = mg.ImageUrl,
                    ExerciseCount = mg.ExerciseMuscleGroups.Count()
                })
                .OrderBy(mg => mg.Name)
                .ToListAsync(cancellationToken);
        }

        // Haalt alle oefeningen op uit de database van een spiergroep en sorteerd deze op naam.
        public async Task<List<ExerciseDto>> GetExercisesByMuscleGroupIdAsync(int muscleGroupId,CancellationToken cancellationToken = default)
        {
            return await _context.Exercises.AsNoTracking()
                .Where(e => e.ExerciseMuscleGroups
                    .Any(emg => emg.MuscleGroupId == muscleGroupId))
                .Select(e => new ExerciseDto
                {
                    Id = e.Id,
                    Name = e.Name,
                    ImageUrl = e.ImageUrl
                })
                .OrderBy(e => e.Name)
                .ToListAsync(cancellationToken);
        }

        // Haalt de oefening op met het meegestuurde id.
        public async Task<ExerciseDto?> GetExerciseByIdAsync(int exerciseId, CancellationToken cancellationToken = default)
        {
            return await _context.Exercises.AsNoTracking()
                .Where(e => e.Id == exerciseId)
                .Select(e => new ExerciseDto
                {
                    Id = e.Id,
                    Name = e.Name,
                    Description = e.Description,
                    ImageUrl = e.ImageUrl
                })
                .FirstOrDefaultAsync(cancellationToken);
        }

        // Haalt het hoogste gewicht op dat de gebruiker ooit heeft gebruikt voor een oefening.
        public async Task<ExercisePersonalRecordDto?> GetExercisePersonalRecordAsync(int exerciseId, Guid userId, CancellationToken cancellationToken = default)
        {
            // Zoekt alle sets die bij de opgegeven oefening horen
            // en controleert tegelijkertijd of de workout session van de ingelogde gebruiker is.
            // Alleen afgeronde workout sessions worden meegenomen.
            var personalRecord = await _context.WorkoutSets
                .AsNoTracking()
                .Where(ws => ws.WorkoutSessionExercise.ExerciseId == exerciseId && ws.WorkoutSessionExercise.WorkoutSession.Workout.UserId == userId && ws.WorkoutSessionExercise.WorkoutSession.IsCompleted)
                .OrderByDescending(ws => ws.Weight)
                .ThenByDescending(ws => ws.Reps)
                .Select(ws => new ExercisePersonalRecordDto
                {
                    Weight = ws.Weight,
                    Reps = ws.Reps
                })
                .FirstOrDefaultAsync(cancellationToken);

            // Return PR
            // Wanneer de oefening nog niet gedaan is stuur 0 kg en 0 reps terug.
            return personalRecord ?? new ExercisePersonalRecordDto
            {
                Weight = 0,
                Reps = 0
            };
        }

        // Haalt de laatste uitgevoerde set op van een oefening.
        public async Task<ExerciseLastPerformedDto?> GetExerciseLastPerformedAsync(int exerciseId, Guid userId, CancellationToken cancellationToken = default)
        {
            // Zoekt alle sets die bij de opgegeven oefening horen.
            // Controleert tegelijkertijd of de workout session van de ingelogde gebruiker is.
            // Alleen afgeronde workout sessions worden meegenomen.
            // Pakt de laatste set van de laatste keer dat deze oefening uitgevoerd is.
            var lastPerformed = await _context.WorkoutSets
                .AsNoTracking()
                .Where(ws => ws.WorkoutSessionExercise.ExerciseId == exerciseId && ws.WorkoutSessionExercise.WorkoutSession.Workout.UserId == userId && ws.WorkoutSessionExercise.WorkoutSession.IsCompleted)
                .OrderByDescending(ws => ws.WorkoutSessionExercise.WorkoutSession.StartedAt)
                .ThenByDescending(ws => ws.SetNumber)
                .Select(ws => new ExerciseLastPerformedDto
                {
                    Weight = ws.Weight,
                    Reps = ws.Reps
                })
                .FirstOrDefaultAsync(cancellationToken);

            // Return lastPerformed als er een oefening gedaan is.
            // Geeft 0 kg en 0 reps terug wanneer de gebruiker deze oefening nog nooit heeft uitgevoerd.
            return lastPerformed ?? new ExerciseLastPerformedDto
            {
                Weight = 0,
                Reps = 0
            };
        }
    }
}
