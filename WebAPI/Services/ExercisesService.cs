using Microsoft.EntityFrameworkCore;
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

    }
}
