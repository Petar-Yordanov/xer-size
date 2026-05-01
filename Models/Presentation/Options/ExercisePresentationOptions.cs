using XerSize.Models.Definitions;

namespace XerSize.Models.Presentation.Options;

public static class ExercisePresentationOptions
{
    public const string AllFilterLabel = "All";

    public static IReadOnlyList<ExerciseForce> Forces { get; } =
    [
        ExerciseForce.Cardio,
        ExerciseForce.Legs,
        ExerciseForce.Pull,
        ExerciseForce.Push
    ];

    public static IReadOnlyList<ExerciseBodyCategory> BodyCategories { get; } =
    [
        ExerciseBodyCategory.LowerBody,
        ExerciseBodyCategory.UpperBody
    ];

    public static IReadOnlyList<ExerciseMechanic> Mechanics { get; } =
    [
        ExerciseMechanic.Compound,
        ExerciseMechanic.Isolation
    ];

    public static IReadOnlyList<ExerciseEquipment> Equipment { get; } =
    [
        ExerciseEquipment.Barbell,
        ExerciseEquipment.Cable,
        ExerciseEquipment.Calisthenics,
        ExerciseEquipment.Cardio,
        ExerciseEquipment.Dumbbell,
        ExerciseEquipment.Machine,
        ExerciseEquipment.Stretching
    ];

    public static IReadOnlyList<LimbInvolvement> LimbInvolvements { get; } =
    [
        LimbInvolvement.Alternating,
        LimbInvolvement.Bilateral,
        LimbInvolvement.Unilateral
    ];

    public static IReadOnlyList<MovementPattern> MovementPatterns { get; } =
    [
        MovementPattern.HorizontalPush,
        MovementPattern.VerticalPush,
        MovementPattern.HorizontalPull,
        MovementPattern.VerticalPull,
        MovementPattern.Squat,
        MovementPattern.Hinge,
        MovementPattern.Lunge,
        MovementPattern.Carry,
        MovementPattern.Rotation,
        MovementPattern.Core,
        MovementPattern.Cardio,
        MovementPattern.Stretching,
        MovementPattern.Isolation
    ];

    public static string ToDisplayName(ExerciseForce? value)
    {
        return value.HasValue ? ToDisplayName(value.Value) : string.Empty;
    }

    public static string ToDisplayName(ExerciseBodyCategory? value)
    {
        return value.HasValue ? ToDisplayName(value.Value) : string.Empty;
    }

    public static string ToDisplayName(ExerciseMechanic? value)
    {
        return value.HasValue ? ToDisplayName(value.Value) : string.Empty;
    }

    public static string ToDisplayName(ExerciseEquipment? value)
    {
        return value.HasValue ? ToDisplayName(value.Value) : string.Empty;
    }

    public static string ToDisplayName(LimbInvolvement? value)
    {
        return value.HasValue ? ToDisplayName(value.Value) : string.Empty;
    }

    public static string ToDisplayName(MovementPattern? value)
    {
        return value.HasValue ? ToDisplayName(value.Value) : string.Empty;
    }

    public static string ToDisplayName(ExerciseForce value)
    {
        return value switch
        {
            ExerciseForce.Cardio => "Cardio",
            ExerciseForce.Legs => "Legs",
            ExerciseForce.Pull => "Pull",
            ExerciseForce.Push => "Push",
            _ => value.ToString()
        };
    }

    public static string ToDisplayName(ExerciseBodyCategory value)
    {
        return value switch
        {
            ExerciseBodyCategory.LowerBody => "Lower Body",
            ExerciseBodyCategory.UpperBody => "Upper Body",
            _ => value.ToString()
        };
    }

    public static string ToDisplayName(ExerciseMechanic value)
    {
        return value switch
        {
            ExerciseMechanic.Compound => "Compound",
            ExerciseMechanic.Isolation => "Isolation",
            _ => value.ToString()
        };
    }

    public static string ToDisplayName(ExerciseEquipment value)
    {
        return value switch
        {
            ExerciseEquipment.Barbell => "Barbell",
            ExerciseEquipment.Cable => "Cable",
            ExerciseEquipment.Calisthenics => "Calisthenics",
            ExerciseEquipment.Cardio => "Cardio",
            ExerciseEquipment.Dumbbell => "Dumbbell",
            ExerciseEquipment.Machine => "Machine",
            ExerciseEquipment.Stretching => "Stretching",
            _ => value.ToString()
        };
    }

    public static string ToDisplayName(LimbInvolvement value)
    {
        return value switch
        {
            LimbInvolvement.Alternating => "Alternating",
            LimbInvolvement.Bilateral => "Bilateral",
            LimbInvolvement.Unilateral => "Unilateral",
            _ => value.ToString()
        };
    }

    public static string ToDisplayName(MovementPattern value)
    {
        return value switch
        {
            MovementPattern.HorizontalPush => "Horizontal Push",
            MovementPattern.VerticalPush => "Vertical Push",
            MovementPattern.HorizontalPull => "Horizontal Pull",
            MovementPattern.VerticalPull => "Vertical Pull",
            MovementPattern.Squat => "Squat",
            MovementPattern.Hinge => "Hinge",
            MovementPattern.Lunge => "Lunge",
            MovementPattern.Carry => "Carry",
            MovementPattern.Rotation => "Rotation",
            MovementPattern.Core => "Core",
            MovementPattern.Cardio => "Cardio",
            MovementPattern.Stretching => "Stretching",
            MovementPattern.Isolation => "Isolation",
            _ => value.ToString()
        };
    }

    public static string ToDisplayName(TrainingType value)
    {
        return value switch
        {
            TrainingType.Strength => "Strength",
            TrainingType.Hypertrophy => "Hypertrophy",
            TrainingType.Cardio => "Cardio",
            TrainingType.Mobility => "Mobility",
            TrainingType.Rehab => "Rehab",
            TrainingType.Mixed => "Mixed",
            _ => value.ToString()
        };
    }
}