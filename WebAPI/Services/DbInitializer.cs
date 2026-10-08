using Microsoft.EntityFrameworkCore;
using WebAPI.Models.Exercises;

namespace WebAPI.Services
{
    public static class DbInitializer
    {
        public static async Task InitializeAsync(ApplicationDbContext context)
        {
            await context.Database.MigrateAsync();

            // Muscle groups
            if (!await context.MuscleGroups.AnyAsync())
            {
                var muscleGroups = new List<MuscleGroup>
                {
                    new MuscleGroup
                    {
                        Name = "Chest",
                        Description = "Chest muscles",
                        ImageUrl = "chestmuscle.png",
                        SortOrder = 1
                    },
                    new MuscleGroup
                    {
                        Name = "Back",
                        Description = "Back muscles",
                        ImageUrl = "backmuscle.png",
                        SortOrder =  8
                    },
                    new MuscleGroup
                    {
                        Name = "Shoulders",
                        Description = "Shoulder muscles",
                        ImageUrl = "shouldersmuscle.png",
                        SortOrder = 2
                    },
                    new MuscleGroup
                    {
                        Name = "Biceps",
                        Description = "Biceps muscles",
                        ImageUrl = "bicepmuscle.png",
                        SortOrder = 3
                    },
                    new MuscleGroup
                    {
                        Name = "Forearms",
                        Description = "Forearms muscles",
                        ImageUrl = "forearmsmuscle.png",
                        SortOrder = 4
                    },
                    new MuscleGroup
                    {
                        Name = "Triceps",
                        Description = "Triceps muscles",
                        ImageUrl = "tricepmuscle.png",
                        SortOrder = 7

                    },
                    new MuscleGroup
                    {
                        Name = "Legs",
                        Description = "Leg muscles",
                        ImageUrl = "legs.png",
                        SortOrder = 9
                    },
                    new MuscleGroup
                    {
                        Name = "Abs",
                        Description = "Abdominal muscles",
                        ImageUrl = "absmuscle.png",
                        SortOrder = 6
                    },
                    new MuscleGroup
                    {
                        Name = "Trapezius",
                        Description = "Trapezius muscles",
                        ImageUrl = "neck.png",
                        SortOrder = 5
                    },
                    new MuscleGroup
                    {
                        Name = "Calves",
                        Description = "Calf muscles",
                        ImageUrl = "calfs.png",
                        SortOrder = 10
                    },

                };

                context.MuscleGroups.AddRange(muscleGroups);
                await context.SaveChangesAsync();
            }

            // Exercises
            if (!await context.Exercises.AnyAsync())
            {
                var exercises = new List<Exercise>
                {
                    new Exercise
                    {
                        Name = "Bench Press",
                        Description = "The bench press is a strength exercise in which you lie on a bench and push a barbell upward from your chest. The exercise primarily targets the chest muscles, but also works the triceps and shoulders. It is a popular exercise for improving upper-body strength.\r\n",
                        ImageUrl = "benchpress.png"
                    },
                    new Exercise
                    {
                        Name = "Incline Bench Press",
                        Description = "The incline bench press is a variation of the bench press in which the bench is positioned at an incline. This places your upper body higher than your legs. The exercise primarily targets the upper part of the chest muscles, with assistance from the shoulders and triceps.\r\n",
                        ImageUrl = "inclinebenchpress.png"
                    },
                    new Exercise
                    {
                        Name = "Decline Bench Press",
                        Description = "The decline bench press is a variation of the bench press in which the bench is positioned at a downward angle. This places your upper body lower than your legs. The exercise primarily targets the lower part of the chest muscles, with assistance from the triceps and shoulders.\r\n",
                        ImageUrl = "declinebenchpress.png"
                    },
                    new Exercise
                    {
                        Name = "Push-Up",
                        Description = "The push-up is a bodyweight exercise in which you place your hands on the floor slightly wider than shoulder-width apart while keeping your body in a straight line. During the exercise, you bend your elbows and lower your chest toward the floor, then push yourself back up to the starting position. The exercise primarily targets the chest and triceps, with assistance from the shoulders and core. \r\n",
                        ImageUrl = "pushup.png"
                    },
                    new Exercise
                    {
                        Name = "Incline Push-Up",
                        Description = "The incline push-up is a push-up variation performed with the hands on an elevated surface, such as a bench. Keep your body straight, lower your chest toward the surface, and push back up. It primarily targets the chest and triceps, with assistance from the shoulders and core.",
                        ImageUrl = "inclinepushup.png"
                    },
                    new Exercise
                    {
                        Name = "Cable Fly",
                        Description = "The standing cable fly is a chest exercise in which you stand between two cable machines and hold a handle in each hand with your arms extended out to the sides. During the exercise, you bring your hands together in front of your chest while keeping a slight bend in your elbows, then slowly return to the starting position. The exercise primarily targets the chest, with assistance from the shoulders and triceps. \r\n",
                        ImageUrl = "cablefly.png"
                    },
                    new Exercise
                    {
                        Name = "Underhand Cable Fly",
                        Description = "The underhand cable fly is a chest exercise in which you stand between two low cable pulleys and hold a handle in each hand with your palms facing upward. During the exercise, you bring your hands upward and together in front of your chest while keeping a slight bend in your elbows, then slowly return to the starting position. The exercise primarily targets the upper chest, with assistance from the shoulders and biceps. \r\n",
                        ImageUrl = "underhandcablefly.png"
                    },
                    new Exercise
                    {
                        Name = "Decline Push-Up",
                        Description = "The decline push-up is a push-up variation performed with the feet elevated on a bench and the hands on the floor. Lower your chest toward the floor and push back up while keeping your body straight. It primarily targets the upper chest and triceps, with assistance from the shoulders and core.",
                        ImageUrl = "declinepushup.png"
                    },
                    new Exercise
                    {
                        Name = "Dumbbell Press",
                        Description = "The dumbbell press is a chest exercise in which you lie on a flat bench and hold a dumbbell in each hand at chest level. During the exercise, you press the dumbbells upward until your arms are extended, then slowly lower them back to the starting position. The exercise primarily targets the chest, with assistance from the triceps and shoulders. \r\n",
                        ImageUrl = "dumbellpress.png"
                    },
                    new Exercise
                    {
                        Name = "Incline Dumbbell Press",
                        Description = "The incline dumbbell press is a chest exercise in which you lie on an inclined bench and hold a dumbbell in each hand at chest level. During the exercise, you press the dumbbells upward until your arms are extended, then slowly lower them back to the starting position. The exercise primarily targets the upper chest, with assistance from the triceps and shoulders. \r\n",
                        ImageUrl = "inclinedumbellpress.png"
                    },
                    new Exercise
                    {
                        Name = "Chest Fly Machine",
                        Description = "The chest fly machine is a chest exercise in which you sit on the machine with your back against the pad and place your arms against the handles or pads. During the exercise, you bring your arms together in front of your chest, then slowly return them to the starting position. The exercise primarily targets the chest, with assistance from the shoulders. \r\n",
                        ImageUrl = "chestflymachine.png"
                    },
                    new Exercise
                    {
                        Name = "Hammer Curl",
                        Description = "The hammer curl is a variation of the biceps curl in which you hold the dumbbells with a neutral grip, with your palms facing each other. During the exercise, you bend your elbows and bring the dumbbells upward toward your shoulders. The exercise primarily targets the biceps and brachialis, with assistance from the forearms.\r\n",
                        ImageUrl = "hammercurls.png"
                    },
                    new Exercise
                    {
                        Name = "Dumbbell Curl",
                        Description = "The dumbbell bicep curl is an exercise in which you hold a dumbbell in each hand with your palms facing forward. During the exercise, you bend your elbows and bring the dumbbells upward toward your shoulders while keeping your upper arms still. The exercise primarily targets the biceps, with assistance from the brachialis and forearms. \r\n",
                        ImageUrl = "dumbellbicepcurl.png"
                    },
                    new Exercise
                    {
                        Name = "Barbell Curl",
                        Description = "The barbell curl is a biceps exercise in which you hold a barbell with an underhand grip, with your palms facing upward. During the exercise, you bend your elbows and bring the barbell upward toward your chest while keeping your upper arms still. The exercise primarily targets the biceps, with assistance from the brachialis and forearms. \r\n",
                        ImageUrl = "halterbicepcurl.png"
                    },
                    new Exercise
                    {
                        Name = "Seated Cable Row",
                        Description = "The seated cable row is a back exercise in which you sit at a cable machine and hold the handle with both hands while keeping your back straight. During the exercise, you pull the handle toward your torso by bending your elbows and squeezing your shoulder blades together, then slowly return to the starting position. The exercise primarily targets the upper and middle back, with assistance from the biceps and forearms. \r\n",
                        ImageUrl = "seatedcablerow.png"
                    },
                    new Exercise
                    {
                        Name = "Barbell Row",
                        Description = "The barbell row is a back exercise in which you hold a barbell with both hands and bend forward at the hips while keeping your back straight.During the exercise, you pull the barbell toward your lower chest or upper abdomen by bending your elbows and squeezing your shoulder blades together, then slowly lower it back to the starting position.The exercise primarily targets the upper and middle back, with assistance from the biceps, forearms, and lower back. \r\n",
                        ImageUrl = "barbellrow.png"
                    },
                    new Exercise
                    {
                        Name = "Underhand Barbell Row",
                        Description = "The underhand barbell row is a back exercise in which you hold a barbell with an underhand grip, with your palms facing forward, and bend forward at the hips while keeping your back straight. During the exercise, you pull the barbell toward your lower abdomen by bending your elbows and squeezing your shoulder blades together, then slowly lower it back to the starting position. The exercise primarily targets the lats and middle back, with assistance from the biceps, forearms, and lower back. \r\n",
                        ImageUrl = "underhandbarbellrow.png"
                    },
                    new Exercise
                    {
                        Name = "Lat Pulldown",
                        Description = "The lat pulldown is a back exercise in which you sit at a cable machine and hold the bar with a wide overhand grip. During the exercise, you pull the bar down toward your upper chest by bending your elbows and squeezing your shoulder blades together, then slowly return the bar to the starting position. The exercise primarily targets the lats, with assistance from the biceps, forearms, and upper back. \r\n",
                        ImageUrl = "latpulldown.png"
                    },
                    new Exercise
                    {
                        Name = "Pull-Up",
                        Description = "The pull-up is a bodyweight back exercise in which you hang from a bar with an overhand grip, with your hands slightly wider than shoulder-width apart. During the exercise, you pull your body upward until your chin is above the bar, then slowly lower yourself back to the starting position. The exercise primarily targets the lats and upper back, with assistance from the biceps, forearms, and core. \r\n",
                        ImageUrl = "pullup.png"
                    },
                    new Exercise
                    {
                        Name = "Cable Pushdown",
                        Description = "The triceps pushdown is an arm exercise in which you stand in front of a cable machine and hold the bar or rope with both hands. During the exercise, you keep your elbows close to your body and push the handle downward until your arms are fully extended, then slowly return to the starting position. The exercise primarily targets the triceps, with assistance from the forearms. \r\n",
                        ImageUrl = "tricepcablepushdown.png"
                    },
                    new Exercise
                    {
                        Name = "Rope Triceps Pushdown",
                        Description = "The rope triceps pushdown is performed standing at a high cable pulley while holding a rope attachment. Keep your elbows close to your sides, extend your arms downward, and separate the rope ends slightly at the bottom before returning with control. It primarily targets the triceps.",
                        ImageUrl = "ropetriceppulldown.png"
                    },
                    new Exercise
                    {
                        Name = "Triceps Dips",
                        Description = "The triceps dip is a bodyweight exercise in which you support yourself on parallel bars with your arms extended. During the exercise, you bend your elbows and lower your body while keeping your torso relatively upright, then push yourself back up to the starting position. The exercise primarily targets the triceps, with assistance from the chest and shoulders. \r\n",
                        ImageUrl = "tricepsdips.png"
                    },
                    new Exercise
                    {
                        Name = "Overhead Dumbbell Triceps Extension",
                        Description = "The overhead dumbbell triceps extension is an arm exercise in which you hold a dumbbell above your head with both hands and your arms extended. During the exercise, you bend your elbows to lower the dumbbell behind your head, then extend your arms to return to the starting position. The exercise primarily targets the triceps, especially the long head, with assistance from the shoulders and forearms. \r\n",
                        ImageUrl = "overheaddumbellextension.png"
                    },
                    new Exercise
                    {
                        Name = "Push Press",
                        Description = "The push press is a shoulder exercise in which you hold a barbell at shoulder level with an overhand grip. During the exercise, you slightly bend your knees and use your legs to help drive the barbell overhead until your arms are fully extended, then slowly lower it back to the starting position. The exercise primarily targets the shoulders and triceps, with assistance from the legs and core. \r\n",
                        ImageUrl = "shoulderpushpress.png"
                    },
                    new Exercise
                    {
                        Name = "Barbell Shrugs",
                        Description = "The barbell shrug is a shoulder and upper back exercise in which you hold a barbell in front of your body with an overhand grip. During the exercise, you raise your shoulders upward toward your ears while keeping your arms straight, then slowly lower them back to the starting position. The exercise primarily targets the trapezius muscles, with assistance from the forearms and upper back. \r\n",
                        ImageUrl = "shrugs.png"
                    },
                    new Exercise
                    {
                        Name = "Dumbbell Lateral Raises",
                        Description = "The dumbbell lateral raise is a shoulder exercise in which you stand with a dumbbell in each hand at your sides. During the exercise, you raise your arms out to the sides until they reach approximately shoulder height, while keeping a slight bend in your elbows, then slowly lower the dumbbells back to the starting position. The exercise primarily targets the side deltoids, with assistance from the trapezius muscles. \r\n",
                        ImageUrl = "lateralraises.png"
                    },
                    new Exercise
                    {
                        Name = "Barbell Squat",
                        Description = "The barbell squat is a lower-body exercise in which you place a barbell across your upper back and stand with your feet approximately shoulder-width apart. During the exercise, you bend your knees and hips to lower your body, then push through your feet to return to the starting position. The exercise primarily targets the quadriceps and glutes, with assistance from the hamstrings, calves, and core. \r\n",
                        ImageUrl = "barbellsquat.png"
                    },
                    new Exercise
                    {
                        Name = "Leg Press Machine",
                        Description = "The leg press is a lower-body exercise in which you sit on the machine with your back against the pad and place your feet on the platform approximately shoulder-width apart. During the exercise, you bend your knees to lower the platform toward your body, then push through your feet to extend your legs back to the starting position. The exercise primarily targets the quadriceps and glutes, with assistance from the hamstrings and calves. \r\n",
                        ImageUrl = "legpressmachine.png"
                    },
                    new Exercise
                    {
                        Name = "Bulgarian Split Squat",
                        Description = "The Bulgarian split squat is a lower-body exercise in which you place one foot behind you on an elevated surface while keeping your front foot firmly on the floor. During the exercise, you bend your front knee and lower your body toward the floor, then push through your front foot to return to the starting position. The exercise primarily targets the quadriceps and glutes, with assistance from the hamstrings and core. \r\n",
                        ImageUrl = "bulgariansquat.png"
                    },
                    new Exercise
                    {
                        Name = "Leg Extension Machine",
                        Description = "The leg extension is a lower-body exercise in which you sit on the machine with your back against the pad and place your lower legs behind the padded bar. During the exercise, you extend your knees to raise the weight until your legs are almost straight, then slowly lower the weight back to the starting position. The exercise primarily targets the quadriceps. \r\n",
                        ImageUrl = "legextension.png"
                    },
                    new Exercise
                    {
                        Name = "Romanian Deadlift",
                        Description = "The Romanian deadlift is a lower-body exercise in which you hold a barbell in front of your thighs with an overhand grip. During the exercise, you push your hips backward and lower the barbell along your legs while keeping your back straight and your knees slightly bent, then drive your hips forward to return to the starting position. The exercise primarily targets the hamstrings and glutes, with assistance from the lower back and core. \r\n",
                        ImageUrl = "romaniondeadlift.png"
                    },
                    new Exercise
                    {
                        Name = "Calf Raises",
                        Description = "The calf raise is a lower-leg exercise that can be performed with or without additional weight. During the exercise, you raise your heels off the floor by pushing through the balls of your feet, then slowly lower your heels back to the starting position. The exercise primarily targets the calf muscles, especially the gastrocnemius and soleus. \r\n",
                        ImageUrl = "calfraises.png"
                    },



                }
            ;

                context.Exercises.AddRange(exercises);
                await context.SaveChangesAsync();

                // Muscle group relations
                var muscleGroups = await context.MuscleGroups
                    .ToDictionaryAsync(x => x.Name);

                var savedExercises = await context.Exercises
                    .ToDictionaryAsync(x => x.Name);

                var relations = new List<ExerciseMuscleGroup>
                {
                    new()
                    {
                        ExerciseId = savedExercises["Bench Press"].Id,
                        MuscleGroupId = muscleGroups["Chest"].Id
                    },
                    new()
                    {
                        ExerciseId = savedExercises["Incline Bench Press"].Id,
                        MuscleGroupId = muscleGroups["Chest"].Id
                    },
                    new()
                    {
                        ExerciseId = savedExercises["Decline Bench Press"].Id,
                        MuscleGroupId = muscleGroups["Chest"].Id
                    },
                    new()
                    {
                        ExerciseId = savedExercises["Push-Up"].Id,
                        MuscleGroupId = muscleGroups["Chest"].Id
                    },
                    new()
                    {
                        ExerciseId = savedExercises["Incline Push-Up"].Id,
                        MuscleGroupId = muscleGroups["Chest"].Id
                    },
                    new()
                    {
                        ExerciseId = savedExercises["Decline Push-Up"].Id,
                        MuscleGroupId = muscleGroups["Chest"].Id
                    },
                    new()
                    {
                        ExerciseId = savedExercises["Cable Fly"].Id,
                        MuscleGroupId = muscleGroups["Chest"].Id
                    },
                    new()
                    {
                        ExerciseId = savedExercises["Underhand Cable Fly"].Id,
                        MuscleGroupId = muscleGroups["Chest"].Id
                    },
                    new()
                    {
                        ExerciseId = savedExercises["Dumbbell Press"].Id,
                        MuscleGroupId = muscleGroups["Chest"].Id
                    },
                    new()
                    {
                        ExerciseId = savedExercises["Incline Dumbbell Press"].Id,
                        MuscleGroupId = muscleGroups["Chest"].Id
                    },
                    new()
                    {
                        ExerciseId = savedExercises["Chest Fly Machine"].Id,
                        MuscleGroupId = muscleGroups["Chest"].Id
                    },
                    new()
                    {
                        ExerciseId = savedExercises["Hammer Curl"].Id,
                        MuscleGroupId = muscleGroups["Biceps"].Id
                    },
                    new()
                    {
                        ExerciseId = savedExercises["Dumbbell Curl"].Id,
                        MuscleGroupId = muscleGroups["Biceps"].Id
                    },
                    new()
                    {
                        ExerciseId = savedExercises["Barbell Curl"].Id,
                        MuscleGroupId = muscleGroups["Biceps"].Id
                    },
                    new()
                    {
                        ExerciseId = savedExercises["Seated Cable Row"].Id,
                        MuscleGroupId = muscleGroups["Back"].Id
                    },
                    new()
                    {
                        ExerciseId = savedExercises["Barbell Row"].Id,
                        MuscleGroupId = muscleGroups["Back"].Id
                    },
                    new()
                    {
                        ExerciseId = savedExercises["Underhand Barbell Row"].Id,
                        MuscleGroupId = muscleGroups["Back"].Id
                    },
                    new()
                    {
                        ExerciseId = savedExercises["Lat Pulldown"].Id,
                        MuscleGroupId = muscleGroups["Back"].Id
                    },
                    new()
                    {
                        ExerciseId = savedExercises["Pull-Up"].Id,
                        MuscleGroupId = muscleGroups["Back"].Id
                    },
                    new()
                    {
                        ExerciseId = savedExercises["Cable Pushdown"].Id,
                        MuscleGroupId = muscleGroups["Triceps"].Id
                    },
                    new()
                    {
                        ExerciseId = savedExercises["Rope Triceps Pushdown"].Id,
                        MuscleGroupId = muscleGroups["Triceps"].Id
                    },
                    new()
                    {
                        ExerciseId = savedExercises["Triceps Dips"].Id,
                        MuscleGroupId = muscleGroups["Triceps"].Id
                    },
                    new()
                    {
                        ExerciseId = savedExercises["Overhead Dumbbell Triceps Extension"].Id,
                        MuscleGroupId = muscleGroups["Triceps"].Id
                    },
                    new()
                    {
                        ExerciseId = savedExercises["Push Press"].Id,
                        MuscleGroupId = muscleGroups["Shoulders"].Id
                    },
                    new()
                    {
                        ExerciseId = savedExercises["Barbell Shrugs"].Id,
                        MuscleGroupId = muscleGroups["Trapezius"].Id
                    },
                    new()
                    {
                        ExerciseId = savedExercises["Dumbbell Lateral Raises"].Id,
                        MuscleGroupId = muscleGroups["Shoulders"].Id
                    },
                    new()
                    {
                        ExerciseId = savedExercises["Barbell Squat"].Id,
                        MuscleGroupId = muscleGroups["Legs"].Id
                    },
                    new()
                    {
                        ExerciseId = savedExercises["Leg Press Machine"].Id,
                        MuscleGroupId = muscleGroups["Legs"].Id
                    },
                    new()
                    {
                        ExerciseId = savedExercises["Bulgarian Split Squat"].Id,
                        MuscleGroupId = muscleGroups["Legs"].Id
                    },
                    new()
                    {
                        ExerciseId = savedExercises["Leg Extension Machine"].Id,
                        MuscleGroupId = muscleGroups["Legs"].Id
                    },
                    new()
                    {
                        ExerciseId = savedExercises["Romanian Deadlift"].Id,
                        MuscleGroupId = muscleGroups["Legs"].Id
                    },
                    new()
                    {
                        ExerciseId = savedExercises["Calf Raises"].Id,
                        MuscleGroupId = muscleGroups["Calves"].Id
                    },


                };

                context.ExerciseMuscleGroups.AddRange(relations);
                await context.SaveChangesAsync();
            }
        }
    }
}