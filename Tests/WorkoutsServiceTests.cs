using Microsoft.EntityFrameworkCore;
using WebAPI;
using WebAPI.Exceptions;
using WebAPI.Models.Exercises;
using WebAPI.Models.Workout;
using WebAPI.Services;

namespace Tests
{
    public class WorkoutsServiceTests
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

        [Fact]
        public async Task CreateWorkoutAsync_ValidData_CreatesWorkout()
        {
            // Maak een nieuwe test database aan.
            using var context = CreateDbContext();

            // Geef de test database aan de echte WorkoutsService.
            // Zo testen we de echte service zonder een echte database te gebruiken.
            var workoutsService = new WorkoutsService(context);

            // Maak een neppe UserId aan.
            var userId = Guid.NewGuid();

            // Maak de workout gegevens aan.
            var workoutDto = new WorkoutDto
            {
                Name = "Push Workout",
                Type = WorkoutType.Cardio
            };

            // Voer de echte CreateWorkoutAsync methode uit.
            await workoutsService.CreateWorkoutAsync(workoutDto, userId);

            // Haal de aangemaakte workout uit de test database.
            var workout = await context.Workouts.FirstOrDefaultAsync();

            // Controleer of de workout is aangemaakt.
            Assert.NotNull(workout);

            // Controleer of de workout aan de juiste gebruiker gekoppeld is.
            Assert.Equal(userId, workout.UserId);

            // Controleer of de naam van de workout klopt.
            Assert.Equal("Push Workout", workout.Name);

            // Controleer of het type van de workout klopt.
            Assert.Equal(WorkoutType.Cardio, workout.Type);
        }

        [Fact]
        public async Task CreateWorkoutAsync_NameContainsSpaces_TrimsWorkoutName()
        {
            // Maak een nieuwe test database aan.
            using var context = CreateDbContext();

            // Geef de test database aan de echte WorkoutsService.
            var workoutsService = new WorkoutsService(context);

            // Maak een neppe UserId aan.
            var userId = Guid.NewGuid();

            // Maak de workout gegevens aan met spaties voor en achter de naam.
            var workoutDto = new WorkoutDto
            {
                Name = "   Push Workout   ",
                Type = WorkoutType.Strength
            };

            // Voer de echte CreateWorkoutAsync methode uit.
            await workoutsService.CreateWorkoutAsync(workoutDto, userId);

            // Haal de aangemaakte workout uit de test database.
            var workout = await context.Workouts.FirstAsync();

            // Controleer of de spaties voor en achter de naam zijn verwijderd.
            Assert.Equal("Push Workout", workout.Name);
        }

