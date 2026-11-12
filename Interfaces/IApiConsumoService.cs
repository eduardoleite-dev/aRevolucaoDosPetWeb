using aRevolucaoDosPetWeb.Models;

namespace aRevolucaoDosPetWeb.Interfaces;

public interface IApiConsumoService
{
    Task<string> GetPublicMessageAsync();
    Task<string> GetPrivateMessageAsync(string token);
}