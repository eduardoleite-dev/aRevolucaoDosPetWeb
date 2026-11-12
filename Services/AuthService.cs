using System.Text;
using System.Text.Json;
using aRevolucaoDosPetWeb.Interfaces;
using aRevolucaoDosPetWeb.Models;
using aRevolucaoDosPetWeb.Util;

namespace aRevolucaoDosPetWeb.Services;

public sealed class AuthService : IAuthService
{
    private readonly IApiConsumoUserService apiUsers;
    private readonly ISessionService session;

    public AuthService(IApiConsumoUserService apiUsers, ISessionService session)
    {
        this.apiUsers = apiUsers;
        this.session = session;
    }

    public async Task<bool> LoginAsync(string username, string password)
    {
        var result = await apiUsers.LoginAsync(new LoginRequest(username, password));
        if (result is null || string.IsNullOrWhiteSpace(result.Token) || result.ExpiresAt <= DateTimeOffset.UtcNow)
            return false;

        var claims = ReadClaims(result.Token);
        await session.SetAsync(new SessionData(result.Token, result.ExpiresAt, result.Username, result.UserId)
        {
            AccessLevel = claims.AccessLevel,
            Role = claims.Role
        });
        return true;
    }

    private static (string AccessLevel, string Role) ReadClaims(string token)
    {
        try
        {
            var parts = token.Split('.');
            if (parts.Length < 2)
                return (ServiceMessages.Auth.UnknownAccessLevel, ServiceMessages.Auth.DefaultRole);

            var payload = parts[1].Replace('-', '+').Replace('_', '/');
            payload = payload.PadRight(payload.Length + (4 - payload.Length % 4) % 4, '=');
            using var document = JsonDocument.Parse(Encoding.UTF8.GetString(Convert.FromBase64String(payload)));
            var root = document.RootElement;
            return (
                root.TryGetProperty("nivel_acesso", out var level)
                    ? level.GetString() ?? ServiceMessages.Auth.UnknownAccessLevel
                    : ServiceMessages.Auth.UnknownAccessLevel,
                root.TryGetProperty("role", out var role)
                    ? role.GetString() ?? ServiceMessages.Auth.DefaultRole
                    : ServiceMessages.Auth.DefaultRole);
        }
        catch
        {
            return (ServiceMessages.Auth.UnknownAccessLevel, ServiceMessages.Auth.DefaultRole);
        }
    }
}