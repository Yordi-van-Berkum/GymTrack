using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Extensions;
using WebAPI.Models.Workout;
using WebAPI.Services;

namespace WebAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class WorkoutsController : ControllerBase
    {
        private readonly IWorkoutsService _workoutsService;
        public WorkoutsController(IWorkoutsService workoutsService)
        {
            _workoutsService = workoutsService;
        }

        [HttpPost("createworkout")]
        public async Task<IActionResult> CreateWorkout([FromBody] WorkoutDto workoutDto, CancellationToken cancellationToken)
        {
            // Haalt de ID van de ingelogde gebruiker uit de claims.
            if (!User.TryGetUserId(out var userId))
                return Unauthorized("Invalid user.");

            try
            {
                // Maakt de workout aan voor de ingelogde gebruiker.
                await _workoutsService.CreateWorkoutAsync(workoutDto, userId, cancellationToken);

                // Geeft een succesvolle response terug wanneer de workout is aangemaakt.
                return Ok("Workout created!");
            }
            catch (Exception)
            {
                // Vangt onverwachte fouten af zonder interne informatie naar de client te sturen.
                return StatusCode(500, "Something went wrong.");
            }
        }

    }
}