
using Microsoft.EntityFrameworkCore;
using WebAPI;
using WebAPI.Exceptions;
using WebAPI.Models.Planning;
using WebAPI.Models.Workout;
using WebAPI.Services;

namespace Tests
{
    public class PlanningServiceTests
    {
        private ApplicationDbContext CreateDbContext()
        {
            // Maak voor iedere test een eigen InMemory-database aan.
            // Hierdoor kunnen tests elkaar niet beïnvloeden.
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            // Geef de nieuwe databasecontext terug.
            return new ApplicationDbContext(options);
        }

        private async Task<Guid> AddWorkoutTestDataAsync(ApplicationDbContext context, Guid userId, string name = "Test Workout")
        {
            // Maak een uniek ID voor de workout.
            var workoutId = Guid.NewGuid();

            // Voeg een workout toe die bij de opgegeven gebruiker hoort.
            context.Workouts.Add(new Workout
            {
                Id = workoutId,
                UserId = userId,
                Name = name,
                Type = WorkoutType.Strength
            });

            // Sla de workout op.
            await context.SaveChangesAsync();

            // Geef het ID terug voor gebruik in de test.
            return workoutId;
        }

        private async Task<Guid> AddPlanningTestDataAsync(ApplicationDbContext context, Guid userId, Guid workoutId, DayOfWeek day)
        {
            // Maak een uniek ID voor de planning.
            var planningId = Guid.NewGuid();

            // Voeg een planning toe voor de opgegeven gebruiker en workout.
            context.Planning.Add(new WeekPlanning
            {
                Id = planningId,
                UserId = userId,
                WorkoutId = workoutId,
                Day = day
            });

            // Sla de planning op.
            await context.SaveChangesAsync();

            // Geef het ID van de planning terug.
            return planningId;
        }

