using System.Text.Json;
using System.Text.Json.Serialization;
using XerSize.Models.Definitions;
using XerSize.Models.Presentation.Options;
using XerSize.Models.Presentation.Settings;

namespace XerSize.Models.Serialization;

public sealed class DisplayNameEnumJsonConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        var enumType = Nullable.GetUnderlyingType(typeToConvert) ?? typeToConvert;

        return enumType == typeof(ExerciseForce)
            || enumType == typeof(ExerciseBodyCategory)
            || enumType == typeof(ExerciseMechanic)
            || enumType == typeof(ExerciseEquipment)
            || enumType == typeof(LimbInvolvement)
            || enumType == typeof(MovementPattern)
            || enumType == typeof(TrainingType)
            || enumType == typeof(GenderOption)
            || enumType == typeof(UnitSystem)
            || enumType == typeof(InitialPageOption);
    }

    public override JsonConverter CreateConverter(
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        var nullableEnumType = Nullable.GetUnderlyingType(typeToConvert);

        if (nullableEnumType is not null)
        {
            var nullableConverterType = typeof(NullableDisplayNameEnumJsonConverter<>).MakeGenericType(nullableEnumType);
            return (JsonConverter)Activator.CreateInstance(nullableConverterType)!;
        }

        var converterType = typeof(DisplayNameEnumJsonConverter<>).MakeGenericType(typeToConvert);
        return (JsonConverter)Activator.CreateInstance(converterType)!;
    }

    private sealed class DisplayNameEnumJsonConverter<TEnum> : JsonConverter<TEnum>
        where TEnum : struct, Enum
    {
        public override TEnum Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Number && reader.TryGetInt32(out var numericValue))
            {
                if (Enum.IsDefined(typeof(TEnum), numericValue))
                    return (TEnum)Enum.ToObject(typeof(TEnum), numericValue);

                throw new JsonException($"Invalid numeric value '{numericValue}' for enum '{typeof(TEnum).Name}'.");
            }

            if (reader.TokenType != JsonTokenType.String)
                throw new JsonException($"Expected string value for enum '{typeof(TEnum).Name}'.");

            var rawValue = reader.GetString();

            if (string.IsNullOrWhiteSpace(rawValue))
                throw new JsonException($"Empty value is not valid for enum '{typeof(TEnum).Name}'.");

            return Parse<TEnum>(rawValue);
        }

        public override void Write(
            Utf8JsonWriter writer,
            TEnum value,
            JsonSerializerOptions options)
        {
            writer.WriteStringValue(ToDisplayName(value));
        }
    }

    private sealed class NullableDisplayNameEnumJsonConverter<TEnum> : JsonConverter<TEnum?>
        where TEnum : struct, Enum
    {
        public override TEnum? Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
                return null;

            if (reader.TokenType == JsonTokenType.Number && reader.TryGetInt32(out var numericValue))
            {
                if (Enum.IsDefined(typeof(TEnum), numericValue))
                    return (TEnum)Enum.ToObject(typeof(TEnum), numericValue);

                throw new JsonException($"Invalid numeric value '{numericValue}' for enum '{typeof(TEnum).Name}'.");
            }

            if (reader.TokenType != JsonTokenType.String)
                throw new JsonException($"Expected string value for enum '{typeof(TEnum).Name}'.");

            var rawValue = reader.GetString();

            if (string.IsNullOrWhiteSpace(rawValue))
                return null;

            return Parse<TEnum>(rawValue);
        }

        public override void Write(
            Utf8JsonWriter writer,
            TEnum? value,
            JsonSerializerOptions options)
        {
            if (!value.HasValue)
            {
                writer.WriteNullValue();
                return;
            }

            writer.WriteStringValue(ToDisplayName(value.Value));
        }
    }

    private static TEnum Parse<TEnum>(string rawValue)
        where TEnum : struct, Enum
    {
        foreach (var value in Enum.GetValues<TEnum>())
        {
            if (Normalize(rawValue) == Normalize(value.ToString()))
                return value;

            if (Normalize(rawValue) == Normalize(ToDisplayName(value)))
                return value;
        }

        throw new JsonException($"Value '{rawValue}' is not valid for enum '{typeof(TEnum).Name}'.");
    }

    private static string ToDisplayName<TEnum>(TEnum value)
        where TEnum : struct, Enum
    {
        return value switch
        {
            ExerciseForce force => ExercisePresentationOptions.ToDisplayName(force),
            ExerciseBodyCategory bodyCategory => ExercisePresentationOptions.ToDisplayName(bodyCategory),
            ExerciseMechanic mechanic => ExercisePresentationOptions.ToDisplayName(mechanic),
            ExerciseEquipment equipment => ExercisePresentationOptions.ToDisplayName(equipment),
            LimbInvolvement limbInvolvement => ExercisePresentationOptions.ToDisplayName(limbInvolvement),
            MovementPattern movementPattern => ExercisePresentationOptions.ToDisplayName(movementPattern),
            TrainingType trainingType => ExercisePresentationOptions.ToDisplayName(trainingType),
            GenderOption gender => ProfilePresentationOptions.ToDisplayName(gender),
            UnitSystem unitSystem => ProfilePresentationOptions.ToDisplayName(unitSystem),
            InitialPageOption initialPage => ProfilePresentationOptions.ToDisplayName(initialPage),
            _ => value.ToString()
        };
    }

    private static string Normalize(string value)
    {
        return new string(
            value
                .Trim()
                .Where(char.IsLetterOrDigit)
                .Select(char.ToLowerInvariant)
                .ToArray());
    }
}