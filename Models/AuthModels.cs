using System.Text.Json.Serialization;

namespace aRevolucaoDosPetWeb.Models;

public sealed record LoginRequest(string Username, string Password);

public sealed record LoginResponse(
    [property: JsonPropertyName("token")] string Token,
    [property: JsonPropertyName("expiresAt")] DateTimeOffset ExpiresAt,
    [property: JsonPropertyName("username")] string Username,
    [property: JsonPropertyName("userId")] string UserId);

public sealed record SessionData(string Token, DateTimeOffset ExpiresAt, string Username, string UserId)
{
    public string AccessLevel { get; init; } = "-";
    public string Role { get; init; } = "Usuário";
    public string DisplayName => string.IsNullOrWhiteSpace(UserId) ? Username : UserId;
    public bool IsExpired => ExpiresAt <= DateTimeOffset.UtcNow;
}

public sealed record HelloWorldResponse(
    [property: JsonPropertyName("message")] string Message,
    [property: JsonPropertyName("mongoStatus")] string? MongoStatus = null);

public sealed record UserLocationResponse(
    [property: JsonPropertyName("type")] string? Type,
    [property: JsonPropertyName("coordinates")] double[]? Coordinates);

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

public sealed record UserLocationRequest(string Type, double[] Coordinates);

public sealed record RegisterUserRequest(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("email")] string Email,
    [property: JsonPropertyName("password")] string Password,
    [property: JsonPropertyName("avatarUrl")] string? AvatarUrl = null,
    [property: JsonPropertyName("bio")] string? Bio = null,
    [property: JsonPropertyName("phone")] string? Phone = null,
    [property: JsonPropertyName("city")] string? City = null,
    [property: JsonPropertyName("state")] string? State = null,
    [property: JsonPropertyName("country")] string? Country = "BR",
    [property: JsonPropertyName("location")] UserLocationRequest? Location = null);

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