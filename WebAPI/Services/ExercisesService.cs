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
                })
                .OrderBy(mg => mg.Name)
                .ToListAsync(cancellationToken);
        }
    }
}
