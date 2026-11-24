using System.Text.Json.Serialization;

namespace aRevolucaoDosPetWeb.Models;

public sealed record UpdateUserRequest(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("email")] string Email,
    [property: JsonPropertyName("avatarUrl")] string? AvatarUrl = null,
    [property: JsonPropertyName("bio")] string? Bio = null,
    [property: JsonPropertyName("phone")] string? Phone = null,
    [property: JsonPropertyName("city")] string? City = null,
    [property: JsonPropertyName("state")] string? State = null,
    [property: JsonPropertyName("country")] string? Country = null,
    [property: JsonPropertyName("location")] UserLocationRequest? Location = null);
