using Microsoft.EntityFrameworkCore;
using WebAPI.Exceptions;
using WebAPI.Models;
using WebAPI.Models.Planning;

namespace WebAPI.Services
{
    public class PlanningService : IPlanningService
    {
        private readonly ApplicationDbContext _context;

        public PlanningService(ApplicationDbContext context)
        {
            _context = context;
        }


        // Haalt de weekplanning op van de ingelogde gebruiker.
        public async Task<List<WeekPlanningDto>> GetMyWeekPlanningAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await _context.Planning
             .AsNoTracking()
             .Where(p => p.UserId == userId)
             .Select(p => new WeekPlanningDto
             {
                 Id = p.Id,
                 Day = p.Day,
                 WorkoutId = p.WorkoutId,
                 WorkoutName = p.Workout.Name
             })
             .OrderBy(p => p.Day)
             .ToListAsync(cancellationToken);
        }


        // Verwijdert een workout uit de weekplanning.
        public async Task DeleteDayPlanningAsync(Guid planningId, Guid userId, CancellationToken cancellationToken = default)
        {
            // Haalt de dagplanning op en controleert of deze van de ingelogde gebruiker is.
            var dayPlanning = await _context.Planning
                .FirstOrDefaultAsync(p => p.Id == planningId && p.UserId == userId, cancellationToken);

            // De dagplanning bestaat niet of behoort niet toe aan de ingelogde gebruiker.
            if (dayPlanning is null)
                throw new NotFoundException("Day planning not found.");

            // Verwijdert de dagplanning.
            _context.Planning.Remove(dayPlanning);

            // Slaat de wijziging op in de database.
            await _context.SaveChangesAsync(cancellationToken);
        }


        // Voegt een workout toe aan een dag in de weekplanning.
        public async Task AddWorkoutToPlanningAsync(Guid workoutId, DayOfWeek day, Guid userId, CancellationToken cancellationToken = default)
        {
            // Controleert of de opgegeven dag geldig is.
            if (!Enum.IsDefined(typeof(DayOfWeek), day))
                throw new InvalidOperationException("Invalid day.");

            // Controleert of de workout bestaat en van de ingelogde gebruiker is.
            var workoutExists = await _context.Workouts
                .AnyAsync(w => w.Id == workoutId && w.UserId == userId, cancellationToken);

            // De workout bestaat niet of behoort niet toe aan de ingelogde gebruiker.
            if (!workoutExists)
                throw new NotFoundException("Workout not found.");

            // Controleert of er al een workout op deze dag staat.
            var dayAlreadyHasWorkout = await _context.Planning
                .AnyAsync(p => p.UserId == userId && p.Day == day, cancellationToken);

            // Er staat al een workout gepland op deze dag.
            if (dayAlreadyHasWorkout)
                throw new ConflictException("A workout has already been added for this day.");

            // Maakt een nieuwe planning aan voor de gebruiker met de gekozen workout en dag.
            var planning = new WeekPlanning
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                WorkoutId = workoutId,
                Day = day
            };

            // Voegt de nieuwe planning toe aan de database.
            _context.Planning.Add(planning);

            // Slaat de nieuwe planning op in de database.
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}