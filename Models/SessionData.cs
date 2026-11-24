namespace aRevolucaoDosPetWeb.Models;

public sealed record SessionData(string Token, DateTimeOffset ExpiresAt, string Username, string UserId)
{
    public string AccessLevel { get; init; } = "-";
    public string Role { get; init; } = "Usuário";
    public string DisplayName => string.IsNullOrWhiteSpace(UserId) ? Username : UserId;
    public bool IsExpired => ExpiresAt <= DateTimeOffset.UtcNow;
}
