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

        [HttpPut("updateworkout")]
        public async Task<IActionResult> UpdateWorkout([FromBody] WorkoutDto workoutDto, CancellationToken cancellationToken)
        {
            // Controleert of de gebruiker is ingelogd en haalt het user ID uit de claims.
            if (!User.TryGetUserId(out var userId))
                return Unauthorized("Invalid user.");

            // Past de workout aan en controleert in de service of deze van de gebruiker is.
            await _workoutsService.UpdateWorkoutAsync(workoutDto, userId, cancellationToken);

            // Geeft een succesvolle response terug wanneer de workout is aangepast.
            return Ok("Workout updated!");
        }

        [HttpGet("{workoutId:guid}/exercises/{exerciseId:int}")]
        public async Task<IActionResult> IsExerciseInWorkout(Guid workoutId, int exerciseId, CancellationToken cancellationToken)
        {
            // Controleert of de gebruiker is ingelogd en haalt het user ID uit de claims.
            if (!User.TryGetUserId(out var userId))
                return Unauthorized("Invalid user.");

            // Controleert of de oefening aan de workout van de ingelogde gebruiker is toegevoegd.
            var isInWorkout = await _workoutsService.IsExerciseInWorkoutAsync(workoutId, exerciseId, userId, cancellationToken);

            // Geeft true of false terug.
            return Ok(isInWorkout);
        }

        [HttpPost("deleteexercisefromworkout")]
        public async Task<IActionResult> DeleteExerciseFromWorkout([FromBody] WorkoutExerciseDto exerciseWorkoutDto, CancellationToken cancellationToken)
        {
            // Controleert of de gebruiker is ingelogd en haalt het user ID uit de claims.
            if (!User.TryGetUserId(out var userId))
                return Unauthorized("Invalid user.");

            // Verwijdert de oefening uit de workout van de ingelogde gebruiker.
            await _workoutsService.DeleteExerciseFromWorkoutAsync(exerciseWorkoutDto, userId, cancellationToken);

            // Geeft een succesvolle response terug wanneer de oefening verwijderd is.
            return Ok("Exercise removed from workout!");
        }

        [HttpPost("startworkout/{workoutId:guid}")]
        public async Task<IActionResult> StartWorkout(Guid workoutId, CancellationToken cancellationToken)
        {
            // Controleert of de gebruiker is ingelogd en haalt het user ID uit de claims.
            if (!User.TryGetUserId(out var userId))
                return Unauthorized("Invalid user.");

            // Start een nieuwe workout sessie voor de ingelogde gebruiker.
            var workoutSession = await _workoutsService.StartWorkoutAsync(workoutId, userId, cancellationToken);

            // Geeft de aangemaakte workout sessie terug naar de frontend.
            return Ok(workoutSession);
        }

        [HttpGet("getexercises/{workoutSessionId:guid}")]
        public async Task<IActionResult> GetWorkoutExercises(Guid workoutSessionId, CancellationToken cancellationToken)
        {
            // Controleert of de gebruiker is ingelogd en haalt het user ID uit de claims.
            if (!User.TryGetUserId(out var userId))
                return Unauthorized("Invalid user.");

            // Haalt de oefeningen van de workout sessie op.
            var exercises = await _workoutsService.GetWorkoutExercisesAsync(workoutSessionId, userId, cancellationToken);

            // Geeft de oefeningen terug.
            return Ok(exercises);
        }

        [HttpPost("addworkoutsessionexercise")]
        public async Task<IActionResult> AddWorkoutSessionExercise([FromBody] WorkoutSessionExerciseDto workoutSessionExerciseDto, CancellationToken cancellationToken)
        {
            // Controleert of de gebruiker is ingelogd en haalt het user ID uit de claims.
            if (!User.TryGetUserId(out var userId))
                return Unauthorized("Invalid user.");

            // Maakt de WorkoutSessionExercise aan en ontvangt het nieuwe ID.
            var workoutSessionExerciseId = await _workoutsService.AddWorkoutSessionExerciseAsync(workoutSessionExerciseDto, userId, cancellationToken);

            // Geeft het ID van de aangemaakte WorkoutSessionExercise terug.
            return Ok(workoutSessionExerciseId);
        }

        // Voegt een nieuwe set toe aan de huidige workout session exercise.
        [HttpPost("addworkoutset")]
        public async Task<IActionResult> AddWorkoutSet([FromBody] WorkoutSetDto workoutSetDto, CancellationToken cancellationToken)
        {
            // Controleert of de gebruiker is ingelogd en haalt het user ID uit de claims.
            if (!User.TryGetUserId(out var userId))
                return Unauthorized("Invalid user.");

            // Voegt de set toe aan de WorkoutSessionExercise van de ingelogde gebruiker.
            await _workoutsService.AddWorkoutSetAsync(workoutSetDto, userId,cancellationToken);

            // Geeft een succesvolle response terug wanneer de set is toegevoegd.
            return Ok("Workout set added!");
        }

        // Haalt de workout summary op van een workout session.
        [HttpGet("summary/{workoutSessionId:guid}")]
        public async Task<IActionResult> GetWorkoutSummary(Guid workoutSessionId, CancellationToken cancellationToken)
        {
            // Controleert of de gebruiker is ingelogd en haalt het user ID op.
            if (!User.TryGetUserId(out var userId))
                return Unauthorized("Invalid user.");

            // Haalt de workout summary op via de workout service.
            var summary = await _workoutsService.GetWorkoutSummaryAsync(workoutSessionId, userId, cancellationToken);

            // Geeft de workout summary terug naar de frontend.
            return Ok(summary);
        }

        // Rondt de workout session af.
        [HttpPost("completeworkoutsession/{workoutSessionId:guid}")]
        public async Task<IActionResult> CompleteWorkoutSession(Guid workoutSessionId, CancellationToken cancellationToken)
        {
            // Controleert of de gebruiker is ingelogd en haalt het user ID uit de claims.
            if (!User.TryGetUserId(out var userId))
                return Unauthorized("Invalid user.");

            // Rondt de workout session af van de ingelogde gebruiker.
            await _workoutsService.CompleteWorkoutSessionAsync(workoutSessionId, userId, cancellationToken);

            // Geeft een succesvolle response terug wanneer de workout session is afgerond.
            return Ok("Workout session completed!");
        }

        // Verwijdert een workout session en de bijbehorende oefeningen en sets die aan deze oefeninge hangen.
        [HttpDelete("deleteworkoutsession/{workoutSessionId:guid}")]
        public async Task<IActionResult> DeleteWorkoutSession(Guid workoutSessionId, CancellationToken cancellationToken)
        {
            // Controleert of de gebruiker is ingelogd en haalt het user ID uit de claims.
            if (!User.TryGetUserId(out var userId))
                return Unauthorized("Invalid user.");

            // Verwijdert de workout session van de ingelogde gebruiker.
            // De gekoppelde oefeningen en sets worden via cascade delete verwijderd.
            await _workoutsService.DeleteWorkoutSessionAsync(workoutSessionId, userId, cancellationToken);

            // Geeft een succesvolle response terug wanneer de workout session is verwijderd.
            return Ok("Workout session deleted successfully!");
        }

        // Controleert of een workout session nog bestaat.
        [HttpGet("workoutsessionexists/{workoutSessionId:guid}")]
        public async Task<IActionResult> WorkoutSessionExists(Guid workoutSessionId, CancellationToken cancellationToken)
        {
            // Controleert of de gebruiker is ingelogd en haalt het user ID uit de claims.
            if (!User.TryGetUserId(out var userId))
                return Unauthorized("Invalid user.");

            // Controleert of de workout session bestaat en van de ingelogde gebruiker is.
            var exists = await _workoutsService.WorkoutSessionExistsAsync(workoutSessionId, userId, cancellationToken);

            // Geeft terug of de workout session bestaat.
            return Ok(exists);
        }
    }
}