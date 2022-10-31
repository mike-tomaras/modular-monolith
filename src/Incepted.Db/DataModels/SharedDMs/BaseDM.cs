using Newtonsoft.Json;

namespace Incepted.Db.DataModels.SharedDMs;

internal class BaseDM
{
    [JsonProperty("v")] public int Version { get; set; }
    [JsonProperty("id")] public Guid Id { get; set; }
}
