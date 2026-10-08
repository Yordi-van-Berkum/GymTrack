
using Microsoft.EntityFrameworkCore;
using WebAPI;
using WebAPI.Models.Exercise;
using WebAPI.Models.Exercises;
using WebAPI.Models.Workout;
using WebAPI.Services;

namespace Tests
{
    public class ExercisesServiceTests
    {
        private ApplicationDbContext CreateDbContext()
        {
            // Maak voor elke test een eigen nepdatabase aan.
            // Zo hebben de tests geen invloed op elkaar.
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            // Geef de nepdatabase terug.
            return new ApplicationDbContext(options);
        }

        private async Task AddWorkoutSetTestDataAsync(ApplicationDbContext context, Guid userId, int exerciseId, decimal weight, int reps, int setNumber = 1, bool isCompleted = true, DateTime? startedAt = null)
        {
            // Maak de IDs voor de testgegevens aan.
            var workoutId = Guid.NewGuid();
            var sessionId = Guid.NewGuid();
            var sessionExerciseId = Guid.NewGuid();

            // Maak een workout voor de gebruiker aan.
            context.Workouts.Add(new Workout
            {
                Id = workoutId,
                UserId = userId,
                Name = "Test Workout",
                Type = WorkoutType.Strength
            });

            // Maak een workout session aan.
            context.WorkoutSessions.Add(new WorkoutSession
            {
                Id = sessionId,
                WorkoutId = workoutId,
                StartedAt = startedAt ?? DateTime.UtcNow,
                LastActivityAt = DateTime.UtcNow,
                IsCompleted = isCompleted
            });

            // Voeg de oefening toe als deze nog niet bestaat.
            if (!await context.Exercises.AnyAsync(e => e.Id == exerciseId) && !context.Exercises.Local.Any(e => e.Id == exerciseId))
            {
                context.Exercises.Add(new Exercise
                {
                    Id = exerciseId,
                    Name = "Test Exercise " + exerciseId,
                    Description = "Test oefening"
                });
            }

            // Koppel de oefening aan de workout session.
            context.WorkoutSessionExercises.Add(new WorkoutSessionExercise
            {
                Id = sessionExerciseId,
                WorkoutSessionId = sessionId,
                ExerciseId = exerciseId
            });

            // Voeg een set met het opgegeven gewicht en herhalingen toe.
            context.WorkoutSets.Add(new WorkoutSet
            {
                Id = Guid.NewGuid(),
                WorkoutSessionExerciseId = sessionExerciseId,
                SetNumber = setNumber,
                Weight = weight,
                Reps = reps
            });

            // Sla alle testgegevens op.
            await context.SaveChangesAsync();
        }

        [Fact]
        public async Task GetMuscleGroupsAsync_MuscleGroupsExist_ReturnsAllMuscleGroups()
        {
            // Maak een nieuwe test database aan.
            using var context = CreateDbContext();
            var exercisesService = new ExercisesService(context);

            // Voeg twee spiergroepen toe.
            context.MuscleGroups.AddRange(
                new MuscleGroup
                {
                    Id = 1,
                    Name = "Chest",
                    Description = "Borstspieren",
                    SortOrder = 1
                },
                new MuscleGroup
                {
                    Id = 2,
                    Name = "Back",
                    Description = "Rugspieren",
                    SortOrder = 2
                });

            // Sla de spiergroepen op.
            await context.SaveChangesAsync();

            // Haal alle spiergroepen op.
            var result = await exercisesService.GetMuscleGroupsAsync();

            // Controleer of beide spiergroepen terugkomen.
            Assert.Equal(2, result.Count);
            Assert.Contains(result, mg => mg.Name == "Chest");
            Assert.Contains(result, mg => mg.Name == "Back");
        }

