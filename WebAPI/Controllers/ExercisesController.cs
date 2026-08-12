using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Services;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ExercisesController : ControllerBase
    {
        private readonly IExercisesService _exercisesService;
        public ExercisesController(IExercisesService exercisesService)
        {
            _exercisesService = exercisesService;
        }

        [HttpGet("musclegroups")]
        public async Task<IActionResult> GetMuscleGroups(CancellationToken cancellationToken)
        {
            // Vraag de service om alle spiergroepen op te halen
            var muscleGroups = await _exercisesService.GetMuscleGroupsAsync(cancellationToken);

            // Return Ok met de lijst van spiergroepen
            return Ok(muscleGroups);
        }
    }
}
