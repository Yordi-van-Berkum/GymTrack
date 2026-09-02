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

            // Slaat de wijzigingen op in de database.
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateWorkoutAsync(WorkoutDto workoutDto, Guid userId, CancellationToken cancellationToken = default)
        {
            // Haalt de workout op en controleert of deze van de ingelogde gebruiker is.
            var workout = await _context.Workouts
                .FirstOrDefaultAsync(w => w.Id == workoutDto.Id && w.UserId == userId, cancellationToken);

            // De workout bestaat niet of is niet van de ingelogde gebruiker.
            if (workout is null)
                throw new NotFoundException("Workout not found.");

            // Past de gegevens van de workout aan.
            workout.Name = workoutDto.Name.Trim();
            workout.Type = workoutDto.Type;

            // Slaat de wijzigingen op in de database.
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<bool> IsExerciseInWorkoutAsync(Guid workoutId, int exerciseId,Guid userId, CancellationToken cancellationToken = default)
        {
            // Checkt of oefening aan de workout toegevoegd is.
            // True als die toegevoegd is.
            // False als die nog niet toegevoegd is.
            return await _context.WorkoutExercises
                .AsNoTracking()
                .AnyAsync(ew => ew.WorkoutId == workoutId && ew.ExerciseId == exerciseId && ew.Workout.UserId == userId,cancellationToken);
        }

        public async Task DeleteExerciseFromWorkoutAsync(WorkoutExerciseDto exerciseWorkoutDto, Guid userId, CancellationToken cancellationToken = default)
        {
            // Haalt de workout op en controleert direct of deze van de ingelogde gebruiker is.
            var workout = await _context.Workouts
                .FirstOrDefaultAsync(w => w.Id == exerciseWorkoutDto.WorkoutId && w.UserId == userId, cancellationToken);

            // Workout bestaat niet of behoort niet toe aan de ingelogde gebruiker.
            if (workout is null)
                throw new InvalidOperationException("Workout not found.");

            // Zoekt de koppeling tussen de workout en de oefening.
            var exerciseWorkout = await _context.WorkoutExercises
                .FirstOrDefaultAsync(ew => ew.WorkoutId == exerciseWorkoutDto.WorkoutId && ew.ExerciseId == exerciseWorkoutDto.ExerciseId, cancellationToken);

            // De oefening is niet toegevoegd aan deze workout.
            if (exerciseWorkout is null)
                throw new InvalidOperationException("Exercise is not in workout.");

            // Verwijdert de koppeling uit de database.
            _context.WorkoutExercises.Remove(exerciseWorkout);

            // Slaat de wijziging op.
            await _context.SaveChangesAsync(cancellationToken);
        }

        // Start een nieuwe workout sessie voor de ingelogde gebruiker.
        public async Task<WorkoutSessionDto> StartWorkoutAsync(Guid workoutId, Guid userId, CancellationToken cancellationToken = default)
        {
            // Controleert of de workout bestaat en van de ingelogde gebruiker is.
            var workoutExists = await _context.Workouts
                .AnyAsync(w => w.Id == workoutId && w.UserId == userId,cancellationToken);

            // De workout bestaat niet of behoort niet toe aan de gebruiker.
            if (!workoutExists)
                throw new NotFoundException("Workout not found.");

            // Maakt een nieuwe workout sessie aan.
            var workoutSession = new WorkoutSession
            {
                Id = Guid.NewGuid(),
                WorkoutId = workoutId,
                StartedAt = DateTime.UtcNow,
                IsCompleted = false,
                LastActivityAt = DateTime.UtcNow,
            };

            // Voegt de nieuwe workout sessie toe aan de database.
            _context.WorkoutSessions.Add(workoutSession);

            // Slaat de workout sessie op.
            await _context.SaveChangesAsync(cancellationToken);

            // Geeft de aangemaakte workout sessie terug naar de frontend.
            return new WorkoutSessionDto
            {
                Id = workoutSession.Id,
                WorkoutId = workoutSession.WorkoutId,
                StartedAt = workoutSession.StartedAt
            };
        }

        // Haalt alle oefeningen van een workout session op.
        public async Task<List<ExerciseDto>> GetWorkoutExercisesAsync(Guid workoutSessionId, Guid userId, CancellationToken cancellationToken = default)
        {
            // Controleert of de workout session bestaat en bij de ingelogde gebruiker hoort.
            var workoutSession = await _context.WorkoutSessions
                .AsNoTracking()
                .FirstOrDefaultAsync(ws => ws.Id == workoutSessionId && ws.Workout.UserId == userId, cancellationToken);

            // De workout session bestaat niet of behoort niet toe aan de ingelogde gebruiker.
            if (workoutSession is null)
                throw new NotFoundException("Workout session not found.");

            // Haalt alle oefeningen van de workout session op in de volgorde.
            var exercises = await _context.WorkoutExercises
                .AsNoTracking()
                .Where(wse => wse.WorkoutId == workoutSession.WorkoutId)
                .OrderBy(wse => wse.SortOrder)
                .Select(wse => new ExerciseDto
                {
                    Id = wse.Exercise.Id,
                    Name = wse.Exercise.Name,
                    ImageUrl = wse.Exercise.ImageUrl
                })
                .ToListAsync(cancellationToken);

            // Geeft de oefeningen terug.
            return exercises;
        }

        public async Task<Guid> AddWorkoutSessionExerciseAsync(WorkoutSessionExerciseDto workoutSessionExerciseDto, Guid userId, CancellationToken cancellationToken = default)
        {
            // Controleert of de workout session bestaat en bij de ingelogde gebruiker hoort.
            var workoutSession = await _context.WorkoutSessions
                .FirstOrDefaultAsync(ws => ws.Id == workoutSessionExerciseDto.WorkoutSessionId && ws.Workout.UserId == userId, cancellationToken);

            // De workout session bestaat niet of behoort niet toe aan de ingelogde gebruiker.
            if (workoutSession is null)
                throw new NotFoundException("Workout session not found.");

            // Controleert of de oefening bestaat.
            var exerciseExists = await _context.Exercises
                .AnyAsync(e => e.Id == workoutSessionExerciseDto.ExerciseId, cancellationToken);

            // De oefening bestaat niet.
            if (!exerciseExists)
                throw new NotFoundException("Exercise not found.");

            // Controleert of deze oefening al aan deze workout session is toegevoegd.
            // Hierdoor voorkomen we dat dezelfde oefening meerdere keren aan dezelfde sessie wordt gekoppeld.
            var alreadyExists = await _context.WorkoutSessionExercises
                .AnyAsync(wse => wse.WorkoutSessionId == workoutSession.Id && wse.ExerciseId == workoutSessionExerciseDto.ExerciseId, cancellationToken);

            // De oefening is al onderdeel van deze workout session.
            if (alreadyExists)
                throw new ConflictException("Exercise already added to workout session.");

            // Maakt een nieuwe koppeling aan tussen de workout session en de oefening.
            var workoutSessionExercise = new WorkoutSessionExercise
            {
                Id = Guid.NewGuid(),
                WorkoutSessionId = workoutSessionExerciseDto.WorkoutSessionId,
                ExerciseId = workoutSessionExerciseDto.ExerciseId
            };

            // Voegt de nieuwe workout session exercise toe aan de database.
            _context.WorkoutSessionExercises.Add(workoutSessionExercise);

            // Slaat de nieuwe workout session exercise op in de database.
            await _context.SaveChangesAsync(cancellationToken);

            // Geeft het ID van de aangemaakte WorkoutSessionExercise terug.
            return workoutSessionExercise.Id;
        }

        public async Task AddWorkoutSetAsync(WorkoutSetDto workoutSetDto, Guid userId, CancellationToken cancellationToken = default)
        {
            // Haalt de workout session exercise op en controleert of deze bij de ingelogde gebruiker hoort.
            var workoutSessionExercise = await _context.WorkoutSessionExercises
                .Include(wse => wse.WorkoutSession)
                .ThenInclude(ws => ws.Workout)
                .FirstOrDefaultAsync(wse => wse.Id == workoutSetDto.WorkoutSessionExerciseId && wse.WorkoutSession.Workout.UserId == userId, cancellationToken);

            // De workout session exercise bestaat niet of hoort niet bij de ingelogde gebruiker.
            if (workoutSessionExercise is null)
                throw new NotFoundException("Workout session exercise not found.");

            // Maakt een nieuw WorkoutSet object aan.
            var workoutSet = new WorkoutSet
            {
                Id = Guid.NewGuid(),
                WorkoutSessionExerciseId = workoutSetDto.WorkoutSessionExerciseId,
                SetNumber = workoutSetDto.SetNumber,
                Weight = workoutSetDto.Weight,
                Reps = workoutSetDto.Reps
            };

            // Voegt de nieuwe set toe aan de database.
            _context.WorkoutSets.Add(workoutSet);

            // Werkt het tijdstip van de laatste activiteit van de workout session bij.
            // Dit zorgt ervoor dat hij weet dat je er nog mee bezig bent en niet hoeft te verwijderen na een bepaalde tijd.
            workoutSessionExercise.WorkoutSession.LastActivityAt = DateTime.UtcNow;

            // Slaat de nieuwe set op in de database.
            await _context.SaveChangesAsync(cancellationToken);
        }

        // Haalt de workout summary op van een workout session.
        public async Task<WorkoutSummaryDto> GetWorkoutSummaryAsync(Guid workoutSessionId, Guid userId, CancellationToken cancellationToken = default)
        {
            // Haalt de workout session met de workout, oefeningen en sets op.
            var workoutSession = await _context.WorkoutSessions
                .Include(ws => ws.Workout)
                .Include(ws => ws.Exercises)
                    .ThenInclude(wse => wse.Exercise)
                .Include(ws => ws.Exercises)
                    .ThenInclude(wse => wse.Sets)
                .FirstOrDefaultAsync(ws => ws.Id == workoutSessionId && ws.Workout.UserId == userId, cancellationToken);

            // Controleert of de workout session bestaat.
            if (workoutSession is null)
                throw new NotFoundException("Workout session not found.");

            // Maakt de WorkoutSummaryDto aan.
            var summary = new WorkoutSummaryDto
            {
                WorkoutSessionId = workoutSession.Id,
                WorkoutName = workoutSession.Workout.Name,
                Date = workoutSession.StartedAt,
            };

            // Loopt door alle oefeningen van de workout session.
            foreach (var sessionExercise in workoutSession.Exercises)
            {
                // Maakt de WorkoutSummaryExerciseDto van de huidige oefening.
                var exerciseSummary = new WorkoutSummaryExerciseDto
                {
                    ExerciseName = sessionExercise.Exercise.Name
                };

                // Loopt door alle sets van de huidige oefening.
                foreach (var set in sessionExercise.Sets.OrderBy(s => s.SetNumber))
                {
                    // Voegt de set toe aan de oefening.
                    exerciseSummary.Sets.Add(new WorkoutSummarySetDto
                    {
                        SetNumber = set.SetNumber,
                        Weight = set.Weight,
                        Reps = set.Reps
                    });
                }

                // Voegt de oefening toe aan de workout summary.
                summary.Exercises.Add(exerciseSummary);
            }

            // Geeft de complete workout summary terug.
            return summary;
        }

        // Rondt een workout session af.
        public async Task CompleteWorkoutSessionAsync(Guid workoutSessionId, Guid userId, CancellationToken cancellationToken = default)
        {
            // Haalt de workout session op en controleert of deze van de ingelogde gebruiker is.
            var workoutSession = await _context.WorkoutSessions
                .FirstOrDefaultAsync(ws => ws.Id == workoutSessionId && ws.Workout.UserId == userId, cancellationToken);

            // De workout session bestaat niet of behoort niet toe aan de ingelogde gebruiker.
            if (workoutSession is null)
                throw new NotFoundException("Workout session not found.");

            // Geeft aan dat de workout volledig is afgerond.
            workoutSession.IsCompleted = true;

            // Werkt het tijdstip van de laatste activiteit van de workout session bij.
            workoutSession.LastActivityAt = DateTime.UtcNow;

            // Slaat de wijziging op in de database.
            await _context.SaveChangesAsync(cancellationToken);
        }

        // Verwijderen van de workout sessie en de oefeningen en sets die aan deze sessie hangen.
        // Deze delete wordt gebruikt wanneer cancellationToken gebruikt wordt.
        public async Task DeleteWorkoutSessionAsync(Guid workoutSessionId, Guid userId, CancellationToken cancellationToken = default)
        {
            // Haalt de workout session op en controleert of deze van de ingelogde gebruiker is.
            var workoutSession = await _context.WorkoutSessions
                .FirstOrDefaultAsync(ws => ws.Id == workoutSessionId &&  ws.Workout.UserId == userId, cancellationToken);

            // De workout session bestaat niet of behoort niet toe aan de gebruiker.
            if (workoutSession is null)
                throw new NotFoundException("Workout session not found.");

            // Verwijdert de workout session.
            // De gekoppelde exercises en sets worden verwijderd via de cascade-relatie
            _context.WorkoutSessions.Remove(workoutSession);

            // Slaat de wijzigingen op.
            await _context.SaveChangesAsync(cancellationToken);
        }

        // Verwijdert onafgemaakte workout sessies die te lang niet actief zijn geweest.
        // Deze delete wordt gebruikt wanneer een gebruiker de pagina volledig afsluit en cancellationToken niet gebruikt kan worden.
        public async Task DeleteInactiveWorkoutSessionsAsync(CancellationToken cancellationToken = default)
        {
            // Bepaalt de tijd waarop een workout session als inactief wordt beschouwd.
            var inactiveSince = DateTime.UtcNow.AddHours(-1);

            // Haalt alle onafgemaakte workout sessies op die langer dan één uur niet actief zijn geweest.
            var inactiveWorkoutSessions = await _context.WorkoutSessions
                .Where(ws => !ws.IsCompleted && ws.LastActivityAt < inactiveSince)
                .ToListAsync(cancellationToken);

            // Verwijdert de gevonden inactieve workout sessies.
            _context.WorkoutSessions.RemoveRange(inactiveWorkoutSessions);

            // Slaat de wijzigingen op in de database.
            await _context.SaveChangesAsync(cancellationToken);
        }

        // Controleert of een workout session nog bestaat.
        public async Task<bool> WorkoutSessionExistsAsync(Guid workoutSessionId, Guid userId, CancellationToken cancellationToken = default)
        {
            // Controleert of de workout session bestaat en van de ingelogde gebruiker is.
            return await _context.WorkoutSessions.AnyAsync(ws => ws.Id == workoutSessionId && ws.Workout.UserId == userId, cancellationToken);
        }


    }
}