        [Fact]
        public async Task GetMuscleGroupsAsync_NoMuscleGroups_ReturnsEmptyList()
        {
            // Maak een lege test database aan.
            using var context = CreateDbContext();
            var exercisesService = new ExercisesService(context);

            // Haal de spiergroepen op.
            var result = await exercisesService.GetMuscleGroupsAsync();

            // Controleer of de lijst leeg is.
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetMuscleGroupsAsync_MuscleGroupsExist_ReturnsCorrectSortOrder()
        {
            // Maak een nieuwe test database aan.
            using var context = CreateDbContext();
            var exercisesService = new ExercisesService(context);

            // Voeg spiergroepen in een andere volgorde toe.
            context.MuscleGroups.AddRange(
                new MuscleGroup
                {
                    Id = 1,
                    Name = "Back",
                    Description = "Rugspieren",
                    SortOrder = 2
                },
                new MuscleGroup
                {
                    Id = 2,
                    Name = "Chest",
                    Description = "Borstspieren",
                    SortOrder = 1
                });

            // Sla de gegevens op.
            await context.SaveChangesAsync();

            // Haal de spiergroepen op.
            var result = await exercisesService.GetMuscleGroupsAsync();

            // Controleer of SortOrder wordt gebruikt.
            Assert.Equal("Chest", result[0].Name);
            Assert.Equal("Back", result[1].Name);
        }

        [Fact]
        public async Task GetMuscleGroupsAsync_MuscleGroupHasExercises_ReturnsCorrectExerciseCount()
        {
            // Maak een nieuwe test database aan.
            using var context = CreateDbContext();
            var exercisesService = new ExercisesService(context);

            // Voeg een spiergroep toe.
            context.MuscleGroups.Add(new MuscleGroup
            {
                Id = 1,
                Name = "Chest",
                Description = "Borstspieren",
                SortOrder = 1
            });

            // Voeg twee oefeningen toe.
            context.Exercises.AddRange(
                new Exercise
                {
                    Id = 1,
                    Name = "Bench Press",
                    Description = "Borst oefening"
                },
                new Exercise
                {
                    Id = 2,
                    Name = "Chest Fly",
                    Description = "Borst oefening"
                });

            // Koppel beide oefeningen aan de spiergroep.
            context.ExerciseMuscleGroups.AddRange(
                new ExerciseMuscleGroup
                {
                    ExerciseId = 1,
                    MuscleGroupId = 1
                },
                new ExerciseMuscleGroup
                {
                    ExerciseId = 2,
                    MuscleGroupId = 1
                });

            // Sla de gegevens op.
            await context.SaveChangesAsync();

            // Haal de spiergroepen op.
            var result = await exercisesService.GetMuscleGroupsAsync();

            // Controleer of de spiergroep twee oefeningen heeft.
            Assert.Single(result);
            Assert.Equal(2, result[0].ExerciseCount);
        }

        [Fact]
        public async Task GetMuscleGroupsAsync_MuscleGroupHasNoExercises_ReturnsZeroExerciseCount()
        {
            // Maak een nieuwe test database aan.
            using var context = CreateDbContext();
            var exercisesService = new ExercisesService(context);

            // Voeg een spiergroep zonder oefeningen toe.
            context.MuscleGroups.Add(new MuscleGroup
            {
                Id = 1,
                Name = "Chest",
                Description = "Borstspieren",
                SortOrder = 1
            });

            // Sla de spiergroep op.
            await context.SaveChangesAsync();

            // Haal de spiergroepen op.
            var result = await exercisesService.GetMuscleGroupsAsync();

            // Controleer of het aantal oefeningen nul is.
            Assert.Single(result);
            Assert.Equal(0, result[0].ExerciseCount);
        }

        [Fact]
        public async Task GetExercisesByMuscleGroupIdAsync_MuscleGroupHasExercises_ReturnsExercises()
        {
            // Maak een nieuwe test database aan.
            using var context = CreateDbContext();
            var exercisesService = new ExercisesService(context);

            // Voeg een spiergroep toe.
            context.MuscleGroups.Add(new MuscleGroup
            {
                Id = 1,
                Name = "Chest",
                Description = "Borstspieren",
                SortOrder = 1
            });

            // Voeg twee oefeningen toe.
            context.Exercises.AddRange(
                new Exercise
                {
                    Id = 1,
                    Name = "Bench Press",
                    Description = "Borst oefening"
                },
                new Exercise
                {
                    Id = 2,
                    Name = "Chest Fly",
                    Description = "Borst oefening"
                });

            // Koppel beide oefeningen aan de spiergroep.
            context.ExerciseMuscleGroups.AddRange(
                new ExerciseMuscleGroup
                {
                    ExerciseId = 1,
                    MuscleGroupId = 1
                },
                new ExerciseMuscleGroup
                {
                    ExerciseId = 2,
                    MuscleGroupId = 1
                });

            // Sla de gegevens op.
            await context.SaveChangesAsync();

            // Haal de oefeningen op.
            var result = await exercisesService.GetExercisesByMuscleGroupIdAsync(1);

            // Controleer of beide oefeningen terugkomen.
            Assert.Equal(2, result.Count);
            Assert.Contains(result, e => e.Name == "Bench Press");
            Assert.Contains(result, e => e.Name == "Chest Fly");
        }

        [Fact]
        public async Task GetExercisesByMuscleGroupIdAsync_NoExercises_ReturnsEmptyList()
        {
            // Maak een lege test database aan.
            using var context = CreateDbContext();
            var exercisesService = new ExercisesService(context);

            // Haal oefeningen op van een spiergroep zonder oefeningen.
            var result = await exercisesService.GetExercisesByMuscleGroupIdAsync(1);

            // Controleer of de lijst leeg is.
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetExercisesByMuscleGroupIdAsync_ExercisesExist_ReturnsExercisesOrderedByName()
        {
            // Maak een nieuwe test database aan.
            using var context = CreateDbContext();
            var exercisesService = new ExercisesService(context);

            // Voeg een spiergroep toe.
            context.MuscleGroups.Add(new MuscleGroup
            {
                Id = 1,
                Name = "Chest",
                Description = "Borstspieren",
                SortOrder = 1
            });

            // Voeg oefeningen in een andere alfabetische volgorde toe.
            context.Exercises.AddRange(
                new Exercise
                {
                    Id = 1,
                    Name = "Chest Fly",
                    Description = "Borst oefening"
                },
                new Exercise
                {
                    Id = 2,
                    Name = "Bench Press",
                    Description = "Borst oefening"
                });

            // Koppel de oefeningen aan de spiergroep.
            context.ExerciseMuscleGroups.AddRange(
                new ExerciseMuscleGroup
                {
                    ExerciseId = 1,
                    MuscleGroupId = 1
                },
                new ExerciseMuscleGroup
                {
                    ExerciseId = 2,
                    MuscleGroupId = 1
                });

            // Sla de gegevens op.
            await context.SaveChangesAsync();

            // Haal de oefeningen op.
            var result = await exercisesService.GetExercisesByMuscleGroupIdAsync(1);

            // Controleer of de oefeningen alfabetisch staan.
            Assert.Equal("Bench Press", result[0].Name);
            Assert.Equal("Chest Fly", result[1].Name);
        }

        [Fact]
        public async Task GetExercisesByMuscleGroupIdAsync_DifferentMuscleGroups_ReturnsOnlyMatchingExercises()
        {
            // Maak een nieuwe test database aan.
            using var context = CreateDbContext();
            var exercisesService = new ExercisesService(context);

            // Voeg twee spiergroepen toe.
            context.MuscleGroups.AddRange(
                new MuscleGroup
                {
                    Id = 1,
                    Name = "Chest",
                    Description = "Borstspieren",
                    SortOrder = 1
                },
                new MuscleGroup
                {
                    Id = 2,
                    Name = "Back",
                    Description = "Rugspieren",
                    SortOrder = 2
                });

            // Voeg een borstoefening en een rugoefening toe.
            context.Exercises.AddRange(
                new Exercise
                {
                    Id = 1,
                    Name = "Bench Press",
                    Description = "Borst oefening"
                },
                new Exercise
                {
                    Id = 2,
                    Name = "Pull Up",
                    Description = "Rug oefening"
                });

            // Koppel iedere oefening aan de juiste spiergroep.
            context.ExerciseMuscleGroups.AddRange(
                new ExerciseMuscleGroup
                {
                    ExerciseId = 1,
                    MuscleGroupId = 1
                },
                new ExerciseMuscleGroup
                {
                    ExerciseId = 2,
                    MuscleGroupId = 2
                });

            // Sla de gegevens op.
            await context.SaveChangesAsync();

            // Haal alleen de borstoefeningen op.
            var result = await exercisesService.GetExercisesByMuscleGroupIdAsync(1);

            // Controleer of alleen Bench Press terugkomt.
            Assert.Single(result);
            Assert.Equal("Bench Press", result[0].Name);
            Assert.DoesNotContain(result, e => e.Name == "Pull Up");
        }

        [Fact]
        public async Task GetExerciseByIdAsync_ExerciseExists_ReturnsExercise()
        {
            // Maak een nieuwe test database aan.
            using var context = CreateDbContext();
            var exercisesService = new ExercisesService(context);

            // Voeg een oefening toe.
            context.Exercises.Add(new Exercise
            {
                Id = 1,
                Name = "Bench Press",
                Description = "Borst oefening"
            });

            // Sla de oefening op.
            await context.SaveChangesAsync();

            // Haal de oefening op.
            var result = await exercisesService.GetExerciseByIdAsync(1);

            // Controleer of de juiste gegevens terugkomen.
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("Bench Press", result.Name);
            Assert.Equal("Borst oefening", result.Description);
        }

        [Fact]
        public async Task GetExerciseByIdAsync_ExerciseDoesNotExist_ReturnsNull()
        {
            // Maak een lege test database aan.
            using var context = CreateDbContext();
            var exercisesService = new ExercisesService(context);

            // Probeer een oefening op te halen die niet bestaat.
            var result = await exercisesService.GetExerciseByIdAsync(999);

            // Controleer of null wordt teruggegeven.
            Assert.Null(result);
        }

        [Fact]
        public async Task GetExerciseByIdAsync_MultipleExercises_ReturnsCorrectExercise()
        {
            // Maak een nieuwe test database aan.
            using var context = CreateDbContext();
            var exercisesService = new ExercisesService(context);

            // Voeg twee verschillende oefeningen toe.
            context.Exercises.AddRange(
                new Exercise
                {
                    Id = 1,
                    Name = "Bench Press",
                    Description = "Borst oefening"
                },
                new Exercise
                {
                    Id = 2,
                    Name = "Pull Up",
                    Description = "Rug oefening"
                });

            // Sla de oefeningen op.
            await context.SaveChangesAsync();

            // Haal alleen oefening twee op.
            var result = await exercisesService.GetExerciseByIdAsync(2);

            // Controleer of de juiste oefening terugkomt.
            Assert.NotNull(result);
            Assert.Equal(2, result.Id);
            Assert.Equal("Pull Up", result.Name);
        }

        [Fact]
        public async Task GetExercisePersonalRecordAsync_NoCompletedSessions_ReturnsZero()
        {
            // Maak een lege test database aan.
            using var context = CreateDbContext();
            var exercisesService = new ExercisesService(context);

            // Haal het record op van een gebruiker zonder trainingen.
            var result = await exercisesService.GetExercisePersonalRecordAsync(
                1, Guid.NewGuid());

            // Controleer of het record nul is.
            Assert.NotNull(result);
            Assert.Equal(0, result.Weight);
            Assert.Equal(0, result.Reps);
        }

        [Fact]
        public async Task GetExercisePersonalRecordAsync_CompletedSessions_ReturnsHighestWeight()
        {
            // Maak een nieuwe test database aan.
            using var context = CreateDbContext();
            var exercisesService = new ExercisesService(context);
            var userId = Guid.NewGuid();

            // Voeg twee afgeronde trainingen met verschillende gewichten toe.
            await AddWorkoutSetTestDataAsync(context, userId, 1, 80, 10);
            await AddWorkoutSetTestDataAsync(context, userId, 1, 100, 5);

            // Haal het persoonlijke record op.
            var result = await exercisesService.GetExercisePersonalRecordAsync(1, userId);

            // Controleer of het hoogste gewicht terugkomt.
            Assert.NotNull(result);
            Assert.Equal(100, result.Weight);
            Assert.Equal(5, result.Reps);
        }

        [Fact]
        public async Task GetExercisePersonalRecordAsync_SameWeight_ReturnsHighestReps()
        {
            // Maak een nieuwe test database aan.
            using var context = CreateDbContext();
            var exercisesService = new ExercisesService(context);
            var userId = Guid.NewGuid();

            // Voeg twee sets met hetzelfde gewicht toe.
            await AddWorkoutSetTestDataAsync(context, userId, 1, 100, 5);
            await AddWorkoutSetTestDataAsync(context, userId, 1, 100, 8);

            // Haal het persoonlijke record op.
            var result = await exercisesService.GetExercisePersonalRecordAsync(1, userId);

            // Controleer of de meeste herhalingen gekozen worden.
            Assert.NotNull(result);
            Assert.Equal(100, result.Weight);
            Assert.Equal(8, result.Reps);
        }

        [Fact]
        public async Task GetExercisePersonalRecordAsync_IncompleteSession_IgnoresSession()
        {
            // Maak een nieuwe test database aan.
            using var context = CreateDbContext();
            var exercisesService = new ExercisesService(context);
            var userId = Guid.NewGuid();

            // Voeg een onafgeronde training toe.
            await AddWorkoutSetTestDataAsync(
                context, userId, 1, 120, 5, isCompleted: false);

            // Haal het persoonlijke record op.
            var result = await exercisesService.GetExercisePersonalRecordAsync(1, userId);

            // Een onafgeronde training mag niet meetellen.
            Assert.NotNull(result);
            Assert.Equal(0, result.Weight);
            Assert.Equal(0, result.Reps);
        }

        [Fact]
        public async Task GetExercisePersonalRecordAsync_OtherUserHasHigherWeight_IgnoresOtherUser()
        {
            // Maak een nieuwe test database aan.
            using var context = CreateDbContext();
            var exercisesService = new ExercisesService(context);

            // Maak twee gebruikers aan.
            var userId = Guid.NewGuid();
            var otherUserId = Guid.NewGuid();

            // Voeg voor beide gebruikers een training toe.
            await AddWorkoutSetTestDataAsync(context, userId, 1, 80, 10);
            await AddWorkoutSetTestDataAsync(context, otherUserId, 1, 150, 5);

            // Haal alleen het record van de eerste gebruiker op.
            var result = await exercisesService.GetExercisePersonalRecordAsync(1, userId);

            // Het gewicht van de andere gebruiker mag niet meetellen.
            Assert.NotNull(result);
            Assert.Equal(80, result.Weight);
            Assert.Equal(10, result.Reps);
        }

        [Fact]
        public async Task GetExercisePersonalRecordAsync_DifferentExercises_ReturnsOnlyRequestedExercise()
        {
            // Maak een nieuwe test database aan.
            using var context = CreateDbContext();
            var exercisesService = new ExercisesService(context);
            var userId = Guid.NewGuid();

            // Voeg sets van twee verschillende oefeningen toe.
            await AddWorkoutSetTestDataAsync(context, userId, 1, 80, 10);
            await AddWorkoutSetTestDataAsync(context, userId, 2, 150, 5);

            // Haal alleen het record van oefening één op.
            var result = await exercisesService.GetExercisePersonalRecordAsync(1, userId);

            // De andere oefening mag niet meetellen.
            Assert.NotNull(result);
            Assert.Equal(80, result.Weight);
            Assert.Equal(10, result.Reps);
        }

        [Fact]
        public async Task GetExerciseLastPerformedAsync_NoCompletedSessions_ReturnsZero()
        {
            // Maak een lege test database aan.
            using var context = CreateDbContext();
            var exercisesService = new ExercisesService(context);

            // Haal de laatste set op van een gebruiker zonder trainingen.
            var result = await exercisesService.GetExerciseLastPerformedAsync(
                1, Guid.NewGuid());

            // Controleer of beide waarden nul zijn.
            Assert.NotNull(result);
            Assert.Equal(0, result.Weight);
            Assert.Equal(0, result.Reps);
        }

        [Fact]
        public async Task GetExerciseLastPerformedAsync_CompletedSession_ReturnsLastSet()
        {
            // Maak een nieuwe test database aan.
            using var context = CreateDbContext();
            var exercisesService = new ExercisesService(context);
            var userId = Guid.NewGuid();

            // Maak de IDs voor de training aan.
            var workoutId = Guid.NewGuid();
            var sessionId = Guid.NewGuid();
            var sessionExerciseId = Guid.NewGuid();

            // Voeg een workout toe.
            context.Workouts.Add(new Workout
            {
                Id = workoutId,
                UserId = userId,
                Name = "Push Workout",
                Type = WorkoutType.Strength
            });

            // Voeg een afgeronde session toe.
            context.WorkoutSessions.Add(new WorkoutSession
            {
                Id = sessionId,
                WorkoutId = workoutId,
                StartedAt = DateTime.UtcNow,
                LastActivityAt = DateTime.UtcNow,
                IsCompleted = true
            });

            // Voeg een oefening toe.
            context.Exercises.Add(new Exercise
            {
                Id = 1,
                Name = "Bench Press",
                Description = "Borst oefening"
            });

            // Koppel de oefening aan de session.
            context.WorkoutSessionExercises.Add(new WorkoutSessionExercise
            {
                Id = sessionExerciseId,
                WorkoutSessionId = sessionId,
                ExerciseId = 1
            });

            // Voeg twee sets in een andere volgorde toe.
            context.WorkoutSets.AddRange(
                new WorkoutSet
                {
                    Id = Guid.NewGuid(),
                    WorkoutSessionExerciseId = sessionExerciseId,
                    SetNumber = 2,
                    Weight = 90,
                    Reps = 8
                },
                new WorkoutSet
                {
                    Id = Guid.NewGuid(),
                    WorkoutSessionExerciseId = sessionExerciseId,
                    SetNumber = 1,
                    Weight = 80,
                    Reps = 10
                });

            // Sla de gegevens op.
            await context.SaveChangesAsync();

            // Haal de laatst uitgevoerde set op.
            var result = await exercisesService.GetExerciseLastPerformedAsync(1, userId);

            // Controleer of set twee wordt teruggegeven.
            Assert.NotNull(result);
            Assert.Equal(90, result.Weight);
            Assert.Equal(8, result.Reps);
        }

        [Fact]
        public async Task GetExerciseLastPerformedAsync_MultipleSessions_ReturnsLatestSession()
        {
            // Maak een nieuwe test database aan.
            using var context = CreateDbContext();
            var exercisesService = new ExercisesService(context);
            var userId = Guid.NewGuid();

            // Voeg een oude training met een hoog gewicht toe.
            await AddWorkoutSetTestDataAsync(
                context, userId, 1, 120, 5,
                startedAt: new DateTime(2025, 1, 1));

            // Voeg een nieuwere training met een lager gewicht toe.
            await AddWorkoutSetTestDataAsync(
                context, userId, 1, 80, 10,
                startedAt: new DateTime(2025, 2, 1));

            // Haal de laatst uitgevoerde set op.
            var result = await exercisesService.GetExerciseLastPerformedAsync(1, userId);

            // Controleer of de nieuwste training gebruikt wordt.
            Assert.NotNull(result);
            Assert.Equal(80, result.Weight);
            Assert.Equal(10, result.Reps);
        }

        [Fact]
        public async Task GetExerciseLastPerformedAsync_IncompleteSession_IgnoresSession()
        {
            // Maak een nieuwe test database aan.
            using var context = CreateDbContext();
            var exercisesService = new ExercisesService(context);
            var userId = Guid.NewGuid();

            // Voeg een onafgeronde training toe.
            await AddWorkoutSetTestDataAsync(
                context, userId, 1, 100, 5, isCompleted: false);

            // Haal de laatst uitgevoerde set op.
            var result = await exercisesService.GetExerciseLastPerformedAsync(1, userId);

            // Een onafgeronde training mag niet meetellen.
            Assert.NotNull(result);
            Assert.Equal(0, result.Weight);
            Assert.Equal(0, result.Reps);
        }

        [Fact]
        public async Task GetExerciseLastPerformedAsync_NewerIncompleteSession_ReturnsOlderCompletedSession()
        {
            // Maak een nieuwe test database aan.
            using var context = CreateDbContext();
            var exercisesService = new ExercisesService(context);
            var userId = Guid.NewGuid();

            // Voeg een oudere afgeronde training toe.
            await AddWorkoutSetTestDataAsync(
                context, userId, 1, 80, 10,
                startedAt: new DateTime(2025, 1, 1));

            // Voeg een nieuwere onafgeronde training toe.
            await AddWorkoutSetTestDataAsync(
                context, userId, 1, 100, 5,
                isCompleted: false,
                startedAt: new DateTime(2025, 2, 1));

            // Haal de laatst uitgevoerde set op.
            var result = await exercisesService.GetExerciseLastPerformedAsync(1, userId);

            // Alleen de afgeronde training mag meetellen.
            Assert.NotNull(result);
            Assert.Equal(80, result.Weight);
            Assert.Equal(10, result.Reps);
        }

        [Fact]
        public async Task GetExerciseLastPerformedAsync_OtherUserHasNewerSession_IgnoresOtherUser()
        {
            // Maak een nieuwe test database aan.
            using var context = CreateDbContext();
            var exercisesService = new ExercisesService(context);

            // Maak twee gebruikers aan.
            var userId = Guid.NewGuid();
            var otherUserId = Guid.NewGuid();

            // Voeg een training van de eerste gebruiker toe.
            await AddWorkoutSetTestDataAsync(
                context, userId, 1, 80, 10,
                startedAt: new DateTime(2025, 1, 1));

            // Voeg een nieuwere training van een andere gebruiker toe.
            await AddWorkoutSetTestDataAsync(
                context, otherUserId, 1, 120, 5,
                startedAt: new DateTime(2025, 2, 1));

            // Haal de laatste set van de eerste gebruiker op.
            var result = await exercisesService.GetExerciseLastPerformedAsync(1, userId);

            // De andere gebruiker mag niet meetellen.
            Assert.NotNull(result);
            Assert.Equal(80, result.Weight);
            Assert.Equal(10, result.Reps);
        }

        [Fact]
        public async Task GetExerciseLastPerformedAsync_DifferentExercises_ReturnsOnlyRequestedExercise()
        {
            // Maak een nieuwe test database aan.
            using var context = CreateDbContext();
            var exercisesService = new ExercisesService(context);
            var userId = Guid.NewGuid();

            // Voeg een training van oefening één toe.
            await AddWorkoutSetTestDataAsync(
                context, userId, 1, 80, 10,
                startedAt: new DateTime(2025, 1, 1));

            // Voeg een nieuwere training van oefening twee toe.
            await AddWorkoutSetTestDataAsync(
                context, userId, 2, 120, 5,
                startedAt: new DateTime(2025, 2, 1));

            // Haal alleen de laatste set van oefening één op.
            var result = await exercisesService.GetExerciseLastPerformedAsync(1, userId);

            // Oefening twee mag niet meetellen.
            Assert.NotNull(result);
            Assert.Equal(80, result.Weight);
            Assert.Equal(10, result.Reps);
        }
    }
}
