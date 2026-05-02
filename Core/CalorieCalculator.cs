namespace XerSize.Core;

public enum Sex
{
    Male,
    Female
}

public static class CalorieCalculator
{
    // Calculates the user's estimated resting calories per day.
    // Use this once from profile data before calculating workout calories.
    public static double CalculateBmrMifflin(
        double weightKg,
        double heightCm,
        int age,
        Sex sex)
    {
        var bmr = 10.0 * weightKg + 6.25 * heightCm - 5.0 * age;

        return sex == Sex.Male
            ? bmr + 5.0
            : bmr - 161.0;
    }

    // Calculates total calories burned during the workout time, including normal resting burn.
    // Use this only if you want "total session calories" instead of extra workout calories.
    public static double CalculateWorkoutCaloriesGross(
        double bmr,
        double met,
        double activeMinutes)
    {
        var restingKcalPerMinute = bmr / 1440.0;
        return met * restingKcalPerMinute * Math.Max(0, activeMinutes);
    }

    // Calculates extra calories burned because of the workout, excluding normal resting burn.
    // Use this as the main "calories burned" number in the app.
    public static double CalculateWorkoutCaloriesActiveOnly(
        double bmr,
        double met,
        double activeMinutes)
    {
        var restingKcalPerMinute = bmr / 1440.0;
        return Math.Max(0.0, met - 1.0) * restingKcalPerMinute * Math.Max(0, activeMinutes);
    }

    // Estimates calories from duration and distance by adjusting the supplied base MET
    // using a caller-provided speed-adjusted MET.
    public static double CalculateWorkoutCaloriesFromAdjustedMet(
        double bmr,
        double adjustedMet,
        double activeMinutes)
    {
        return CalculateWorkoutCaloriesActiveOnly(
            bmr,
            adjustedMet,
            activeMinutes);
    }
}