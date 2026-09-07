using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Extensions;
using WebAPI.Services;

namespace WebAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class PlanningController : ControllerBase
    {
        private readonly IPlanningService _planningService;

        public PlanningController(IPlanningService planningService)
        {
            _planningService = planningService;
        }


        [HttpGet("getmyweekplanning")]
        public async Task<IActionResult> GetMyWeekPlanning(CancellationToken cancellationToken)
        {
            // Controleert of de gebruiker is ingelogd en haalt het user ID uit de claims.
            if (!User.TryGetUserId(out var userId))
                return Unauthorized("Invalid user.");

            // Haalt de weekplanning op van de ingelogde gebruiker.
            var planning = await _planningService.GetMyWeekPlanningAsync(userId, cancellationToken);

            // Geeft de weekplanning terug.
            return Ok(planning);
        }


        [HttpDelete("deletedayplanning/{planningId:guid}")]
        public async Task<IActionResult> DeleteDayPlanning(Guid planningId, CancellationToken cancellationToken)
        {
            // Controleert of de gebruiker is ingelogd en haalt het user ID uit de claims.
            if (!User.TryGetUserId(out var userId))
                return Unauthorized("Invalid user.");

            // Verwijdert de dagplanning van de ingelogde gebruiker.
            await _planningService.DeleteDayPlanningAsync(planningId, userId, cancellationToken);

            // Geeft een succesvolle response terug wanneer de dagplanning is verwijderd.
            return Ok("Day planning deleted successfully!");
        }


        [HttpPost("adddayplanning/{workoutId:guid}/{day}")]
        public async Task<IActionResult> AddWorkoutToPlanning(Guid workoutId, DayOfWeek day, CancellationToken cancellationToken)
        {
            // Controleert of de gebruiker is ingelogd en haalt het user ID uit de claims.
            if (!User.TryGetUserId(out var userId))
                return Unauthorized("Invalid user.");

            // Voegt de workout toe aan de planning van de ingelogde gebruiker.
            await _planningService.AddWorkoutToPlanningAsync(workoutId, day, userId, cancellationToken);

            // Geeft een succesvolle response terug wanneer de workout is toegevoegd.
            return Ok("Workout added successfully!");
        }
    }
}