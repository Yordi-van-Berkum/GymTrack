using WebAPI.Services;

namespace WebAPI.BackgroundServices
{
    public class WorkoutSessionCleanupService : BackgroundService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public WorkoutSessionCleanupService(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                // Maakt een nieuwe scope aan zodat de WorkoutsService en DbContext gebruikt kunnen worden.
                using var scope = _serviceScopeFactory.CreateScope();

                // Haalt de WorkoutsService op uit de dependency injection container.
                var workoutsService = scope.ServiceProvider.GetRequiredService<IWorkoutsService>();

                // Verwijdert workout sessies die te lang niet actief zijn geweest.
                await workoutsService.DeleteInactiveWorkoutSessionsAsync(stoppingToken);

                // Wacht 20 minuten voordat opnieuw gecontroleerd wordt.
                await Task.Delay(TimeSpan.FromMinutes(20), stoppingToken);
            }
        }
    }
}