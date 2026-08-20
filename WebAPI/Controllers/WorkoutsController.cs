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

        [HttpGet("myworkouts")]
        public async Task<IActionResult> GetMyWorkoutsAsync(CancellationToken cancellationToken)
        {
            // Haalt de ID van de ingelogde gebruiker uit de claims.
            if (!User.TryGetUserId(out var userId))
                return Unauthorized("Invalid user.");

            try
            {
                // Haalt alle workouts op die van de ingelogde gebruiker zijn.
                var workouts = await _workoutsService.GetMyWorkoutsAsync(userId, cancellationToken);

                // Geeft de workouts terug.
                return Ok(workouts);
            }
            catch (Exception)
            {
                // Vangt onverwachte fouten af zonder interne informatie naar de client te sturen.
                return StatusCode(500, "Something went wrong.");
            }
        }

        [HttpGet("getworkoutbyid/{workoutId:guid}")]
        public async Task<IActionResult> GetWorkoutById(Guid workoutId, CancellationToken cancellationToken)
        {
            // Haalt de ID van de ingelogde gebruiker uit de claims.
            if (!User.TryGetUserId(out var userId))
                return Unauthorized("Invalid user.");

            try
            {
                // Haalt de workout op van de ingelogde gebruiker.
                var workout = await _workoutsService.GetWorkoutByIdAsync(workoutId, userId, cancellationToken);

                // Workout bestaat niet of behoort niet toe aan de gebruiker.
                if (workout is null)
                    return NotFound("Workout not found.");

                return Ok(workout);
            }
            catch (Exception)
            {
                return StatusCode(500, "Something went wrong.");
            }
        }

        [HttpPost("addexercisetoworkout")]
        public async Task<IActionResult> AddExerciseToWorkout(WorkoutExerciseDto workoutExerciseDto)
        {
            // Als de gebruiker niet is ingelogd, stuur Unauthorized terug met een toast message.
            if (!User.TryGetUserId(out var userId))
                return Unauthorized("Invalid user");

            try
            {
                // Stuur workoutExerciseDto en userId naar de service om de oefening aan de workout in de database toe te voegen.
                await _workoutsService.AddExerciseToWorkoutAsync(workoutExerciseDto, userId);
                // Return Ok als goed gegaan
                return Ok("Exercise added to workout!");
            }
            // Catch alle fouten waarvan de service InvalidOperationException terug geeft en return met message.
            catch (InvalidOperationException ex)
            {
                return NotFound(ex.Message);
            }
            // Catch alle onverwachten fouten.
            catch (Exception)
            {
                return StatusCode(500, "Something went wrong.");
            }
        }

        [HttpGet("getexercisesbyworkoutid/{workoutId:guid}")]
        public async Task<IActionResult> GetExercisesByWorkoutId(Guid workoutId, CancellationToken cancellationToken)
        {
            // Haal id op van ingelogde gebruiker
            if (!User.TryGetUserId(out var userId))
                return Unauthorized("Invalid user!");

            try
            {
                // Haal de oefeningen op vanuit de service
                var exercises = await _workoutsService.GetExercisesByWorkoutIdAsync(workoutId, userId, cancellationToken);

                // Return Ok met de lijst van oefeningen
                return Ok(exercises);
            }
            // Catch alle fouten waarvan de service InvalidOperationException terug geeft en return met message
            catch (InvalidOperationException ex)
            {
                return NotFound(ex.Message);
            }
            // Catch alle onverwachten fouten.
            catch (Exception)
            {
                return StatusCode(500, "Something went wrong.");
            }
        }
    }
}