        [Fact]
        public async Task GetMyWorkoutsAsync_UserHasWorkouts_ReturnsOnlyWorkoutsFromUser()
        {
            // Maak een nieuwe test database aan.
            using var context = CreateDbContext();

            // Geef de test database aan de echte WorkoutsService.
            var workoutsService = new WorkoutsService(context);

            // Maak twee verschillende neppe gebruikers aan.
            var userId = Guid.NewGuid();
            var otherUserId = Guid.NewGuid();

            // Voeg twee workouts toe voor de gebruiker die we gaan testen.
            context.Workouts.AddRange(
                new Workout
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    Name = "Push Workout",
                    Type = WorkoutType.Mixed
                },
                new Workout
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    Name = "Pull Workout",
                    Type = WorkoutType.Mixed
                });

            // Voeg ook een workout toe van een andere gebruiker.
            context.Workouts.Add(
                new Workout
                {
                    Id = Guid.NewGuid(),
                    UserId = otherUserId,
                    Name = "Other Workout",
                    Type = WorkoutType.Mixed
                });

            // Sla de test gegevens op in de test database.
            await context.SaveChangesAsync();

            // Haal de workouts op van de gebruiker die we testen.
            var result = await workoutsService.GetMyWorkoutsAsync(userId);

            // Controleer of alleen de twee workouts van deze gebruiker worden teruggegeven.
            Assert.Equal(2, result.Count);

            // Controleer of de workout van de andere gebruiker niet wordt teruggegeven.
            Assert.DoesNotContain(result, w => w.Name == "Other Workout");

            // Controleer of alle teruggegeven workouts van onze test gebruiker zijn.
            Assert.Contains(result, w => w.Name == "Push Workout");
            Assert.Contains(result, w => w.Name == "Pull Workout");
        }

        [Fact]
        public async Task GetMyWorkoutsAsync_UserHasWorkouts_ReturnsWorkoutsOrderedByName()
        {
            // Maak een nieuwe test database aan.
            using var context = CreateDbContext();

            // Geef de test database aan de echte WorkoutsService.
            var workoutsService = new WorkoutsService(context);

            // Maak een neppe UserId aan.
            var userId = Guid.NewGuid();

            // Voeg workouts in een verkeerde alfabetische volgorde toe.
            context.Workouts.AddRange(
                new Workout
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    Name = "Push Workout",
                    Type = WorkoutType.Strength
                },
                new Workout
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    Name = "Leg Workout",
                    Type = WorkoutType.Strength
                },
                new Workout
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    Name = "Back Workout",
                    Type = WorkoutType.Strength
                });

            // Sla de test gegevens op in de test database.
            await context.SaveChangesAsync();

            // Haal de workouts op van de gebruiker.
            var result = await workoutsService.GetMyWorkoutsAsync(userId);

            // Controleer of de workouts alfabetisch op naam worden teruggegeven.
            Assert.Equal("Back Workout", result[0].Name);
            Assert.Equal("Leg Workout", result[1].Name);
            Assert.Equal("Push Workout", result[2].Name);
        }

        [Fact]
        public async Task GetWorkoutByIdAsync_WorkoutExists_ReturnsWorkout()
        {
            // Maak een nieuwe test database aan.
            using var context = CreateDbContext();

            // Geef de test database aan de echte WorkoutsService.
            var workoutsService = new WorkoutsService(context);

            // Maak een neppe UserId en WorkoutId aan.
            var userId = Guid.NewGuid();
            var workoutId = Guid.NewGuid();

            // Voeg de workout toe die we later gaan ophalen.
            context.Workouts.Add(new Workout
            {
                Id = workoutId,
                UserId = userId,
                Name = "Push Workout",
                Type = WorkoutType.Strength
            });

            // Sla de workout op in de test database.
            await context.SaveChangesAsync();

            // Haal de workout op met het juiste WorkoutId en UserId.
            var result = await workoutsService.GetWorkoutByIdAsync(workoutId, userId);

            // Controleer of de workout gevonden is.
            Assert.NotNull(result);

            // Controleer of het juiste WorkoutId wordt teruggegeven.
            Assert.Equal(workoutId, result.Id);

            // Controleer of de juiste naam wordt teruggegeven.
            Assert.Equal("Push Workout", result.Name);

            // Controleer of het juiste type wordt teruggegeven.
            Assert.Equal(WorkoutType.Strength, result.Type);
        }

        [Fact]
        public async Task GetWorkoutByIdAsync_WorkoutDoesNotExist_ReturnsNull()
        {
            // Maak een nieuwe test database aan.
            using var context = CreateDbContext();

            // Geef de test database aan de echte WorkoutsService.
            var workoutsService = new WorkoutsService(context);

            // Maak een neppe UserId en een WorkoutId aan die niet in de database staan.
            var userId = Guid.NewGuid();
            var workoutId = Guid.NewGuid();

            // Probeer een workout op te halen die niet bestaat.
            var result = await workoutsService.GetWorkoutByIdAsync(workoutId, userId);

            // Controleer of null wordt teruggegeven omdat de workout niet bestaat.
            Assert.Null(result);
        }

        [Fact]
        public async Task GetWorkoutByIdAsync_WorkoutBelongsToDifferentUser_ReturnsNull()
        {
            // Maak een nieuwe test database aan.
            using var context = CreateDbContext();

            // Geef de test database aan de echte WorkoutsService.
            var workoutsService = new WorkoutsService(context);

            // Maak twee verschillende neppe gebruikers aan.
            var ownerUserId = Guid.NewGuid();
            var otherUserId = Guid.NewGuid();

            // Maak een neppe WorkoutId aan.
            var workoutId = Guid.NewGuid();

            // Voeg een workout toe die van de eerste gebruiker is.
            context.Workouts.Add(new Workout
            {
                Id = workoutId,
                UserId = ownerUserId,
                Name = "Push Workout",
                Type = WorkoutType.Strength
            });

            // Sla de workout op in de test database.
            await context.SaveChangesAsync();

            // Probeer de workout op te halen als een andere gebruiker.
            var result = await workoutsService.GetWorkoutByIdAsync(workoutId, otherUserId);

            // Controleer of null wordt teruggegeven omdat de workout niet van deze gebruiker is.
            Assert.Null(result);
        }

        [Fact]
        public async Task AddExerciseToWorkoutAsync_ValidData_AddsExerciseToWorkout()
        {
            // Maak een nieuwe test database aan.
            using var context = CreateDbContext();

            // Geef de test database aan de echte WorkoutsService.
            var workoutsService = new WorkoutsService(context);

            // Maak een neppe gebruiker en workout aan.
            var userId = Guid.NewGuid();
            var workoutId = Guid.NewGuid();

            // Voeg een workout toe.
            context.Workouts.Add(new Workout
            {
                Id = workoutId,
                UserId = userId,
                Name = "Push Workout",
                Type = WorkoutType.Strength
            });

            // Voeg een oefening toe met een verplichte Description.
            context.Exercises.Add(new Exercise
            {
                Id = 1,
                Name = "Bench Press",
                Description = "Borst oefening"
            });

            // Sla de test gegevens op.
            await context.SaveChangesAsync();

            // Maak de gegevens aan waarmee de oefening wordt toegevoegd.
            var workoutExerciseDto = new WorkoutExerciseDto
            {
                WorkoutId = workoutId,
                ExerciseId = 1
            };

            // Voeg de oefening toe aan de workout.
            await workoutsService.AddExerciseToWorkoutAsync(workoutExerciseDto, userId);

            // Haal de aangemaakte koppeling uit de database.
            var workoutExercise = await context.WorkoutExercises.FirstOrDefaultAsync();

            // Controleer of de koppeling is aangemaakt.
            Assert.NotNull(workoutExercise);

            // Controleer of de juiste workout en oefening gekoppeld zijn.
            Assert.Equal(workoutId, workoutExercise.WorkoutId);
            Assert.Equal(1, workoutExercise.ExerciseId);

            // De eerste oefening hoort SortOrder 1 te krijgen.
            Assert.Equal(1, workoutExercise.SortOrder);
        }

        [Fact]
        public async Task AddExerciseToWorkoutAsync_WorkoutDoesNotExist_ThrowsNotFoundException()
        {
            // Maak een nieuwe test database aan.
            using var context = CreateDbContext();

            // Geef de test database aan de echte WorkoutsService.
            var workoutsService = new WorkoutsService(context);

            // Maak gegevens met een WorkoutId dat niet bestaat.
            var workoutExerciseDto = new WorkoutExerciseDto
            {
                WorkoutId = Guid.NewGuid(),
                ExerciseId = 1
            };

            // Controleer of een NotFoundException wordt gegooid.
            await Assert.ThrowsAsync<NotFoundException>(() =>
                workoutsService.AddExerciseToWorkoutAsync(workoutExerciseDto, Guid.NewGuid()));
        }

        [Fact]
        public async Task AddExerciseToWorkoutAsync_ExerciseDoesNotExist_ThrowsNotFoundException()
        {
            // Maak een nieuwe test database aan.
            using var context = CreateDbContext();

            // Geef de test database aan de echte WorkoutsService.
            var workoutsService = new WorkoutsService(context);

            // Maak een neppe gebruiker en workout aan.
            var userId = Guid.NewGuid();
            var workoutId = Guid.NewGuid();

            // Voeg de workout toe.
            context.Workouts.Add(new Workout
            {
                Id = workoutId,
                UserId = userId,
                Name = "Push Workout",
                Type = WorkoutType.Strength
            });

            // Sla de workout op.
            await context.SaveChangesAsync();

            // Gebruik een ExerciseId dat niet bestaat.
            var workoutExerciseDto = new WorkoutExerciseDto
            {
                WorkoutId = workoutId,
                ExerciseId = 999
            };

            // Controleer of een NotFoundException wordt gegooid.
            await Assert.ThrowsAsync<NotFoundException>(() =>
                workoutsService.AddExerciseToWorkoutAsync(workoutExerciseDto, userId));
        }

        [Fact]
        public async Task AddExerciseToWorkoutAsync_ExerciseAlreadyAdded_ThrowsConflictException()
        {
            // Maak een nieuwe test database aan.
            using var context = CreateDbContext();

            // Geef de test database aan de echte WorkoutsService.
            var workoutsService = new WorkoutsService(context);

            // Maak een neppe gebruiker en workout aan.
            var userId = Guid.NewGuid();
            var workoutId = Guid.NewGuid();

            // Voeg een workout toe.
            context.Workouts.Add(new Workout
            {
                Id = workoutId,
                UserId = userId,
                Name = "Push Workout",
                Type = WorkoutType.Strength
            });

            // Voeg een oefening toe met een verplichte Description.
            context.Exercises.Add(new Exercise
            {
                Id = 1,
                Name = "Bench Press",
                Description = "Borst oefening"
            });

            // Voeg dezelfde oefening alvast aan de workout toe.
            context.WorkoutExercises.Add(new WorkoutExercise
            {
                WorkoutId = workoutId,
                ExerciseId = 1,
                SortOrder = 1
            });

            // Sla de test gegevens op.
            await context.SaveChangesAsync();

            // Maak dezelfde koppeling opnieuw.
            var workoutExerciseDto = new WorkoutExerciseDto
            {
                WorkoutId = workoutId,
                ExerciseId = 1
            };

            // Controleer of een ConflictException wordt gegooid.
            await Assert.ThrowsAsync<ConflictException>(() =>
                workoutsService.AddExerciseToWorkoutAsync(workoutExerciseDto, userId));
        }

        [Fact]
        public async Task AddExerciseToWorkoutAsync_WorkoutAlreadyHasExercise_UsesNextSortOrder()
        {
            // Maak een nieuwe test database aan.
            using var context = CreateDbContext();

            // Geef de test database aan de echte WorkoutsService.
            var workoutsService = new WorkoutsService(context);

            // Maak een neppe gebruiker en workout aan.
            var userId = Guid.NewGuid();
            var workoutId = Guid.NewGuid();

            // Voeg een workout toe.
            context.Workouts.Add(new Workout
            {
                Id = workoutId,
                UserId = userId,
                Name = "Push Workout",
                Type = WorkoutType.Strength
            });

            // Voeg twee oefeningen toe met hun verplichte Description.
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
                    Name = "Shoulder Press",
                    Description = "Schouder oefening"
                });

            // Voeg de eerste oefening alvast aan de workout toe.
            context.WorkoutExercises.Add(new WorkoutExercise
            {
                WorkoutId = workoutId,
                ExerciseId = 1,
                SortOrder = 1
            });

            // Sla de test gegevens op.
            await context.SaveChangesAsync();

            // Maak de gegevens van de tweede oefening aan.
            var workoutExerciseDto = new WorkoutExerciseDto
            {
                WorkoutId = workoutId,
                ExerciseId = 2
            };

            // Voeg de tweede oefening toe.
            await workoutsService.AddExerciseToWorkoutAsync(workoutExerciseDto, userId);

            // Haal de tweede koppeling op.
            var workoutExercise = await context.WorkoutExercises
                .FirstAsync(we => we.ExerciseId == 2);

            // Controleer of de volgende SortOrder is gebruikt.
            Assert.Equal(2, workoutExercise.SortOrder);
        }

        [Fact]
        public async Task GetExercisesByWorkoutIdAsync_WorkoutHasExercises_ReturnsExercisesInCorrectOrder()
        {
            // Maak een nieuwe test database aan.
            using var context = CreateDbContext();

            // Geef de test database aan de echte WorkoutsService.
            var workoutsService = new WorkoutsService(context);

            // Maak een neppe gebruiker en workout aan.
            var userId = Guid.NewGuid();
            var workoutId = Guid.NewGuid();

            // Voeg een workout toe.
            context.Workouts.Add(new Workout
            {
                Id = workoutId,
                UserId = userId,
                Name = "Push Workout",
                Type = WorkoutType.Strength
            });

            // Voeg twee oefeningen toe met hun verplichte Description.
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
                    Name = "Shoulder Press",
                    Description = "Schouder oefening"
                });

            // Voeg de oefeningen in een andere volgorde toe.
            context.WorkoutExercises.AddRange(
                new WorkoutExercise
                {
                    WorkoutId = workoutId,
                    ExerciseId = 1,
                    SortOrder = 2
                },
                new WorkoutExercise
                {
                    WorkoutId = workoutId,
                    ExerciseId = 2,
                    SortOrder = 1
                });

            // Sla de test gegevens op.
            await context.SaveChangesAsync();

            // Haal de oefeningen van de workout op.
            var result = await workoutsService.GetExercisesByWorkoutIdAsync(workoutId, userId);

            // Controleer of beide oefeningen terugkomen.
            Assert.Equal(2, result.Count);

            // Controleer of de SortOrder gebruikt wordt.
            Assert.Equal("Shoulder Press", result[0].Name);
            Assert.Equal("Bench Press", result[1].Name);
        }

        [Fact]
        public async Task GetExercisesByWorkoutIdAsync_WorkoutDoesNotExist_ThrowsNotFoundException()
        {
            // Maak een nieuwe test database aan.
            using var context = CreateDbContext();

            // Geef de test database aan de echte WorkoutsService.
            var workoutsService = new WorkoutsService(context);

            // Probeer oefeningen op te halen van een workout die niet bestaat.
            await Assert.ThrowsAsync<NotFoundException>(() =>
                workoutsService.GetExercisesByWorkoutIdAsync(Guid.NewGuid(), Guid.NewGuid()));
        }

        [Fact]
        public async Task DeleteWorkoutAsync_WorkoutExists_DeletesWorkout()
        {
            // Maak een nieuwe test database aan.
            using var context = CreateDbContext();

            // Geef de test database aan de echte WorkoutsService.
            var workoutsService = new WorkoutsService(context);

            // Maak een neppe gebruiker en workout aan.
            var userId = Guid.NewGuid();
            var workoutId = Guid.NewGuid();

            // Voeg de workout toe.
            context.Workouts.Add(new Workout
            {
                Id = workoutId,
                UserId = userId,
                Name = "Push Workout",
                Type = WorkoutType.Strength
            });

            // Sla de workout op.
            await context.SaveChangesAsync();

            // Verwijder de workout.
            await workoutsService.DeleteWorkoutAsync(workoutId, userId);

            // Controleer of de workout nog bestaat.
            var exists = await context.Workouts.AnyAsync(w => w.Id == workoutId);

            // De workout hoort niet meer te bestaan.
            Assert.False(exists);
        }

        [Fact]
        public async Task DeleteWorkoutAsync_WorkoutDoesNotExist_ThrowsNotFoundException()
        {
            // Maak een nieuwe test database aan.
            using var context = CreateDbContext();

            // Geef de test database aan de echte WorkoutsService.
            var workoutsService = new WorkoutsService(context);

            // Probeer een workout te verwijderen die niet bestaat.
            await Assert.ThrowsAsync<NotFoundException>(() =>
                workoutsService.DeleteWorkoutAsync(Guid.NewGuid(), Guid.NewGuid()));
        }

        [Fact]
        public async Task UpdateWorkoutAsync_ValidData_UpdatesWorkout()
        {
            // Maak een nieuwe test database aan.
            using var context = CreateDbContext();

            // Geef de test database aan de echte WorkoutsService.
            var workoutsService = new WorkoutsService(context);

            // Maak een neppe gebruiker en workout aan.
            var userId = Guid.NewGuid();
            var workoutId = Guid.NewGuid();

            // Voeg de originele workout toe.
            context.Workouts.Add(new Workout
            {
                Id = workoutId,
                UserId = userId,
                Name = "Old Workout",
                Type = WorkoutType.Cardio
            });

            // Sla de workout op.
            await context.SaveChangesAsync();

            // Maak de nieuwe gegevens van de workout aan.
            var workoutDto = new WorkoutDto
            {
                Id = workoutId,
                Name = "   New Workout   ",
                Type = WorkoutType.Strength
            };

            // Update de workout.
            await workoutsService.UpdateWorkoutAsync(workoutDto, userId);

            // Haal de aangepaste workout op.
            var workout = await context.Workouts.FirstAsync(w => w.Id == workoutId);

            // Controleer of de naam aangepast en getrimd is.
            Assert.Equal("New Workout", workout.Name);

            // Controleer of het type aangepast is.
            Assert.Equal(WorkoutType.Strength, workout.Type);
        }

        [Fact]
        public async Task UpdateWorkoutAsync_WorkoutDoesNotExist_ThrowsNotFoundException()
        {
            // Maak een nieuwe test database aan.
            using var context = CreateDbContext();

            // Geef de test database aan de echte WorkoutsService.
            var workoutsService = new WorkoutsService(context);

            // Maak gegevens voor een workout die niet bestaat.
            var workoutDto = new WorkoutDto
            {
                Id = Guid.NewGuid(),
                Name = "Push Workout",
                Type = WorkoutType.Strength
            };

            // Controleer of een NotFoundException wordt gegooid.
            await Assert.ThrowsAsync<NotFoundException>(() =>
                workoutsService.UpdateWorkoutAsync(workoutDto, Guid.NewGuid()));
        }

        [Fact]
        public async Task IsExerciseInWorkoutAsync_ExerciseIsInWorkout_ReturnsTrue()
        {
            // Maak een nieuwe test database aan.
            using var context = CreateDbContext();

            // Geef de test database aan de echte WorkoutsService.
            var workoutsService = new WorkoutsService(context);

            // Maak een neppe gebruiker en workout aan.
            var userId = Guid.NewGuid();
            var workoutId = Guid.NewGuid();

            // Voeg de workout toe.
            context.Workouts.Add(new Workout
            {
                Id = workoutId,
                UserId = userId,
                Name = "Push Workout",
                Type = WorkoutType.Strength
            });

            // Voeg een oefening toe met een verplichte Description.
            context.Exercises.Add(new Exercise
            {
                Id = 1,
                Name = "Bench Press",
                Description = "Borst oefening"
            });

            // Koppel de oefening aan de workout.
            context.WorkoutExercises.Add(new WorkoutExercise
            {
                WorkoutId = workoutId,
                ExerciseId = 1,
                SortOrder = 1
            });

            // Sla de test gegevens op.
            await context.SaveChangesAsync();

            // Controleer of de oefening in de workout zit.
            var result = await workoutsService.IsExerciseInWorkoutAsync(workoutId, 1, userId);

            // Het resultaat hoort true te zijn.
            Assert.True(result);
        }

        [Fact]
        public async Task IsExerciseInWorkoutAsync_ExerciseIsNotInWorkout_ReturnsFalse()
        {
            // Maak een nieuwe test database aan.
            using var context = CreateDbContext();

            // Geef de test database aan de echte WorkoutsService.
            var workoutsService = new WorkoutsService(context);

            // Controleer een koppeling die niet bestaat.
            var result = await workoutsService.IsExerciseInWorkoutAsync(
                Guid.NewGuid(), 1, Guid.NewGuid());

            // Het resultaat hoort false te zijn.
            Assert.False(result);
        }

        [Fact]
        public async Task DeleteExerciseFromWorkoutAsync_ExerciseExists_DeletesExerciseFromWorkout()
        {
            // Maak een nieuwe test database aan.
            using var context = CreateDbContext();

            // Geef de test database aan de echte WorkoutsService.
            var workoutsService = new WorkoutsService(context);

            // Maak een neppe gebruiker en workout aan.
            var userId = Guid.NewGuid();
            var workoutId = Guid.NewGuid();

            // Voeg een workout toe.
            context.Workouts.Add(new Workout
            {
                Id = workoutId,
                UserId = userId,
                Name = "Push Workout",
                Type = WorkoutType.Strength
            });

            // Voeg een oefening toe met een verplichte Description.
            context.Exercises.Add(new Exercise
            {
                Id = 1,
                Name = "Bench Press",
                Description = "Borst oefening"
            });

            // Koppel de oefening aan de workout.
            context.WorkoutExercises.Add(new WorkoutExercise
            {
                WorkoutId = workoutId,
                ExerciseId = 1,
                SortOrder = 1
            });

            // Sla de test gegevens op.
            await context.SaveChangesAsync();

            // Maak de gegevens voor het verwijderen aan.
            var workoutExerciseDto = new WorkoutExerciseDto
            {
                WorkoutId = workoutId,
                ExerciseId = 1
            };

            // Verwijder de oefening uit de workout.
            await workoutsService.DeleteExerciseFromWorkoutAsync(workoutExerciseDto, userId);

            // Controleer of de koppeling nog bestaat.
            var exists = await context.WorkoutExercises.AnyAsync(
                we => we.WorkoutId == workoutId && we.ExerciseId == 1);

            // De koppeling hoort verwijderd te zijn.
            Assert.False(exists);
        }

        [Fact]
        public async Task DeleteExerciseFromWorkoutAsync_WorkoutDoesNotExist_ThrowsInvalidOperationException()
        {
            // Maak een nieuwe test database aan.
            using var context = CreateDbContext();

            // Geef de test database aan de echte WorkoutsService.
            var workoutsService = new WorkoutsService(context);

            // Maak gegevens voor een workout die niet bestaat.
            var workoutExerciseDto = new WorkoutExerciseDto
            {
                WorkoutId = Guid.NewGuid(),
                ExerciseId = 1
            };

            // Controleer of een InvalidOperationException wordt gegooid.
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                workoutsService.DeleteExerciseFromWorkoutAsync(
                    workoutExerciseDto, Guid.NewGuid()));
        }

        [Fact]
        public async Task DeleteExerciseFromWorkoutAsync_ExerciseIsNotInWorkout_ThrowsInvalidOperationException()
        {
            // Maak een nieuwe test database aan.
            using var context = CreateDbContext();

            // Geef de test database aan de echte WorkoutsService.
            var workoutsService = new WorkoutsService(context);

            // Maak een neppe gebruiker en workout aan.
            var userId = Guid.NewGuid();
            var workoutId = Guid.NewGuid();

            // Voeg een workout zonder oefeningen toe.
            context.Workouts.Add(new Workout
            {
                Id = workoutId,
                UserId = userId,
                Name = "Push Workout",
                Type = WorkoutType.Strength
            });

            // Sla de workout op.
            await context.SaveChangesAsync();

            // Maak gegevens voor een koppeling die niet bestaat.
            var workoutExerciseDto = new WorkoutExerciseDto
            {
                WorkoutId = workoutId,
                ExerciseId = 1
            };

            // Controleer of een InvalidOperationException wordt gegooid.
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                workoutsService.DeleteExerciseFromWorkoutAsync(
                    workoutExerciseDto, userId));
        }

        [Fact]
        public async Task StartWorkoutAsync_WorkoutExists_CreatesWorkoutSession()
        {
            // Maak een nieuwe test database aan.
            using var context = CreateDbContext();

            // Geef de test database aan de echte WorkoutsService.
            var workoutsService = new WorkoutsService(context);

            // Maak een neppe gebruiker en workout aan.
            var userId = Guid.NewGuid();
            var workoutId = Guid.NewGuid();

            // Voeg een workout toe.
            context.Workouts.Add(new Workout
            {
                Id = workoutId,
                UserId = userId,
                Name = "Push Workout",
                Type = WorkoutType.Strength
            });

            // Sla de workout op.
            await context.SaveChangesAsync();

            // Start een workout session.
            var result = await workoutsService.StartWorkoutAsync(workoutId, userId);

            // Controleer of een SessionId is aangemaakt.
            Assert.NotEqual(Guid.Empty, result.Id);

            // Controleer of de session aan de juiste workout gekoppeld is.
            Assert.Equal(workoutId, result.WorkoutId);

            // Haal de session uit de database.
            var workoutSession = await context.WorkoutSessions
                .FirstAsync(ws => ws.Id == result.Id);

            // Een nieuwe session hoort nog niet afgerond te zijn.
            Assert.False(workoutSession.IsCompleted);
        }

        [Fact]
        public async Task StartWorkoutAsync_WorkoutDoesNotExist_ThrowsNotFoundException()
        {
            // Maak een nieuwe test database aan.
            using var context = CreateDbContext();

            // Geef de test database aan de echte WorkoutsService.
            var workoutsService = new WorkoutsService(context);

            // Probeer een workout te starten die niet bestaat.
            await Assert.ThrowsAsync<NotFoundException>(() =>
                workoutsService.StartWorkoutAsync(Guid.NewGuid(), Guid.NewGuid()));
        }

        [Fact]
        public async Task GetWorkoutExercisesAsync_ValidSession_ReturnsWorkoutExercises()
        {
            // Maak een nieuwe test database aan.
            using var context = CreateDbContext();

            // Geef de test database aan de echte WorkoutsService.
            var workoutsService = new WorkoutsService(context);

            // Maak de IDs voor de test aan.
            var userId = Guid.NewGuid();
            var workoutId = Guid.NewGuid();
            var sessionId = Guid.NewGuid();

            // Voeg een workout toe.
            context.Workouts.Add(new Workout
            {
                Id = workoutId,
                UserId = userId,
                Name = "Push Workout",
                Type = WorkoutType.Strength
            });

            // Voeg een oefening toe met een verplichte Description.
            context.Exercises.Add(new Exercise
            {
                Id = 1,
                Name = "Bench Press",
                Description = "Borst oefening"
            });

            // Koppel de oefening aan de workout.
            context.WorkoutExercises.Add(new WorkoutExercise
            {
                WorkoutId = workoutId,
                ExerciseId = 1,
                SortOrder = 1
            });

            // Maak een workout session aan.
            context.WorkoutSessions.Add(new WorkoutSession
            {
                Id = sessionId,
                WorkoutId = workoutId,
                StartedAt = DateTime.UtcNow,
                LastActivityAt = DateTime.UtcNow,
                IsCompleted = false
            });

            // Sla de test gegevens op.
            await context.SaveChangesAsync();

            // Haal de oefeningen van de workout session op.
            var result = await workoutsService.GetWorkoutExercisesAsync(sessionId, userId);

            // Controleer of één oefening terugkomt.
            Assert.Single(result);

            // Controleer of de juiste oefening terugkomt.
            Assert.Equal("Bench Press", result[0].Name);
        }

        [Fact]
        public async Task GetWorkoutExercisesAsync_SessionDoesNotExist_ThrowsNotFoundException()
        {
            // Maak een nieuwe test database aan.
            using var context = CreateDbContext();

            // Geef de test database aan de echte WorkoutsService.
            var workoutsService = new WorkoutsService(context);

            // Probeer oefeningen op te halen van een session die niet bestaat.
            await Assert.ThrowsAsync<NotFoundException>(() =>
                workoutsService.GetWorkoutExercisesAsync(Guid.NewGuid(), Guid.NewGuid()));
        }

        [Fact]
        public async Task AddWorkoutSessionExerciseAsync_ValidData_AddsExerciseToSession()
        {
            // Maak een nieuwe test database aan.
            using var context = CreateDbContext();

            // Geef de test database aan de echte WorkoutsService.
            var workoutsService = new WorkoutsService(context);

            // Maak de IDs voor de test aan.
            var userId = Guid.NewGuid();
            var workoutId = Guid.NewGuid();
            var sessionId = Guid.NewGuid();

            // Voeg een workout toe.
            context.Workouts.Add(new Workout
            {
                Id = workoutId,
                UserId = userId,
                Name = "Push Workout",
                Type = WorkoutType.Strength
            });

            // Voeg een workout session toe.
            context.WorkoutSessions.Add(new WorkoutSession
            {
                Id = sessionId,
                WorkoutId = workoutId,
                StartedAt = DateTime.UtcNow,
                LastActivityAt = DateTime.UtcNow,
                IsCompleted = false
            });

            // Voeg een oefening toe met een verplichte Description.
            context.Exercises.Add(new Exercise
            {
                Id = 1,
                Name = "Bench Press",
                Description = "Borst oefening"
            });

            // Sla de test gegevens op.
            await context.SaveChangesAsync();

            // Maak de gegevens voor de session exercise aan.
            var workoutSessionExerciseDto = new WorkoutSessionExerciseDto
            {
                WorkoutSessionId = sessionId,
                ExerciseId = 1
            };

            // Voeg de oefening toe aan de session.
            var result = await workoutsService.AddWorkoutSessionExerciseAsync(
                workoutSessionExerciseDto, userId);

            // Controleer of een nieuw ID is aangemaakt.
            Assert.NotEqual(Guid.Empty, result);

            // Controleer of de koppeling in de database staat.
            var exists = await context.WorkoutSessionExercises
                .AnyAsync(wse => wse.Id == result);

            // De koppeling hoort te bestaan.
            Assert.True(exists);
        }

        [Fact]
        public async Task AddWorkoutSessionExerciseAsync_SessionDoesNotExist_ThrowsNotFoundException()
        {
            // Maak een nieuwe test database aan.
            using var context = CreateDbContext();

            // Geef de test database aan de echte WorkoutsService.
            var workoutsService = new WorkoutsService(context);

            // Maak gegevens voor een session die niet bestaat.
            var workoutSessionExerciseDto = new WorkoutSessionExerciseDto
            {
                WorkoutSessionId = Guid.NewGuid(),
                ExerciseId = 1
            };

            // Controleer of een NotFoundException wordt gegooid.
            await Assert.ThrowsAsync<NotFoundException>(() =>
                workoutsService.AddWorkoutSessionExerciseAsync(
                    workoutSessionExerciseDto, Guid.NewGuid()));
        }

        [Fact]
        public async Task AddWorkoutSessionExerciseAsync_ExerciseDoesNotExist_ThrowsNotFoundException()
        {
            // Maak een nieuwe test database aan.
            using var context = CreateDbContext();

            // Geef de test database aan de echte WorkoutsService.
            var workoutsService = new WorkoutsService(context);

            // Maak de IDs voor de test aan.
            var userId = Guid.NewGuid();
            var workoutId = Guid.NewGuid();
            var sessionId = Guid.NewGuid();

            // Voeg een workout toe.
            context.Workouts.Add(new Workout
            {
                Id = workoutId,
                UserId = userId,
                Name = "Push Workout",
                Type = WorkoutType.Strength
            });

            // Voeg een workout session toe.
            context.WorkoutSessions.Add(new WorkoutSession
            {
                Id = sessionId,
                WorkoutId = workoutId,
                StartedAt = DateTime.UtcNow,
                LastActivityAt = DateTime.UtcNow,
                IsCompleted = false
            });

            // Sla de test gegevens op.
            await context.SaveChangesAsync();

            // Gebruik een ExerciseId dat niet bestaat.
            var workoutSessionExerciseDto = new WorkoutSessionExerciseDto
            {
                WorkoutSessionId = sessionId,
                ExerciseId = 999
            };

            // Controleer of een NotFoundException wordt gegooid.
            await Assert.ThrowsAsync<NotFoundException>(() =>
                workoutsService.AddWorkoutSessionExerciseAsync(
                    workoutSessionExerciseDto, userId));
        }

        [Fact]
        public async Task AddWorkoutSessionExerciseAsync_ExerciseAlreadyAdded_ThrowsConflictException()
        {
            // Maak een nieuwe test database aan.
            using var context = CreateDbContext();

            // Geef de test database aan de echte WorkoutsService.
            var workoutsService = new WorkoutsService(context);

            // Maak de IDs voor de test aan.
            var userId = Guid.NewGuid();
            var workoutId = Guid.NewGuid();
            var sessionId = Guid.NewGuid();

            // Voeg een workout toe.
            context.Workouts.Add(new Workout
            {
                Id = workoutId,
                UserId = userId,
                Name = "Push Workout",
                Type = WorkoutType.Strength
            });

            // Voeg een workout session toe.
            context.WorkoutSessions.Add(new WorkoutSession
            {
                Id = sessionId,
                WorkoutId = workoutId,
                StartedAt = DateTime.UtcNow,
                LastActivityAt = DateTime.UtcNow,
                IsCompleted = false
            });

            // Voeg een oefening toe met een verplichte Description.
            context.Exercises.Add(new Exercise
            {
                Id = 1,
                Name = "Bench Press",
                Description = "Borst oefening"
            });

            // Voeg de oefening alvast aan de session toe.
            context.WorkoutSessionExercises.Add(new WorkoutSessionExercise
            {
                Id = Guid.NewGuid(),
                WorkoutSessionId = sessionId,
                ExerciseId = 1
            });

            // Sla de test gegevens op.
            await context.SaveChangesAsync();

            // Maak dezelfde koppeling nog een keer.
            var workoutSessionExerciseDto = new WorkoutSessionExerciseDto
            {
                WorkoutSessionId = sessionId,
                ExerciseId = 1
            };

            // Controleer of een ConflictException wordt gegooid.
            await Assert.ThrowsAsync<ConflictException>(() =>
                workoutsService.AddWorkoutSessionExerciseAsync(
                    workoutSessionExerciseDto, userId));
        }

        [Fact]
        public async Task AddWorkoutSetAsync_ValidData_AddsWorkoutSet()
        {
            // Maak een nieuwe test database aan.
            using var context = CreateDbContext();

            // Geef de test database aan de echte WorkoutsService.
            var workoutsService = new WorkoutsService(context);

            // Maak de IDs voor de test aan.
            var userId = Guid.NewGuid();
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

            // Voeg een workout session toe.
            context.WorkoutSessions.Add(new WorkoutSession
            {
                Id = sessionId,
                WorkoutId = workoutId,
                StartedAt = DateTime.UtcNow,
                LastActivityAt = DateTime.UtcNow.AddMinutes(-10),
                IsCompleted = false
            });

            // Voeg een oefening toe met een verplichte Description.
            context.Exercises.Add(new Exercise
            {
                Id = 1,
                Name = "Bench Press",
                Description = "Borst oefening"
            });

            // Voeg de oefening aan de session toe.
            context.WorkoutSessionExercises.Add(new WorkoutSessionExercise
            {
                Id = sessionExerciseId,
                WorkoutSessionId = sessionId,
                ExerciseId = 1
            });

            // Sla de test gegevens op.
            await context.SaveChangesAsync();

            // Maak de gegevens van de set aan.
            var workoutSetDto = new WorkoutSetDto
            {
                WorkoutSessionExerciseId = sessionExerciseId,
                SetNumber = 1,
                Weight = 80,
                Reps = 10
            };

            // Voeg de set toe.
            await workoutsService.AddWorkoutSetAsync(workoutSetDto, userId);

            // Haal de set uit de database.
            var workoutSet = await context.WorkoutSets.FirstOrDefaultAsync();

            // Controleer of de set is aangemaakt.
            Assert.NotNull(workoutSet);

            // Controleer of de gegevens van de set kloppen.
            Assert.Equal(workoutSetDto.SetNumber, workoutSet.SetNumber);
            Assert.Equal(workoutSetDto.Weight, workoutSet.Weight);
            Assert.Equal(workoutSetDto.Reps, workoutSet.Reps);
        }

        [Fact]
        public async Task AddWorkoutSetAsync_SessionExerciseDoesNotExist_ThrowsNotFoundException()
        {
            // Maak een nieuwe test database aan.
            using var context = CreateDbContext();

            // Geef de test database aan de echte WorkoutsService.
            var workoutsService = new WorkoutsService(context);

            // Maak een set voor een SessionExercise die niet bestaat.
            var workoutSetDto = new WorkoutSetDto
            {
                WorkoutSessionExerciseId = Guid.NewGuid(),
                SetNumber = 1,
                Weight = 80,
                Reps = 10
            };

            // Controleer of een NotFoundException wordt gegooid.
            await Assert.ThrowsAsync<NotFoundException>(() =>
                workoutsService.AddWorkoutSetAsync(workoutSetDto, Guid.NewGuid()));
        }

        [Fact]
        public async Task GetWorkoutSummaryAsync_ValidSession_ReturnsWorkoutSummary()
        {
            // Maak een nieuwe test database aan.
            using var context = CreateDbContext();

            // Geef de test database aan de echte WorkoutsService.
            var workoutsService = new WorkoutsService(context);

            // Maak de IDs voor de test aan.
            var userId = Guid.NewGuid();
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

            // Voeg een workout session toe.
            context.WorkoutSessions.Add(new WorkoutSession
            {
                Id = sessionId,
                WorkoutId = workoutId,
                StartedAt = DateTime.UtcNow,
                LastActivityAt = DateTime.UtcNow,
                IsCompleted = true
            });

            // Voeg een oefening toe met een verplichte Description.
            context.Exercises.Add(new Exercise
            {
                Id = 1,
                Name = "Bench Press",
                Description = "Borst oefening"
            });

            // Voeg de oefening aan de session toe.
            context.WorkoutSessionExercises.Add(new WorkoutSessionExercise
            {
                Id = sessionExerciseId,
                WorkoutSessionId = sessionId,
                ExerciseId = 1
            });

            // Voeg twee sets expres in de verkeerde volgorde toe.
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

            // Sla de test gegevens op.
            await context.SaveChangesAsync();

            // Haal de workout summary op.
            var result = await workoutsService.GetWorkoutSummaryAsync(sessionId, userId);

            // Controleer of de juiste session en workout terugkomen.
            Assert.Equal(sessionId, result.WorkoutSessionId);
            Assert.Equal("Push Workout", result.WorkoutName);

            // Controleer of één oefening terugkomt.
            Assert.Single(result.Exercises);

            // Controleer of de juiste oefening terugkomt.
            Assert.Equal("Bench Press", result.Exercises[0].ExerciseName);

            // Controleer of beide sets terugkomen.
            Assert.Equal(2, result.Exercises[0].Sets.Count);

            // Controleer of de sets op SetNumber gesorteerd zijn.
            Assert.Equal(1, result.Exercises[0].Sets[0].SetNumber);
            Assert.Equal(2, result.Exercises[0].Sets[1].SetNumber);
        }

        [Fact]
        public async Task GetWorkoutSummaryAsync_SessionDoesNotExist_ThrowsNotFoundException()
        {
            // Maak een nieuwe test database aan.
            using var context = CreateDbContext();

            // Geef de test database aan de echte WorkoutsService.
            var workoutsService = new WorkoutsService(context);

            // Probeer een summary op te halen van een session die niet bestaat.
            await Assert.ThrowsAsync<NotFoundException>(() =>
                workoutsService.GetWorkoutSummaryAsync(Guid.NewGuid(), Guid.NewGuid()));
        }

        [Fact]
        public async Task CompleteWorkoutSessionAsync_SessionExists_CompletesSession()
        {
            // Maak een nieuwe test database aan.
            using var context = CreateDbContext();

            // Geef de test database aan de echte WorkoutsService.
            var workoutsService = new WorkoutsService(context);

            // Maak de IDs voor de test aan.
            var userId = Guid.NewGuid();
            var workoutId = Guid.NewGuid();
            var sessionId = Guid.NewGuid();

            // Voeg een workout toe.
            context.Workouts.Add(new Workout
            {
                Id = workoutId,
                UserId = userId,
                Name = "Push Workout",
                Type = WorkoutType.Strength
            });

            // Voeg een onafgeronde workout session toe.
            context.WorkoutSessions.Add(new WorkoutSession
            {
                Id = sessionId,
                WorkoutId = workoutId,
                StartedAt = DateTime.UtcNow,
                LastActivityAt = DateTime.UtcNow,
                IsCompleted = false
            });

            // Sla de test gegevens op.
            await context.SaveChangesAsync();

            // Rond de workout session af.
            await workoutsService.CompleteWorkoutSessionAsync(sessionId, userId);

            // Haal de session opnieuw op.
            var workoutSession = await context.WorkoutSessions
                .FirstAsync(ws => ws.Id == sessionId);

            // De session hoort nu afgerond te zijn.
            Assert.True(workoutSession.IsCompleted);
        }

        [Fact]
        public async Task CompleteWorkoutSessionAsync_SessionDoesNotExist_ThrowsNotFoundException()
        {
            // Maak een nieuwe test database aan.
            using var context = CreateDbContext();

            // Geef de test database aan de echte WorkoutsService.
            var workoutsService = new WorkoutsService(context);

            // Probeer een session af te ronden die niet bestaat.
            await Assert.ThrowsAsync<NotFoundException>(() =>
                workoutsService.CompleteWorkoutSessionAsync(
                    Guid.NewGuid(), Guid.NewGuid()));
        }

        [Fact]
        public async Task DeleteWorkoutSessionAsync_SessionExists_DeletesSession()
        {
            // Maak een nieuwe test database aan.
            using var context = CreateDbContext();

            // Geef de test database aan de echte WorkoutsService.
            var workoutsService = new WorkoutsService(context);

            // Maak de IDs voor de test aan.
            var userId = Guid.NewGuid();
            var workoutId = Guid.NewGuid();
            var sessionId = Guid.NewGuid();

            // Voeg een workout toe.
            context.Workouts.Add(new Workout
            {
                Id = workoutId,
                UserId = userId,
                Name = "Push Workout",
                Type = WorkoutType.Strength
            });

            // Voeg een workout session toe.
            context.WorkoutSessions.Add(new WorkoutSession
            {
                Id = sessionId,
                WorkoutId = workoutId,
                StartedAt = DateTime.UtcNow,
                LastActivityAt = DateTime.UtcNow,
                IsCompleted = false
            });

            // Sla de test gegevens op.
            await context.SaveChangesAsync();

            // Verwijder de workout session.
            await workoutsService.DeleteWorkoutSessionAsync(sessionId, userId);

            // Controleer of de session nog bestaat.
            var exists = await context.WorkoutSessions.AnyAsync(ws => ws.Id == sessionId);

            // De session hoort verwijderd te zijn.
            Assert.False(exists);
        }

        [Fact]
        public async Task DeleteWorkoutSessionAsync_SessionDoesNotExist_ThrowsNotFoundException()
        {
            // Maak een nieuwe test database aan.
            using var context = CreateDbContext();

            // Geef de test database aan de echte WorkoutsService.
            var workoutsService = new WorkoutsService(context);

            // Probeer een session te verwijderen die niet bestaat.
            await Assert.ThrowsAsync<NotFoundException>(() =>
                workoutsService.DeleteWorkoutSessionAsync(
                    Guid.NewGuid(), Guid.NewGuid()));
        }

        [Fact]
        public async Task DeleteInactiveWorkoutSessionsAsync_InactiveSessionExists_DeletesInactiveSession()
        {
            // Maak een nieuwe test database aan.
            using var context = CreateDbContext();

            // Geef de test database aan de echte WorkoutsService.
            var workoutsService = new WorkoutsService(context);

            // Maak een neppe workout aan.
            var workoutId = Guid.NewGuid();

            context.Workouts.Add(new Workout
            {
                Id = workoutId,
                UserId = Guid.NewGuid(),
                Name = "Push Workout",
                Type = WorkoutType.Strength
            });

            // Maak een oude onafgeronde session aan.
            var sessionId = Guid.NewGuid();

            context.WorkoutSessions.Add(new WorkoutSession
            {
                Id = sessionId,
                WorkoutId = workoutId,
                StartedAt = DateTime.UtcNow.AddHours(-2),
                LastActivityAt = DateTime.UtcNow.AddHours(-2),
                IsCompleted = false
            });

            // Sla de test gegevens op.
            await context.SaveChangesAsync();

            // Verwijder de inactieve workout sessions.
            await workoutsService.DeleteInactiveWorkoutSessionsAsync();

            // Controleer of de oude session nog bestaat.
            var exists = await context.WorkoutSessions.AnyAsync(ws => ws.Id == sessionId);

            // De oude onafgeronde session hoort verwijderd te zijn.
            Assert.False(exists);
        }

        [Fact]
        public async Task DeleteInactiveWorkoutSessionsAsync_CompletedSessionIsOld_DoesNotDeleteSession()
        {
            // Maak een nieuwe test database aan.
            using var context = CreateDbContext();

            // Geef de test database aan de echte WorkoutsService.
            var workoutsService = new WorkoutsService(context);

            // Maak een neppe workout aan.
            var workoutId = Guid.NewGuid();

            context.Workouts.Add(new Workout
            {
                Id = workoutId,
                UserId = Guid.NewGuid(),
                Name = "Push Workout",
                Type = WorkoutType.Strength
            });

            // Maak een oude maar afgeronde session aan.
            var sessionId = Guid.NewGuid();

            context.WorkoutSessions.Add(new WorkoutSession
            {
                Id = sessionId,
                WorkoutId = workoutId,
                StartedAt = DateTime.UtcNow.AddHours(-2),
                LastActivityAt = DateTime.UtcNow.AddHours(-2),
                IsCompleted = true
            });

            // Sla de test gegevens op.
            await context.SaveChangesAsync();

            // Voer het verwijderen van inactieve sessions uit.
            await workoutsService.DeleteInactiveWorkoutSessionsAsync();

            // Controleer of de afgeronde session nog bestaat.
            var exists = await context.WorkoutSessions.AnyAsync(ws => ws.Id == sessionId);

            // Een afgeronde session mag niet verwijderd worden.
            Assert.True(exists);
        }

        [Fact]
        public async Task DeleteInactiveWorkoutSessionsAsync_ActiveSessionExists_DoesNotDeleteSession()
        {
            // Maak een nieuwe test database aan.
            using var context = CreateDbContext();

            // Geef de test database aan de echte WorkoutsService.
            var workoutsService = new WorkoutsService(context);

            // Maak een neppe workout aan.
            var workoutId = Guid.NewGuid();

            context.Workouts.Add(new Workout
            {
                Id = workoutId,
                UserId = Guid.NewGuid(),
                Name = "Push Workout",
                Type = WorkoutType.Strength
            });

            // Maak een actieve onafgeronde session aan.
            var sessionId = Guid.NewGuid();

            context.WorkoutSessions.Add(new WorkoutSession
            {
                Id = sessionId,
                WorkoutId = workoutId,
                StartedAt = DateTime.UtcNow,
                LastActivityAt = DateTime.UtcNow,
                IsCompleted = false
            });

            // Sla de test gegevens op.
            await context.SaveChangesAsync();

            // Voer het verwijderen van inactieve sessions uit.
            await workoutsService.DeleteInactiveWorkoutSessionsAsync();

            // Controleer of de actieve session nog bestaat.
            var exists = await context.WorkoutSessions.AnyAsync(ws => ws.Id == sessionId);

            // Een actieve session mag niet verwijderd worden.
            Assert.True(exists);
        }

        [Fact]
        public async Task WorkoutSessionExistsAsync_SessionExists_ReturnsTrue()
        {
            // Maak een nieuwe test database aan.
            using var context = CreateDbContext();

            // Geef de test database aan de echte WorkoutsService.
            var workoutsService = new WorkoutsService(context);

            // Maak de IDs voor de test aan.
            var userId = Guid.NewGuid();
            var workoutId = Guid.NewGuid();
            var sessionId = Guid.NewGuid();

            // Voeg een workout toe.
            context.Workouts.Add(new Workout
            {
                Id = workoutId,
                UserId = userId,
                Name = "Push Workout",
                Type = WorkoutType.Strength
            });

            // Voeg een workout session toe.
            context.WorkoutSessions.Add(new WorkoutSession
            {
                Id = sessionId,
                WorkoutId = workoutId,
                StartedAt = DateTime.UtcNow,
                LastActivityAt = DateTime.UtcNow,
                IsCompleted = false
            });

            // Sla de test gegevens op.
            await context.SaveChangesAsync();

            // Controleer via de service of de session bestaat.
            var result = await workoutsService.WorkoutSessionExistsAsync(sessionId, userId);

            // De session hoort te bestaan.
            Assert.True(result);
        }

        [Fact]
        public async Task WorkoutSessionExistsAsync_SessionDoesNotExist_ReturnsFalse()
        {
            // Maak een nieuwe test database aan.
            using var context = CreateDbContext();

            // Geef de test database aan de echte WorkoutsService.
            var workoutsService = new WorkoutsService(context);

            // Controleer een session die niet bestaat.
            var result = await workoutsService.WorkoutSessionExistsAsync(
                Guid.NewGuid(), Guid.NewGuid());

            // De session hoort niet te bestaan.
            Assert.False(result);
        }

        [Fact]
        public async Task GetWorkoutExerciseHistoryAsync_WorkoutDoesNotExist_ThrowsNotFoundException()
        {
            // Maak een nieuwe test database aan.
            using var context = CreateDbContext();

            // Geef de test database aan de echte WorkoutsService.
            var workoutsService = new WorkoutsService(context);

            // Probeer history op te halen van een workout die niet bestaat.
            await Assert.ThrowsAsync<NotFoundException>(() =>
                workoutsService.GetWorkoutExerciseHistoryAsync(
                    Guid.NewGuid(), Guid.NewGuid()));
        }

        [Fact]
        public async Task GetWorkoutExerciseHistoryAsync_WorkoutHasNoExercises_ReturnsEmptyList()
        {
            // Maak een nieuwe test database aan.
            using var context = CreateDbContext();

            // Geef de test database aan de echte WorkoutsService.
            var workoutsService = new WorkoutsService(context);

            // Maak een neppe gebruiker en workout aan.
            var userId = Guid.NewGuid();
            var workoutId = Guid.NewGuid();

            // Voeg een workout zonder oefeningen toe.
            context.Workouts.Add(new Workout
            {
                Id = workoutId,
                UserId = userId,
                Name = "Push Workout",
                Type = WorkoutType.Strength
            });

            // Sla de workout op.
            await context.SaveChangesAsync();

            // Haal de history op.
            var result = await workoutsService.GetWorkoutExerciseHistoryAsync(
                workoutId, userId);

            // Er hoort geen history te zijn.
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetWorkoutExerciseHistoryAsync_NoCompletedSessions_ReturnsEmptyList()
        {
            // Maak een nieuwe test database aan.
            using var context = CreateDbContext();

            // Geef de test database aan de echte WorkoutsService.
            var workoutsService = new WorkoutsService(context);

            // Maak een neppe gebruiker en workout aan.
            var userId = Guid.NewGuid();
            var workoutId = Guid.NewGuid();

            // Voeg een workout toe.
            context.Workouts.Add(new Workout
            {
                Id = workoutId,
                UserId = userId,
                Name = "Push Workout",
                Type = WorkoutType.Strength
            });

            // Voeg een oefening toe met een verplichte Description.
            context.Exercises.Add(new Exercise
            {
                Id = 1,
                Name = "Bench Press",
                Description = "Borst oefening"
            });

            // Koppel de oefening aan de workout.
            context.WorkoutExercises.Add(new WorkoutExercise
            {
                WorkoutId = workoutId,
                ExerciseId = 1,
                SortOrder = 1
            });

            // Sla de test gegevens op.
            await context.SaveChangesAsync();

            // Haal de history op terwijl er nog geen afgeronde sessions zijn.
            var result = await workoutsService.GetWorkoutExerciseHistoryAsync(
                workoutId, userId);

            // Er hoort nog geen history te zijn.
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetWorkoutExerciseHistoryAsync_CompletedSessionExists_ReturnsBestSet()
        {
            // Maak een nieuwe test database aan.
            using var context = CreateDbContext();

            // Geef de test database aan de echte WorkoutsService.
            var workoutsService = new WorkoutsService(context);

            // Maak de IDs voor de test aan.
            var userId = Guid.NewGuid();
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

            // Voeg een oefening toe met een verplichte Description.
            context.Exercises.Add(new Exercise
            {
                Id = 1,
                Name = "Bench Press",
                Description = "Borst oefening"
            });

            // Koppel de oefening aan de workout.
            context.WorkoutExercises.Add(new WorkoutExercise
            {
                WorkoutId = workoutId,
                ExerciseId = 1,
                SortOrder = 1
            });

            // Voeg een afgeronde workout session toe.
            context.WorkoutSessions.Add(new WorkoutSession
            {
                Id = sessionId,
                WorkoutId = workoutId,
                StartedAt = DateTime.UtcNow.AddDays(-1),
                LastActivityAt = DateTime.UtcNow.AddDays(-1),
                IsCompleted = true
            });

            // Voeg de oefening aan de session toe.
            context.WorkoutSessionExercises.Add(new WorkoutSessionExercise
            {
                Id = sessionExerciseId,
                WorkoutSessionId = sessionId,
                ExerciseId = 1
            });

            // Voeg meerdere sets toe.
            context.WorkoutSets.AddRange(
                new WorkoutSet
                {
                    Id = Guid.NewGuid(),
                    WorkoutSessionExerciseId = sessionExerciseId,
                    SetNumber = 1,
                    Weight = 80,
                    Reps = 10
                },
                new WorkoutSet
                {
                    Id = Guid.NewGuid(),
                    WorkoutSessionExerciseId = sessionExerciseId,
                    SetNumber = 2,
                    Weight = 100,
                    Reps = 5
                },
                new WorkoutSet
                {
                    Id = Guid.NewGuid(),
                    WorkoutSessionExerciseId = sessionExerciseId,
                    SetNumber = 3,
                    Weight = 90,
                    Reps = 8
                });

            // Sla alle test gegevens op.
            await context.SaveChangesAsync();

            // Haal de workout history op.
            var result = await workoutsService.GetWorkoutExerciseHistoryAsync(
                workoutId, userId);

            // Controleer of er history voor één oefening is.
            Assert.Single(result);

            // Controleer of de juiste oefening terugkomt.
            Assert.Equal(1, result[0].ExerciseId);
            Assert.Equal("Bench Press", result[0].ExerciseName);

            // Controleer of er één history record is.
            Assert.Single(result[0].History);

            // Controleer of de set met het hoogste gewicht gekozen is.
            Assert.Equal(100, result[0].History[0].Weight);
            Assert.Equal(5, result[0].History[0].Reps);
        }
    }
}