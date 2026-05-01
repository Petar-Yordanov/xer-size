using System.Text.Json;

namespace XerSize.Models.Serialization;

public static class XerSizeJsonSerializerOptions
{
    public static JsonSerializerOptions Default { get; } = CreateDefault();

    public static JsonSerializerOptions CreateDefault()
    {
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = true
        };

        options.Converters.Add(new DisplayNameEnumJsonConverterFactory());

        return options;
    }
}