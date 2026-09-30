using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Extensions;
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

        [HttpGet("getexercisesbymusclegroupid/{muscleGroupId:int:min(1)}")]
        public async Task<IActionResult> GetExercisesByMuscleGroupId(int muscleGroupId, CancellationToken cancellationToken)
        {
            // Haalt de oefeningen op die bij de opgegeven spiergroep horen.
            var exercises = await _exercisesService.GetExercisesByMuscleGroupIdAsync(muscleGroupId, cancellationToken);

            // Return Ok met de lijst met oefeningen
            return Ok(exercises);
        }

        [HttpGet("getexercise/{exerciseId:int}")]
        public async Task<IActionResult> GetExerciseById(int exerciseId, CancellationToken cancellationToken)
        {
            // Haalt de informatie op van een bepaalde oefening.
            var exercise = await _exercisesService.GetExerciseByIdAsync(exerciseId, cancellationToken);

            // Return Ok met de oefening
            return Ok(exercise);
        }

        // Haalt het persoonlijke record op van een oefening.
        [HttpGet("exercisepersonalrecord/{exerciseId:int}")]
        public async Task<IActionResult> GetExercisePersonalRecord(int exerciseId, CancellationToken cancellationToken)
        {
            // Controleert of de gebruiker is ingelogd en haalt het user ID uit de claims.
            if (!User.TryGetUserId(out var userId))
                return Unauthorized("Invalid user.");

            // Haalt het persoonlijke record van de oefening op voor de ingelogde gebruiker.
            var personalRecord = await _exercisesService.GetExercisePersonalRecordAsync(exerciseId, userId, cancellationToken);

            // Geeft het persoonlijke record terug naar de frontend.
            return Ok(personalRecord);
        }

        // Haalt de laatste uitgevoerde set op van een oefening.
        [HttpGet("exerciselastperformed/{exerciseId:int}")]
        public async Task<IActionResult> GetExerciseLastPerformed(int exerciseId, CancellationToken cancellationToken)
        {
            // Controleert of de gebruiker is ingelogd en haalt het user ID op.
            if (!User.TryGetUserId(out var userId))
                return Unauthorized("Invalid user.");

            // Haalt de laatste uitgevoerde set van de oefening op voor de ingelogde gebruiker.
            var lastPerformed = await _exercisesService.GetExerciseLastPerformedAsync(exerciseId, userId, cancellationToken);

            // Geeft de laatste uitgevoerde set terug naar de frontend.
            return Ok(lastPerformed);
        }
    }
}
