using System.Text.Json.Serialization;

namespace aRevolucaoDosPetWeb.Models;

public sealed record LoginResponse(
    [property: JsonPropertyName("token")] string Token,
    [property: JsonPropertyName("expiresAt")] DateTimeOffset ExpiresAt,
    [property: JsonPropertyName("username")] string Username,
    [property: JsonPropertyName("userId")] string UserId);
