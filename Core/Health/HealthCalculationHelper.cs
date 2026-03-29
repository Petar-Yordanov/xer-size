using System;

namespace XerSize.Core.Health
{
    /// Provides rough health and workout estimation helpers for BMI display, BMR baseline energy, and session calorie estimates in the app.
    /// BMI is mainly for profile/statistics context, while BMR is used as the baseline for approximate calorie-burning calculations.
    public static class HealthCalculationHelper
    {
        private const double MinutesPerDay = 1440.0;

        /// Calculates BMI from body weight and height for profile/statistics display and rough body-size classification.
        public static double CalculateBmi(double weightKg, double heightCm)
        {
            if (weightKg <= 0)
                return 0;

            if (heightCm <= 0)
                return 0;

            var heightM = heightCm / 100.0;
            return weightKg / (heightM * heightM);
        }

        /// Calculates BMR from body metrics to provide a baseline kcal/day value used by rough workout calorie estimates.
        public static double CalculateBmr(double weightKg, double heightCm, int age, Sex sex)
        {
            if (weightKg <= 0 || heightCm <= 0 || age <= 0)
                return 0;

            return sex switch
            {
                Sex.Male => (10.0 * weightKg) + (6.25 * heightCm) - (5.0 * age) + 5.0,
                Sex.Female => (10.0 * weightKg) + (6.25 * heightCm) - (5.0 * age) - 161.0,
                _ => (10.0 * weightKg) + (6.25 * heightCm) - (5.0 * age) - 78.0
            };
        }

        /// Estimates calories for a strength-training session using BMR as a baseline, active exercise time, and logged rest time.
        public static double EstimateStrengthCalories(
            double weightKg,
            double heightCm,
            int age,
            Sex sex,
            double exerciseMinutes,
            double restMinutes,
            StrengthIntensity intensity = StrengthIntensity.Moderate)
        {
            if (exerciseMinutes < 0)
                exerciseMinutes = 0;

            if (restMinutes < 0)
                restMinutes = 0;

            var bmr = CalculateBmr(weightKg, heightCm, age, sex);
            if (bmr <= 0)
                return 0;

            var kcalPerMinuteAtRest = bmr / MinutesPerDay;
            var activeMultiplier = GetStrengthIntensityMultiplier(intensity);
            const double restMultiplier = 1.2;

            var total =
                (kcalPerMinuteAtRest * exerciseMinutes * activeMultiplier) +
                (kcalPerMinuteAtRest * restMinutes * restMultiplier);

            return Math.Max(0, total);
        }

        /// Estimates calories for a cardio session from BMR and total cardio duration using a rough intensity multiplier.
        public static double EstimateCardioCalories(
            double weightKg,
            double heightCm,
            int age,
            Sex sex,
            double cardioMinutes,
            CardioIntensity intensity = CardioIntensity.Moderate)
        {
            if (cardioMinutes < 0)
                cardioMinutes = 0;

            var bmr = CalculateBmr(weightKg, heightCm, age, sex);
            if (bmr <= 0)
                return 0;

            var kcalPerMinuteAtRest = bmr / MinutesPerDay;
            var cardioMultiplier = GetCardioIntensityMultiplier(intensity);

            var total = kcalPerMinuteAtRest * cardioMinutes * cardioMultiplier;
            return Math.Max(0, total);
        }

        /// Estimates strength calories from sets, reps, logged exercise time, and rest time, preferring exercise time when available.
        public static double EstimateStrengthCaloriesFromWorkoutData(
            double weightKg,
            double heightCm,
            int age,
            Sex sex,
            int sets,
            int repsPerSet,
            double? exerciseMinutes,
            double restMinutes,
            StrengthIntensity intensity = StrengthIntensity.Moderate)
        {
            var resolvedExerciseMinutes = exerciseMinutes.HasValue && exerciseMinutes.Value > 0
                ? exerciseMinutes.Value
                : EstimateStrengthExerciseMinutesFromSetsAndReps(sets, repsPerSet);

            return EstimateStrengthCalories(
                weightKg,
                heightCm,
                age,
                sex,
                resolvedExerciseMinutes,
                restMinutes,
                intensity);
        }

        /// Roughly estimates active lifting time from sets and reps when direct exercise duration was not logged.
        public static double EstimateStrengthExerciseMinutesFromSetsAndReps(
            int sets,
            int repsPerSet,
            double averageSecondsPerRep = 4.0)
        {
            if (sets <= 0 || repsPerSet <= 0 || averageSecondsPerRep <= 0)
                return 0;

            var totalSeconds = sets * repsPerSet * averageSecondsPerRep;
            return totalSeconds / 60.0;
        }

        /// Returns a user-facing BMI category label for profile/statistics screens.
        public static string GetBmiCategory(double bmi)
        {
            if (bmi <= 0)
                return "Unknown";

            if (bmi < 18.5)
                return "Underweight";

            if (bmi < 25.0)
                return "Normal weight";

            if (bmi < 30.0)
                return "Overweight";

            return "Obese";
        }

        /// Calculates total lifted volume for training analytics, even though it should not be used directly as a calorie formula.
        public static double CalculateLiftVolumeKg(double weightPerRepKg, int sets, int repsPerSet)
        {
            if (weightPerRepKg < 0 || sets <= 0 || repsPerSet <= 0)
                return 0;

            return weightPerRepKg * sets * repsPerSet;
        }

        private static double GetStrengthIntensityMultiplier(StrengthIntensity intensity)
        {
            return intensity switch
            {
                StrengthIntensity.Light => 3.0,
                StrengthIntensity.Moderate => 4.5,
                StrengthIntensity.Hard => 6.0,
                StrengthIntensity.Circuit => 6.5,
                _ => 4.5
            };
        }

        private static double GetCardioIntensityMultiplier(CardioIntensity intensity)
        {
            return intensity switch
            {
                CardioIntensity.Light => 4.0,
                CardioIntensity.Moderate => 6.0,
                CardioIntensity.Hard => 8.0,
                _ => 6.0
            };
        }
    }

    public enum Sex
    {
        Unknown = 0,
        Male = 1,
        Female = 2
    }

    public enum StrengthIntensity
    {
        Light = 0,
        Moderate = 1,
        Hard = 2,
        Circuit = 3
    }

    public enum CardioIntensity
    {
        Light = 0,
        Moderate = 1,
        Hard = 2
    }
}

/*
 * BMI for profile/statistics display
 * BMR for calorie baseline
 * exerciseMinutes + restMinutes for strength calories
 * cardioMinutes for cardio calories
 * lift volume for strength analytics, progress charts, and records
 var bmi = HealthCalculationHelper.CalculateBmi(weightKg, heightCm);
var bmr = HealthCalculationHelper.CalculateBmr(weightKg, heightCm, age, sex);

var strengthCalories = HealthCalculationHelper.EstimateStrengthCalories(
    weightKg,
    heightCm,
    age,
    sex,
    exerciseMinutes,
    restMinutes,
    StrengthIntensity.Moderate);

var cardioCalories = HealthCalculationHelper.EstimateCardioCalories(
    weightKg,
    heightCm,
    age,
    sex,
    cardioMinutes,
    CardioIntensity.Moderate);
 */