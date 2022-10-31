using Humanizer;
using System.Text.Json.Serialization;

namespace Incepted.Shared.ValueTypes;

public record class Modification
{
    [JsonPropertyName("notes")] public string Notes { get; private init; }
    [JsonPropertyName("timestamp")] public DateTimeOffset TimeStamp { get; private init; }

    [Newtonsoft.Json.JsonIgnore][JsonIgnore] public string TimestampString => $"{TimeStamp.ToLocalTime().Humanize()}: {TimeStamp.ToLocalTime().ToString("HH:mm")}";

    public Modification(string notes, DateTimeOffset timeStamp)
    {
        Notes = notes;
        TimeStamp = timeStamp;
    }
}