        [Fact]
        public async Task GetMyWeekPlanningAsync_NoPlanning_ReturnsEmptyList()
        {
            // Maak een lege testdatabase aan.
            using var context = CreateDbContext();
            var planningService = new PlanningService(context);
            var userId = Guid.NewGuid();

            // Haal de weekplanning van de gebruiker op.
            var result = await planningService.GetMyWeekPlanningAsync(userId);

            // Controleer of de lijst leeg is.
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetMyWeekPlanningAsync_OnePlanning_ReturnsPlanning()
        {
            // Maak een nieuwe testdatabase aan.
            using var context = CreateDbContext();
            var planningService = new PlanningService(context);
            var userId = Guid.NewGuid();

            // Voeg een workout en een planning voor maandag toe.
            var workoutId = await AddWorkoutTestDataAsync(context, userId, "Push Workout");
            var planningId = await AddPlanningTestDataAsync(context, userId, workoutId, DayOfWeek.Monday);

            // Haal de weekplanning op.
            var result = await planningService.GetMyWeekPlanningAsync(userId);

            // Controleer of de juiste planning wordt teruggegeven.
            Assert.Single(result);
            Assert.Equal(planningId, result[0].Id);
            Assert.Equal(DayOfWeek.Monday, result[0].Day);
            Assert.Equal(workoutId, result[0].WorkoutId);
            Assert.Equal("Push Workout", result[0].WorkoutName);
        }

        [Fact]
        public async Task GetMyWeekPlanningAsync_MultipleDays_ReturnsAllPlanning()
        {
            // Maak een nieuwe testdatabase aan.
            using var context = CreateDbContext();
            var planningService = new PlanningService(context);
            var userId = Guid.NewGuid();

            // Voeg drie workouts toe.
            var mondayWorkoutId = await AddWorkoutTestDataAsync(context, userId, "Push");
            var wednesdayWorkoutId = await AddWorkoutTestDataAsync(context, userId, "Pull");
            var fridayWorkoutId = await AddWorkoutTestDataAsync(context, userId, "Legs");

            // Plan de workouts op drie verschillende dagen.
            await AddPlanningTestDataAsync(context, userId, mondayWorkoutId, DayOfWeek.Monday);
            await AddPlanningTestDataAsync(context, userId, wednesdayWorkoutId, DayOfWeek.Wednesday);
            await AddPlanningTestDataAsync(context, userId, fridayWorkoutId, DayOfWeek.Friday);

            // Haal de weekplanning op.
            var result = await planningService.GetMyWeekPlanningAsync(userId);

            // Controleer of alle drie de planningen aanwezig zijn.
            Assert.Equal(3, result.Count);
            Assert.Contains(result, p => p.WorkoutName == "Push");
            Assert.Contains(result, p => p.WorkoutName == "Pull");
            Assert.Contains(result, p => p.WorkoutName == "Legs");
        }

        [Fact]
        public async Task GetMyWeekPlanningAsync_MultipleDays_ReturnsCorrectDayOrder()
        {
            // Maak een nieuwe testdatabase aan.
            using var context = CreateDbContext();
            var planningService = new PlanningService(context);
            var userId = Guid.NewGuid();

            // Voeg drie workouts toe.
            var fridayWorkoutId = await AddWorkoutTestDataAsync(context, userId, "Friday Workout");
            var mondayWorkoutId = await AddWorkoutTestDataAsync(context, userId, "Monday Workout");
            var wednesdayWorkoutId = await AddWorkoutTestDataAsync(context, userId, "Wednesday Workout");

            // Voeg de dagen bewust in een verkeerde volgorde toe.
            await AddPlanningTestDataAsync(context, userId, fridayWorkoutId, DayOfWeek.Friday);
            await AddPlanningTestDataAsync(context, userId, mondayWorkoutId, DayOfWeek.Monday);
            await AddPlanningTestDataAsync(context, userId, wednesdayWorkoutId, DayOfWeek.Wednesday);

            // Haal de weekplanning op.
            var result = await planningService.GetMyWeekPlanningAsync(userId);

            // Controleer of de dagen oplopend gesorteerd zijn.
            Assert.Equal(3, result.Count);
            Assert.Equal(DayOfWeek.Monday, result[0].Day);
            Assert.Equal(DayOfWeek.Wednesday, result[1].Day);
            Assert.Equal(DayOfWeek.Friday, result[2].Day);
        }

        [Fact]
        public async Task GetMyWeekPlanningAsync_OtherUserHasPlanning_IgnoresOtherUser()
        {
            // Maak een nieuwe testdatabase aan.
            using var context = CreateDbContext();
            var planningService = new PlanningService(context);

            // Maak twee verschillende gebruikers aan.
            var userId = Guid.NewGuid();
            var otherUserId = Guid.NewGuid();

            // Voeg voor beide gebruikers een workout toe.
            var workoutId = await AddWorkoutTestDataAsync(context, userId, "My Workout");
            var otherWorkoutId = await AddWorkoutTestDataAsync(context, otherUserId, "Other Workout");

            // Voeg voor beide gebruikers een planning toe.
            await AddPlanningTestDataAsync(context, userId, workoutId, DayOfWeek.Monday);
            await AddPlanningTestDataAsync(context, otherUserId, otherWorkoutId, DayOfWeek.Tuesday);

            // Haal alleen de planning van de eerste gebruiker op.
            var result = await planningService.GetMyWeekPlanningAsync(userId);

            // Controleer of de andere gebruiker niet zichtbaar is.
            Assert.Single(result);
            Assert.Equal("My Workout", result[0].WorkoutName);
            Assert.DoesNotContain(result, p => p.WorkoutName == "Other Workout");
        }

        [Fact]
        public async Task GetMyWeekPlanningAsync_OnlyOtherUserHasPlanning_ReturnsEmptyList()
        {
            // Maak een nieuwe testdatabase aan.
            using var context = CreateDbContext();
            var planningService = new PlanningService(context);
            var userId = Guid.NewGuid();
            var otherUserId = Guid.NewGuid();

            // Voeg uitsluitend voor de andere gebruiker een planning toe.
            var workoutId = await AddWorkoutTestDataAsync(context, otherUserId, "Other Workout");
            await AddPlanningTestDataAsync(context, otherUserId, workoutId, DayOfWeek.Monday);

            // Haal de planning op van de gebruiker zonder planning.
            var result = await planningService.GetMyWeekPlanningAsync(userId);

            // Controleer of er geen planning wordt teruggegeven.
            Assert.Empty(result);
        }

        [Fact]
        public async Task AddWorkoutToPlanningAsync_ValidWorkout_AddsPlanning()
        {
            // Maak een nieuwe testdatabase aan.
            using var context = CreateDbContext();
            var planningService = new PlanningService(context);
            var userId = Guid.NewGuid();

            // Voeg een workout toe die bij de gebruiker hoort.
            var workoutId = await AddWorkoutTestDataAsync(context, userId, "Push Workout");

            // Plan de workout op maandag.
            await planningService.AddWorkoutToPlanningAsync(workoutId, DayOfWeek.Monday, userId);

            // Haal de opgeslagen planning uit de database.
            var planning = await context.Planning.SingleAsync();

            // Controleer of alle gegevens correct zijn opgeslagen.
            Assert.NotEqual(Guid.Empty, planning.Id);
            Assert.Equal(userId, planning.UserId);
            Assert.Equal(workoutId, planning.WorkoutId);
            Assert.Equal(DayOfWeek.Monday, planning.Day);
        }

        [Fact]
        public async Task AddWorkoutToPlanningAsync_MultipleDifferentDays_AddsAllPlanning()
        {
            // Maak een nieuwe testdatabase aan.
            using var context = CreateDbContext();
            var planningService = new PlanningService(context);
            var userId = Guid.NewGuid();

            // Voeg twee workouts toe.
            var pushWorkoutId = await AddWorkoutTestDataAsync(context, userId, "Push");
            var pullWorkoutId = await AddWorkoutTestDataAsync(context, userId, "Pull");

            // Plan de workouts op verschillende dagen.
            await planningService.AddWorkoutToPlanningAsync(pushWorkoutId, DayOfWeek.Monday, userId);
            await planningService.AddWorkoutToPlanningAsync(pullWorkoutId, DayOfWeek.Tuesday, userId);

            // Haal alle planningen op.
            var result = await context.Planning.ToListAsync();

            // Controleer of beide dagen zijn opgeslagen.
            Assert.Equal(2, result.Count);
            Assert.Contains(result, p => p.Day == DayOfWeek.Monday && p.WorkoutId == pushWorkoutId);
            Assert.Contains(result, p => p.Day == DayOfWeek.Tuesday && p.WorkoutId == pullWorkoutId);
        }

        [Fact]
        public async Task AddWorkoutToPlanningAsync_SameWorkoutDifferentDays_AddsBothDays()
        {
            // Maak een nieuwe testdatabase aan.
            using var context = CreateDbContext();
            var planningService = new PlanningService(context);
            var userId = Guid.NewGuid();

            // Voeg één workout toe.
            var workoutId = await AddWorkoutTestDataAsync(context, userId, "Full Body");

            // Plan dezelfde workout op twee verschillende dagen.
            await planningService.AddWorkoutToPlanningAsync(workoutId, DayOfWeek.Monday, userId);
            await planningService.AddWorkoutToPlanningAsync(workoutId, DayOfWeek.Friday, userId);

            // Haal alle planningen van de gebruiker op.
            var result = await context.Planning
                .Where(p => p.UserId == userId)
                .ToListAsync();

            // Controleer of dezelfde workout op beide dagen staat.
            Assert.Equal(2, result.Count);
            Assert.All(result, p => Assert.Equal(workoutId, p.WorkoutId));
            Assert.Contains(result, p => p.Day == DayOfWeek.Monday);
            Assert.Contains(result, p => p.Day == DayOfWeek.Friday);
        }

        [Fact]
        public async Task AddWorkoutToPlanningAsync_WorkoutDoesNotExist_ThrowsNotFoundException()
        {
            // Maak een lege testdatabase aan.
            using var context = CreateDbContext();
            var planningService = new PlanningService(context);
            var userId = Guid.NewGuid();

            // Probeer een niet-bestaande workout te plannen.
            await Assert.ThrowsAsync<NotFoundException>(() =>
                planningService.AddWorkoutToPlanningAsync(Guid.NewGuid(), DayOfWeek.Monday, userId));

            // Controleer of er niets is opgeslagen.
            Assert.Empty(await context.Planning.ToListAsync());
        }

        [Fact]
        public async Task AddWorkoutToPlanningAsync_OtherUsersWorkout_ThrowsNotFoundException()
        {
            // Maak een nieuwe testdatabase aan.
            using var context = CreateDbContext();
            var planningService = new PlanningService(context);
            var userId = Guid.NewGuid();
            var otherUserId = Guid.NewGuid();

            // Voeg een workout toe die bij een andere gebruiker hoort.
            var workoutId = await AddWorkoutTestDataAsync(context, otherUserId, "Private Workout");

            // Probeer de workout van de andere gebruiker te plannen.
            await Assert.ThrowsAsync<NotFoundException>(() =>
                planningService.AddWorkoutToPlanningAsync(workoutId, DayOfWeek.Monday, userId));

            // Controleer of er geen planning is aangemaakt.
            Assert.Empty(await context.Planning.ToListAsync());
        }

        [Fact]
        public async Task AddWorkoutToPlanningAsync_DayAlreadyHasWorkout_ThrowsConflictException()
        {
            // Maak een nieuwe testdatabase aan.
            using var context = CreateDbContext();
            var planningService = new PlanningService(context);
            var userId = Guid.NewGuid();

            // Voeg twee workouts toe.
            var firstWorkoutId = await AddWorkoutTestDataAsync(context, userId, "Push");
            var secondWorkoutId = await AddWorkoutTestDataAsync(context, userId, "Pull");

            // Plan de eerste workout op maandag.
            await planningService.AddWorkoutToPlanningAsync(firstWorkoutId, DayOfWeek.Monday, userId);

            // Probeer een tweede workout op dezelfde dag te plannen.
            await Assert.ThrowsAsync<ConflictException>(() =>
                planningService.AddWorkoutToPlanningAsync(secondWorkoutId, DayOfWeek.Monday, userId));

            // Controleer of de oorspronkelijke planning behouden blijft.
            var planning = await context.Planning.SingleAsync();
            Assert.Equal(firstWorkoutId, planning.WorkoutId);
            Assert.Equal(DayOfWeek.Monday, planning.Day);
        }

        [Fact]
        public async Task AddWorkoutToPlanningAsync_SameWorkoutSameDay_ThrowsConflictException()
        {
            // Maak een nieuwe testdatabase aan.
            using var context = CreateDbContext();
            var planningService = new PlanningService(context);
            var userId = Guid.NewGuid();

            // Voeg een workout toe.
            var workoutId = await AddWorkoutTestDataAsync(context, userId, "Push");

            // Plan de workout op woensdag.
            await planningService.AddWorkoutToPlanningAsync(workoutId, DayOfWeek.Wednesday, userId);

            // Probeer exact dezelfde workout opnieuw op woensdag te plannen.
            await Assert.ThrowsAsync<ConflictException>(() =>
                planningService.AddWorkoutToPlanningAsync(workoutId, DayOfWeek.Wednesday, userId));

            // Controleer of er maar één planning bestaat.
            Assert.Equal(1, await context.Planning.CountAsync());
        }

        [Fact]
        public async Task AddWorkoutToPlanningAsync_OtherUserHasSameDay_AllowsPlanning()
        {
            // Maak een nieuwe testdatabase aan.
            using var context = CreateDbContext();
            var planningService = new PlanningService(context);
            var userId = Guid.NewGuid();
            var otherUserId = Guid.NewGuid();

            // Voeg voor beide gebruikers een workout toe.
            var workoutId = await AddWorkoutTestDataAsync(context, userId, "My Push");
            var otherWorkoutId = await AddWorkoutTestDataAsync(context, otherUserId, "Other Push");

            // De andere gebruiker heeft maandag al ingepland.
            await AddPlanningTestDataAsync(context, otherUserId, otherWorkoutId, DayOfWeek.Monday);

            // Plan voor de eerste gebruiker ook een workout op maandag.
            await planningService.AddWorkoutToPlanningAsync(workoutId, DayOfWeek.Monday, userId);

            // Controleer of beide gebruikers hun eigen planning hebben.
            var result = await context.Planning.ToListAsync();
            Assert.Equal(2, result.Count);
            Assert.Contains(result, p => p.UserId == userId && p.WorkoutId == workoutId && p.Day == DayOfWeek.Monday);
            Assert.Contains(result, p => p.UserId == otherUserId && p.WorkoutId == otherWorkoutId && p.Day == DayOfWeek.Monday);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(7)]
        [InlineData(100)]
        public async Task AddWorkoutToPlanningAsync_InvalidDay_ThrowsInvalidOperationException(int invalidDay)
        {
            // Maak een nieuwe testdatabase aan.
            using var context = CreateDbContext();
            var planningService = new PlanningService(context);
            var userId = Guid.NewGuid();

            // Voeg een geldige workout toe.
            var workoutId = await AddWorkoutTestDataAsync(context, userId, "Push");

            // Probeer een ongeldige dag te gebruiken.
            await Assert.ThrowsAsync<InvalidOperationException>(() => planningService.AddWorkoutToPlanningAsync(workoutId, (DayOfWeek)invalidDay, userId));

            // Controleer of er niets is opgeslagen.
            Assert.Empty(await context.Planning.ToListAsync());
        }

        [Fact]
        public async Task DeleteDayPlanningAsync_PlanningExists_RemovesPlanning()
        {
            // Maak een nieuwe testdatabase aan.
            using var context = CreateDbContext();
            var planningService = new PlanningService(context);
            var userId = Guid.NewGuid();

            // Voeg een workout en planning toe.
            var workoutId = await AddWorkoutTestDataAsync(context, userId, "Push");
            var planningId = await AddPlanningTestDataAsync(context, userId, workoutId, DayOfWeek.Monday);

            // Verwijder de planning.
            await planningService.DeleteDayPlanningAsync(planningId, userId);

            // Controleer of de planning is verwijderd.
            Assert.Empty(await context.Planning.ToListAsync());
        }

        [Fact]
        public async Task DeleteDayPlanningAsync_PlanningDoesNotExist_ThrowsNotFoundException()
        {
            // Maak een lege testdatabase aan.
            using var context = CreateDbContext();
            var planningService = new PlanningService(context);
            var userId = Guid.NewGuid();

            // Probeer een niet-bestaande planning te verwijderen.
            await Assert.ThrowsAsync<NotFoundException>(() => planningService.DeleteDayPlanningAsync(Guid.NewGuid(), userId));

            // Controleer of de database leeg blijft.
            Assert.Empty(await context.Planning.ToListAsync());
        }

        [Fact]
        public async Task DeleteDayPlanningAsync_OtherUsersPlanning_ThrowsNotFoundException()
        {
            // Maak een nieuwe testdatabase aan.
            using var context = CreateDbContext();
            var planningService = new PlanningService(context);
            var userId = Guid.NewGuid();
            var otherUserId = Guid.NewGuid();

            // Voeg een planning toe die bij een andere gebruiker hoort.
            var workoutId = await AddWorkoutTestDataAsync(context, otherUserId, "Private Workout");
            var planningId = await AddPlanningTestDataAsync(context, otherUserId, workoutId, DayOfWeek.Monday);

            // Probeer de planning van de andere gebruiker te verwijderen.
            await Assert.ThrowsAsync<NotFoundException>(() => planningService.DeleteDayPlanningAsync(planningId, userId));

            // Controleer of de planning van de andere gebruiker blijft bestaan.
            Assert.True(await context.Planning.AnyAsync(p => p.Id == planningId && p.UserId == otherUserId));
        }

        [Fact]
        public async Task DeleteDayPlanningAsync_MultiplePlanning_RemovesOnlyRequestedPlanning()
        {
            // Maak een nieuwe testdatabase aan.
            using var context = CreateDbContext();
            var planningService = new PlanningService(context);
            var userId = Guid.NewGuid();

            // Voeg twee workouts toe.
            var mondayWorkoutId = await AddWorkoutTestDataAsync(context, userId, "Push");
            var tuesdayWorkoutId = await AddWorkoutTestDataAsync(context, userId, "Pull");

            // Plan beide workouts op verschillende dagen.
            var mondayPlanningId = await AddPlanningTestDataAsync(context, userId, mondayWorkoutId, DayOfWeek.Monday);
            var tuesdayPlanningId = await AddPlanningTestDataAsync(context, userId, tuesdayWorkoutId, DayOfWeek.Tuesday);

            // Verwijder alleen de planning van maandag.
            await planningService.DeleteDayPlanningAsync(mondayPlanningId, userId);

            // Controleer of alleen dinsdag overblijft.
            var remainingPlanning = await context.Planning.SingleAsync();
            Assert.Equal(tuesdayPlanningId, remainingPlanning.Id);
            Assert.Equal(DayOfWeek.Tuesday, remainingPlanning.Day);
            Assert.Equal(tuesdayWorkoutId, remainingPlanning.WorkoutId);
        }

        [Fact]
        public async Task DeleteDayPlanningAsync_ExistingPlanning_DoesNotDeleteWorkout()
        {
            // Maak een nieuwe testdatabase aan.
            using var context = CreateDbContext();
            var planningService = new PlanningService(context);
            var userId = Guid.NewGuid();

            // Voeg een workout en planning toe.
            var workoutId = await AddWorkoutTestDataAsync(context, userId, "Push");
            var planningId = await AddPlanningTestDataAsync(context, userId, workoutId, DayOfWeek.Monday);

            // Verwijder de planning.
            await planningService.DeleteDayPlanningAsync(planningId, userId);

            // Controleer of de planning verdwenen is.
            Assert.False(await context.Planning.AnyAsync(p => p.Id == planningId));

            // Controleer of de workout zelf nog bestaat.
            Assert.True(await context.Workouts.AnyAsync(w => w.Id == workoutId && w.UserId == userId));
        }

        [Fact]
        public async Task DeleteDayPlanningAsync_OtherUserHasPlanning_KeepsOtherUsersPlanning()
        {
            // Maak een nieuwe testdatabase aan.
            using var context = CreateDbContext();
            var planningService = new PlanningService(context);
            var userId = Guid.NewGuid();
            var otherUserId = Guid.NewGuid();

            // Voeg voor beide gebruikers een workout toe.
            var workoutId = await AddWorkoutTestDataAsync(context, userId, "My Workout");
            var otherWorkoutId = await AddWorkoutTestDataAsync(context, otherUserId, "Other Workout");

            // Voeg voor beide gebruikers een planning toe.
            var planningId = await AddPlanningTestDataAsync(context, userId, workoutId, DayOfWeek.Monday);
            var otherPlanningId = await AddPlanningTestDataAsync(context, otherUserId, otherWorkoutId, DayOfWeek.Monday);

            // Verwijder alleen de planning van de eerste gebruiker.
            await planningService.DeleteDayPlanningAsync(planningId, userId);

            // Controleer of de planning van de andere gebruiker blijft bestaan.
            var remainingPlanning = await context.Planning.SingleAsync();
            Assert.Equal(otherPlanningId, remainingPlanning.Id);
            Assert.Equal(otherUserId, remainingPlanning.UserId);
        }

        // ============================================================
        // GET TODAYS WORKOUT
        // ============================================================

        [Fact]
        public async Task GetTodaysWorkoutAsync_NoPlanning_ReturnsNull()
        {
            // Maak een lege testdatabase aan.
            using var context = CreateDbContext();
            var planningService = new PlanningService(context);
            var userId = Guid.NewGuid();

            // Haal de workout van vandaag op.
            var result = await planningService.GetTodaysWorkoutAsync(userId);

            // Controleer of er geen workout wordt teruggegeven.
            Assert.Null(result);
        }

        [Fact]
        public async Task GetTodaysWorkoutAsync_TodayHasWorkout_ReturnsCorrectWorkout()
        {
            // Maak een nieuwe testdatabase aan.
            using var context = CreateDbContext();
            var planningService = new PlanningService(context);
            var userId = Guid.NewGuid();

            // Bepaal welke dag het vandaag is.
            var today = DateTime.Today.DayOfWeek;

            // Voeg een workout toe voor vandaag.
            var workoutId = await AddWorkoutTestDataAsync(context, userId, "Today's Workout");
            var planningId = await AddPlanningTestDataAsync(context, userId, workoutId, today);

            // Haal de workout van vandaag op.
            var result = await planningService.GetTodaysWorkoutAsync(userId);

            // Controleer of de juiste workout wordt teruggegeven.
            Assert.NotNull(result);
            Assert.Equal(planningId, result.Id);
            Assert.Equal(today, result.Day);
            Assert.Equal(workoutId, result.WorkoutId);
            Assert.Equal("Today's Workout", result.WorkoutName);
        }

        [Fact]
        public async Task GetTodaysWorkoutAsync_OnlyOtherDayPlanned_ReturnsNull()
        {
            // Maak een nieuwe testdatabase aan.
            using var context = CreateDbContext();
            var planningService = new PlanningService(context);
            var userId = Guid.NewGuid();

            // Bepaal vandaag en kies een andere dag.
            var today = DateTime.Today.DayOfWeek;
            var otherDay = (DayOfWeek)(((int)today + 1) % 7);

            // Voeg een workout toe voor de andere dag.
            var workoutId = await AddWorkoutTestDataAsync(context, userId, "Tomorrow Workout");
            await AddPlanningTestDataAsync(context, userId, workoutId, otherDay);

            // Haal de workout van vandaag op.
            var result = await planningService.GetTodaysWorkoutAsync(userId);

            // Controleer of de workout van de andere dag niet terugkomt.
            Assert.Null(result);
        }

        [Fact]
        public async Task GetTodaysWorkoutAsync_MultipleDays_ReturnsOnlyToday()
        {
            // Maak een nieuwe testdatabase aan.
            using var context = CreateDbContext();
            var planningService = new PlanningService(context);
            var userId = Guid.NewGuid();

            // Bepaal vandaag en een andere dag.
            var today = DateTime.Today.DayOfWeek;
            var otherDay = (DayOfWeek)(((int)today + 1) % 7);

            // Voeg twee workouts toe.
            var todayWorkoutId = await AddWorkoutTestDataAsync(context, userId, "Today");
            var otherWorkoutId = await AddWorkoutTestDataAsync(context, userId, "Other Day");

            // Plan beide workouts op hun eigen dag.
            await AddPlanningTestDataAsync(context, userId, otherWorkoutId, otherDay);
            await AddPlanningTestDataAsync(context, userId, todayWorkoutId, today);

            // Haal de workout van vandaag op.
            var result = await planningService.GetTodaysWorkoutAsync(userId);

            // Controleer of alleen de workout van vandaag terugkomt.
            Assert.NotNull(result);
            Assert.Equal(todayWorkoutId, result.WorkoutId);
            Assert.Equal("Today", result.WorkoutName);
        }

        [Fact]
        public async Task GetTodaysWorkoutAsync_OtherUserHasTodaysWorkout_IgnoresOtherUser()
        {
            // Maak een nieuwe testdatabase aan.
            using var context = CreateDbContext();
            var planningService = new PlanningService(context);
            var userId = Guid.NewGuid();
            var otherUserId = Guid.NewGuid();

            // Bepaal welke dag het vandaag is.
            var today = DateTime.Today.DayOfWeek;

            // Voeg voor beide gebruikers een workout toe.
            var workoutId = await AddWorkoutTestDataAsync(context, userId, "My Workout");
            var otherWorkoutId = await AddWorkoutTestDataAsync(context, otherUserId, "Other Workout");

            // Plan voor beide gebruikers een workout op vandaag.
            await AddPlanningTestDataAsync(context, userId, workoutId, today);
            await AddPlanningTestDataAsync(context, otherUserId, otherWorkoutId, today);

            // Haal alleen de workout van de eerste gebruiker op.
            var result = await planningService.GetTodaysWorkoutAsync(userId);

            // Controleer of de andere gebruiker niet wordt teruggegeven.
            Assert.NotNull(result);
            Assert.Equal(workoutId, result.WorkoutId);
            Assert.Equal("My Workout", result.WorkoutName);
        }

        [Fact]
        public async Task GetTodaysWorkoutAsync_OnlyOtherUserHasTodaysWorkout_ReturnsNull()
        {
            // Maak een nieuwe testdatabase aan.
            using var context = CreateDbContext();
            var planningService = new PlanningService(context);
            var userId = Guid.NewGuid();
            var otherUserId = Guid.NewGuid();

            // Bepaal welke dag het vandaag is.
            var today = DateTime.Today.DayOfWeek;

            // Voeg alleen voor de andere gebruiker een workout toe.
            var workoutId = await AddWorkoutTestDataAsync(context, otherUserId, "Private Workout");
            await AddPlanningTestDataAsync(context, otherUserId, workoutId, today);

            // Haal de workout van vandaag op voor de eerste gebruiker.
            var result = await planningService.GetTodaysWorkoutAsync(userId);

            // Controleer of de planning van de andere gebruiker verborgen blijft.
            Assert.Null(result);
        }
    }
}
