using aRevolucaoDosPetWeb.Models;

namespace aRevolucaoDosPetWeb.Interfaces;

public interface ISessionService
{
    Task<SessionData?> GetAsync();
    Task SetAsync(SessionData session);
    Task ClearAsync();
}