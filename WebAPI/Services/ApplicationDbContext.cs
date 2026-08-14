using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WebAPI.Models;
using WebAPI.Models.Auth;
using WebAPI.Models.Exercises;
using WebAPI.Models.Workout;

namespace WebAPI.Services
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<MuscleGroup> MuscleGroups { get; set; }

        public DbSet<Exercise> Exercises { get; set; }

        public DbSet<ExerciseMuscleGroup> ExerciseMuscleGroups { get; set; }

        public DbSet<Workout> Workouts { get; set; }

        public DbSet<WorkoutExercise> WorkoutExercises { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<ExerciseMuscleGroup>()
                .HasKey(x => new { x.ExerciseId, x.MuscleGroupId });

            builder.Entity<ExerciseMuscleGroup>()
                .HasOne(x => x.Exercise)
                .WithMany(e => e.ExerciseMuscleGroups)
                .HasForeignKey(x => x.ExerciseId);

            builder.Entity<ExerciseMuscleGroup>()
                .HasOne(x => x.MuscleGroup)
                .WithMany(m => m.ExerciseMuscleGroups)
                .HasForeignKey(x => x.MuscleGroupId);

            builder.Entity<WorkoutExercise>()
                .HasKey(ew => new { ew.ExerciseId, ew.WorkoutId });

            builder.Entity<WorkoutExercise>()
                .HasOne(ew => ew.Exercise)
                .WithMany(e => e.WorkoutExercise)
                .HasForeignKey(ew => ew.ExerciseId);

            builder.Entity<WorkoutExercise>()
                .HasOne(ew => ew.Workout)
                .WithMany(w => w.WorkoutExercise)
                .HasForeignKey(ew => ew.WorkoutId);
        }
    }
}
