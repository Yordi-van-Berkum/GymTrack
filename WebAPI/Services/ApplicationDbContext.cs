using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;
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
        public DbSet<WorkoutSession> WorkoutSessions { get; set; }
        public DbSet<WorkoutSessionExercise> WorkoutSessionExercises { get; set; }
        public DbSet<WorkoutSet> WorkoutSets { get; set; }

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

            builder.Entity<WorkoutSession>()
                .HasOne(ws => ws.Workout)
                .WithMany()
                .HasForeignKey(ws => ws.WorkoutId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<WorkoutSessionExercise>()
                .HasOne(wse => wse.WorkoutSession)
                .WithMany(ws => ws.Exercises)
                .HasForeignKey(wse => wse.WorkoutSessionId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<WorkoutSessionExercise>()
                .HasOne(wse => wse.Exercise)
                .WithMany()
                .HasForeignKey(wse => wse.ExerciseId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<WorkoutSet>()
                .HasOne(ws => ws.WorkoutSessionExercise)
                .WithMany(wse => wse.Sets)
                .HasForeignKey(ws => ws.WorkoutSessionExerciseId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
