namespace XerSize.Services;

public sealed class TrainingAdviceService
{
    private readonly Random random = new();

    private static readonly IReadOnlyList<TrainingAdvice> Advice =
    [
        new(
            "Small habits matter.",
            "Consistency beats intensity when recovery is managed well."),

        new(
            "Progress compounds slowly.",
            "A few solid sessions every week will beat random all-out days."),

        new(
            "Recover like you train.",
            "Better sleep, food, and rest days make your hard sets count more."),

        new(
            "Track the basics.",
            "Reps, weight, time, and consistency tell you more than motivation does."),

        new(
            "Do not chase soreness.",
            "A good workout should create progress, not destroy your next session."),

        new(
            "Warm up with intent.",
            "Use lighter sets to practice the movement before pushing heavier work."),

        new(
            "Strong sessions start simple.",
            "Pick the main lift, execute it well, then build around it."),

        new(
            "More is not always better.",
            "If performance drops hard, recovery may be the missing exercise."),

        new(
            "Keep the streak realistic.",
            "A sustainable plan is easier to repeat than a perfect plan."),

        new(
            "Quality reps win.",
            "Controlled reps with good range usually beat heavier sloppy work.")
    ];

    public TrainingAdvice GetRandomAdvice()
    {
        if (Advice.Count == 0)
            return new TrainingAdvice("Keep going.", "Log your workout and build momentum.");

        return Advice[random.Next(Advice.Count)];
    }
}

public sealed record TrainingAdvice(
    string Title,
    string Subtitle);