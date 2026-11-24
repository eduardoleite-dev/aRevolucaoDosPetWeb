using System.Text.Json.Serialization;

namespace aRevolucaoDosPetWeb.Models;

public sealed record UserProfileResponse(
    [property: JsonPropertyName("id")] string? Id,
    [property: JsonPropertyName("name")] string? Name,
    [property: JsonPropertyName("email")] string? Email,
    [property: JsonPropertyName("avatarUrl")] string? AvatarUrl,
    [property: JsonPropertyName("bio")] string? Bio,
    [property: JsonPropertyName("phone")] string? Phone,
    [property: JsonPropertyName("city")] string? City,
    [property: JsonPropertyName("state")] string? State,
    [property: JsonPropertyName("country")] string? Country,
    [property: JsonPropertyName("location")] UserLocationResponse? Location,
    [property: JsonPropertyName("accessLevel")] int AccessLevel,
    [property: JsonPropertyName("status")] string? Status,
    [property: JsonPropertyName("emailVerifiedAt")] DateTimeOffset? EmailVerifiedAt,
    [property: JsonPropertyName("lastLoginAt")] DateTimeOffset? LastLoginAt,
    [property: JsonPropertyName("createdAt")] DateTimeOffset CreatedAt,
    [property: JsonPropertyName("updatedAt")] DateTimeOffset UpdatedAt,
    [property: JsonPropertyName("deletedAt")] DateTimeOffset? DeletedAt);
