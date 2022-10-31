using Incepted.Shared.ValueTypes;
using System.Collections.Immutable;
using System.Text.Json.Serialization;

namespace Incepted.Shared.DTOs;


public record class UpdateDealAssigneesDTO(
    [property: JsonPropertyName("dealId")] Guid DealId,
    [property: JsonPropertyName("assignees")] IImmutableList<EmployeeDTO> Assignees
    )
{
    [JsonIgnore] public IEnumerable<UserId> AssigneeIds => Assignees.Select(a => new UserId(a.UserId));
}


public record class EmployeeDTO(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("userId")] string UserId,
    [property: JsonPropertyName("firstName")] string FirstName,
    [property: JsonPropertyName("lastName")] string LastName,
    [property: JsonPropertyName("email")] string Email)
{
    [JsonIgnore] public string Initials => $"{FirstName.Substring(0, 1)}{LastName.Substring(0, 1)}";
    [JsonIgnore] public string FullName => $"{FirstName} {LastName}";

    public override string ToString()
    {
        return FullName;
    }
}