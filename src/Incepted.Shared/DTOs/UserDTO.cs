using System.Text.Json.Serialization;

namespace Incepted.Shared.DTOs;

public record class UserDTO(
    [property: JsonPropertyName("firstName")] string FirstName,
    [property: JsonPropertyName("lastName")] string LastName,
    [property: JsonPropertyName("email")] string Email
    );

