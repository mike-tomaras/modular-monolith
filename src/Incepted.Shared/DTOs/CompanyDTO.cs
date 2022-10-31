using System.Text.Json.Serialization;

namespace Incepted.Shared.DTOs;

public record class CompanyDTO(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("name")] string Name
    )
{
    public override string ToString() => Name;
}