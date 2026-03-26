using XerSize.Models;
using XerSize.Services.Interfaces;

namespace XerSize.Services;

public sealed class InMemoryExerciseCatalogService : IExerciseCatalogService
{
    private readonly List<ExerciseCatalogItem> _catalog = new()
    {
        new ExerciseCatalogItem
        {
            Id = "Bench_Press",
            Name = "Bench Press",
            Force = "Push",
            BodyCategory = "Upper Body",
            Mechanic = "Compound",
            Equipment = "Barbell",
            PrimaryMuscles = new List<string> { "Sternal Head", "Clavicular Head" },
            SecondaryMuscles = new List<string> { "Anterior Deltoid", "Lateral Head" },
            PrimaryMuscleCategories = new List<string> { "Chest" },
            SecondaryMuscleCategories = new List<string> { "Shoulders", "Triceps" },
            LimbInvolvement = "Bilateral",
            MovementPattern = "Horizontal Push"
        },
        new ExerciseCatalogItem
        {
            Id = "Incline_Dumbbell_Press",
            Name = "Incline Dumbbell Press",
            Force = "Push",
            BodyCategory = "Upper Body",
            Mechanic = "Compound",
            Equipment = "Dumbbell",
            PrimaryMuscles = new List<string> { "Clavicular Head", "Anterior Deltoid" },
            SecondaryMuscles = new List<string> { "Lateral Head" },
            PrimaryMuscleCategories = new List<string> { "Chest", "Shoulders" },
            SecondaryMuscleCategories = new List<string> { "Triceps" },
            LimbInvolvement = "Bilateral",
            MovementPattern = "Incline Push"
        },
        new ExerciseCatalogItem
        {
            Id = "Arnold_Press",
            Name = "Arnold Press",
            Force = "Push",
            BodyCategory = "Upper Body",
            Mechanic = "Compound",
            Equipment = "Dumbbell",
            PrimaryMuscles = new List<string> { "Anterior Deltoid", "Lateral Deltoid" },
            SecondaryMuscles = new List<string> { "Lateral Head", "Upper Trapezius" },
            PrimaryMuscleCategories = new List<string> { "Shoulders" },
            SecondaryMuscleCategories = new List<string> { "Triceps", "Upper Back" },
            LimbInvolvement = "Bilateral",
            MovementPattern = "Vertical Push"
        },
        new ExerciseCatalogItem
        {
            Id = "Cable_Lateral_Raise",
            Name = "Cable Lateral Raise",
            Force = "Push",
            BodyCategory = "Upper Body",
            Mechanic = "Isolation",
            Equipment = "Cable",
            PrimaryMuscles = new List<string> { "Lateral Deltoid" },
            SecondaryMuscles = new List<string> { "Supraspinatus" },
            PrimaryMuscleCategories = new List<string> { "Shoulders" },
            SecondaryMuscleCategories = new List<string> { "Shoulders" },
            LimbInvolvement = "Unilateral",
            MovementPattern = "Shoulder Abduction"
        },
        new ExerciseCatalogItem
        {
            Id = "Triceps_Pushdown",
            Name = "Triceps Pushdown",
            Force = "Push",
            BodyCategory = "Upper Body",
            Mechanic = "Isolation",
            Equipment = "Cable",
            PrimaryMuscles = new List<string> { "Lateral Head", "Long Head" },
            SecondaryMuscles = new List<string>(),
            PrimaryMuscleCategories = new List<string> { "Triceps" },
            SecondaryMuscleCategories = new List<string>(),
            LimbInvolvement = "Bilateral",
            MovementPattern = "Elbow Extension"
        },
        new ExerciseCatalogItem
        {
            Id = "Pull_Up",
            Name = "Pull Up",
            Force = "Pull",
            BodyCategory = "Upper Body",
            Mechanic = "Compound",
            Equipment = "Calisthenics",
            PrimaryMuscles = new List<string> { "Latissimus Dorsi" },
            SecondaryMuscles = new List<string> { "Biceps Brachii", "Rhomboids" },
            PrimaryMuscleCategories = new List<string> { "Back" },
            SecondaryMuscleCategories = new List<string> { "Biceps", "Upper Back" },
            LimbInvolvement = "Bilateral",
            MovementPattern = "Vertical Pull"
        },
        new ExerciseCatalogItem
        {
            Id = "Lat_Pulldown",
            Name = "Lat Pulldown",
            Force = "Pull",
            BodyCategory = "Upper Body",
            Mechanic = "Compound",
            Equipment = "Cable",
            PrimaryMuscles = new List<string> { "Latissimus Dorsi" },
            SecondaryMuscles = new List<string> { "Biceps Brachii" },
            PrimaryMuscleCategories = new List<string> { "Back" },
            SecondaryMuscleCategories = new List<string> { "Biceps" },
            LimbInvolvement = "Bilateral",
            MovementPattern = "Vertical Pull"
        },
        new ExerciseCatalogItem
        {
            Id = "Seated_Row",
            Name = "Seated Row",
            Force = "Pull",
            BodyCategory = "Upper Body",
            Mechanic = "Compound",
            Equipment = "Machine",
            PrimaryMuscles = new List<string> { "Latissimus Dorsi", "Rhomboids" },
            SecondaryMuscles = new List<string> { "Biceps Brachii" },
            PrimaryMuscleCategories = new List<string> { "Back", "Upper Back" },
            SecondaryMuscleCategories = new List<string> { "Biceps" },
            LimbInvolvement = "Bilateral",
            MovementPattern = "Horizontal Pull"
        },
        new ExerciseCatalogItem
        {
            Id = "Hammer_Curl",
            Name = "Hammer Curl",
            Force = "Pull",
            BodyCategory = "Upper Body",
            Mechanic = "Isolation",
            Equipment = "Dumbbell",
            PrimaryMuscles = new List<string> { "Brachialis", "Brachioradialis" },
            SecondaryMuscles = new List<string> { "Biceps Brachii" },
            PrimaryMuscleCategories = new List<string> { "Biceps", "Forearms" },
            SecondaryMuscleCategories = new List<string> { "Biceps" },
            LimbInvolvement = "Alternating",
            MovementPattern = "Elbow Flexion"
        },
        new ExerciseCatalogItem
        {
            Id = "Face_Pull",
            Name = "Face Pull",
            Force = "Pull",
            BodyCategory = "Upper Body",
            Mechanic = "Isolation",
            Equipment = "Cable",
            PrimaryMuscles = new List<string> { "Posterior Deltoid", "Rhomboids" },
            SecondaryMuscles = new List<string> { "Upper Trapezius" },
            PrimaryMuscleCategories = new List<string> { "Shoulders", "Upper Back" },
            SecondaryMuscleCategories = new List<string> { "Upper Back" },
            LimbInvolvement = "Bilateral",
            MovementPattern = "Horizontal Pull"
        },
        new ExerciseCatalogItem
        {
            Id = "Leg_Press",
            Name = "Leg Press",
            Force = "Push",
            BodyCategory = "Lower Body",
            Mechanic = "Compound",
            Equipment = "Machine",
            PrimaryMuscles = new List<string> { "Quadriceps" },
            SecondaryMuscles = new List<string> { "Gluteus Maximus" },
            PrimaryMuscleCategories = new List<string> { "Quads" },
            SecondaryMuscleCategories = new List<string> { "Glutes" },
            LimbInvolvement = "Bilateral",
            MovementPattern = "Squat"
        },
        new ExerciseCatalogItem
        {
            Id = "Romanian_Deadlift",
            Name = "Romanian Deadlift",
            Force = "Pull",
            BodyCategory = "Lower Body",
            Mechanic = "Compound",
            Equipment = "Barbell",
            PrimaryMuscles = new List<string> { "Hamstrings", "Gluteus Maximus" },
            SecondaryMuscles = new List<string> { "Erector Spinae" },
            PrimaryMuscleCategories = new List<string> { "Hamstrings", "Glutes" },
            SecondaryMuscleCategories = new List<string> { "Lower Back" },
            LimbInvolvement = "Bilateral",
            MovementPattern = "Hinge"
        },
        new ExerciseCatalogItem
        {
            Id = "Leg_Curl",
            Name = "Leg Curl",
            Force = "Pull",
            BodyCategory = "Lower Body",
            Mechanic = "Isolation",
            Equipment = "Machine",
            PrimaryMuscles = new List<string> { "Hamstrings" },
            SecondaryMuscles = new List<string>(),
            PrimaryMuscleCategories = new List<string> { "Hamstrings" },
            SecondaryMuscleCategories = new List<string>(),
            LimbInvolvement = "Bilateral",
            MovementPattern = "Knee Flexion"
        },
        new ExerciseCatalogItem
        {
            Id = "Standing_Calf_Raise",
            Name = "Standing Calf Raise",
            Force = "Push",
            BodyCategory = "Lower Body",
            Mechanic = "Isolation",
            Equipment = "Machine",
            PrimaryMuscles = new List<string> { "Gastrocnemius" },
            SecondaryMuscles = new List<string> { "Soleus" },
            PrimaryMuscleCategories = new List<string> { "Calves" },
            SecondaryMuscleCategories = new List<string> { "Calves" },
            LimbInvolvement = "Bilateral",
            MovementPattern = "Ankle Plantarflexion"
        },
        new ExerciseCatalogItem
        {
            Id = "Hack_Squat",
            Name = "Hack Squat",
            Force = "Push",
            BodyCategory = "Lower Body",
            Mechanic = "Compound",
            Equipment = "Machine",
            PrimaryMuscles = new List<string> { "Quadriceps" },
            SecondaryMuscles = new List<string> { "Gluteus Maximus" },
            PrimaryMuscleCategories = new List<string> { "Quads" },
            SecondaryMuscleCategories = new List<string> { "Glutes" },
            LimbInvolvement = "Bilateral",
            MovementPattern = "Squat"
        },
        new ExerciseCatalogItem
        {
            Id = "Leg_Extension",
            Name = "Leg Extension",
            Force = "Push",
            BodyCategory = "Lower Body",
            Mechanic = "Isolation",
            Equipment = "Machine",
            PrimaryMuscles = new List<string> { "Quadriceps" },
            SecondaryMuscles = new List<string>(),
            PrimaryMuscleCategories = new List<string> { "Quads" },
            SecondaryMuscleCategories = new List<string>(),
            LimbInvolvement = "Bilateral",
            MovementPattern = "Knee Extension"
        },
        new ExerciseCatalogItem
        {
            Id = "Plank",
            Name = "Plank",
            Force = "Static",
            BodyCategory = "Core",
            Mechanic = "Isolation",
            Equipment = "Bodyweight",
            PrimaryMuscles = new List<string> { "Rectus Abdominis", "Transverse Abdominis" },
            SecondaryMuscles = new List<string> { "Obliques" },
            PrimaryMuscleCategories = new List<string> { "Core" },
            SecondaryMuscleCategories = new List<string> { "Core" },
            LimbInvolvement = "Bilateral",
            MovementPattern = "Anti-Extension"
        }
    };

    public Task<IReadOnlyList<ExerciseCatalogItem>> GetAllAsync()
    {
        return Task.FromResult((IReadOnlyList<ExerciseCatalogItem>)_catalog);
    }

    public Task<ExerciseCatalogItem?> GetByIdAsync(string id)
    {
        ExerciseCatalogItem? item = _catalog.FirstOrDefault(x => x.Id == id);
        return Task.FromResult(item);
    }
}