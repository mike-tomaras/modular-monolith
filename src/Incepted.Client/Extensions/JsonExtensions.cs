using Optional;
using Serilog;
using System.Text.Json;

namespace Incepted.Client.Extensions;

internal static class JsonExtensions
{
    public static Option<T> DeserializeJson<T>(string json)
    {
        try
        {
            var jsonOptions = new JsonSerializerOptions();
            //to add json converters etc
            var value = JsonSerializer.Deserialize<T>(json, jsonOptions);
#pragma warning disable CS8619 // Nullability of reference types in value doesn't match target type.
            return value.Some();
#pragma warning restore CS8619 // Nullability of reference types in value doesn't match target type.
        }
        catch (Exception ex) when (ex is JsonException || ex is ArgumentException)
        {
            Log.Warning(ex, "Failed to deserialize Json");
        }

        return Option.None<T>();
    }

    //TODO: uncomment if needed
//    public static Option<string> SerializeJson<T>(T content)
//    {
//        try
//        {
//            var jsonOptions = new JsonSerializerOptions();
//            //to add json converters etc
//            var value = JsonSerializer.Serialize<T>(content, jsonOptions);
//#pragma warning disable CS8619 // Nullability of reference types in value doesn't match target type.
//            return value.Some();
//#pragma warning restore CS8619 // Nullability of reference types in value doesn't match target type.
//        }
//        catch (Exception ex) when (ex is JsonException || ex is ArgumentException)
//        {
//            Log.Warning(ex, "Failed to deserialize Json");
//        }

//        return Option.None<string>();
//    }
}
