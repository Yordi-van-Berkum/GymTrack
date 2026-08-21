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
            // Controleert of de gebruiker is ingelogd en haalt het user ID uit de claims.
            if (!User.TryGetUserId(out var userId))
                return Unauthorized("Invalid user.");

            // Maakt de workout aan voor de ingelogde gebruiker.
            await _workoutsService.CreateWorkoutAsync(workoutDto, userId, cancellationToken);

            // Geeft een succesvolle response terug wanneer de workout is aangemaakt.
            return Ok("Workout created!");
        }

        [HttpGet("myworkouts")]
        public async Task<IActionResult> GetMyWorkouts(CancellationToken cancellationToken)
        {
            // Controleert of de gebruiker is ingelogd en haalt het user ID uit de claims.
            if (!User.TryGetUserId(out var userId))
                return Unauthorized("Invalid user.");

            // Haalt alle workouts op die van de ingelogde gebruiker zijn.
            var workouts = await _workoutsService.GetMyWorkoutsAsync(userId, cancellationToken);

            // Geeft de workouts terug.
            return Ok(workouts);
        }

        [HttpGet("getworkoutbyid/{workoutId:guid}")]
        public async Task<IActionResult> GetWorkoutById(Guid workoutId, CancellationToken cancellationToken)
        {
            // Controleert of de gebruiker is ingelogd en haalt het user ID uit de claims.
            if (!User.TryGetUserId(out var userId))
                return Unauthorized("Invalid user.");

            // Haalt de workout op en controleert in de service of deze van de gebruiker is.
            var workout = await _workoutsService.GetWorkoutByIdAsync(workoutId, userId, cancellationToken);

            // Geeft de gevonden workout terug.
            return Ok(workout);
        }

        [HttpPost("addexercisetoworkout")]
        public async Task<IActionResult> AddExerciseToWorkout([FromBody] WorkoutExerciseDto workoutExerciseDto, CancellationToken cancellationToken)
        {
            // Controleert of de gebruiker is ingelogd en haalt het user ID uit de claims.
            if (!User.TryGetUserId(out var userId))
                return Unauthorized("Invalid user.");

            // Voegt de oefening toe aan de workout van de ingelogde gebruiker.
            await _workoutsService.AddExerciseToWorkoutAsync(workoutExerciseDto, userId, cancellationToken);

            // Geeft een succesvolle response terug wanneer de oefening is toegevoegd.
            return Ok("Exercise added to workout!");
        }

        [HttpGet("getexercisesbyworkoutid/{workoutId:guid}")]
        public async Task<IActionResult> GetExercisesByWorkoutId(Guid workoutId, CancellationToken cancellationToken)
        {
            // Controleert of de gebruiker is ingelogd en haalt het user ID uit de claims.
            if (!User.TryGetUserId(out var userId))
                return Unauthorized("Invalid user.");

            // Haalt de oefeningen op en controleert in de service of de workout van de gebruiker is.
            var exercises = await _workoutsService.GetExercisesByWorkoutIdAsync(workoutId, userId, cancellationToken);

            // Geeft de gevonden oefeningen terug.
            return Ok(exercises);
        }

        [HttpDelete("deleteworkout/{workoutId:guid}")]
        public async Task<IActionResult> DeleteWorkout(Guid workoutId,CancellationToken cancellationToken)
        {
            // Controleert of de gebruiker is ingelogd en haalt het user ID uit de claims.
            if (!User.TryGetUserId(out var userId))
                return Unauthorized("Invalid user.");

            // Verwijdert de workout die bij de ingelogde gebruiker hoort.
            await _workoutsService.DeleteWorkoutAsync(workoutId,userId, cancellationToken);

            // Geeft een succesvolle response terug wanneer de workout is verwijderd.
            return Ok("Workout deleted successfully!");
        }
    }
}