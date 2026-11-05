using System.Net.Http.Headers;
using System.Net.Http.Json;
using aRevolucaoDosPetWeb.Models;

namespace aRevolucaoDosPetWeb.Services;

public sealed class ApiService
{
    public HttpClient Client { get; }

    public ApiService(HttpClient client) => Client = client;

    public async Task<string> GetPublicMessageAsync()
    {
        try
        {
            var result = await Client.GetFromJsonAsync<HelloWorldResponse>("api/HelloWorld/publico");
            return result?.Message ?? "A API não retornou uma mensagem.";
        }
        catch
        {
            return "Não foi possível consultar a API pública.";
        }
    }

    public async Task<string> GetPrivateMessageAsync(string token)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "api/HelloWorld/privado");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        try
        {
            using var response = await Client.SendAsync(request);
            if (!response.IsSuccessStatusCode)
                return $"A API recusou a chamada ({(int)response.StatusCode}).";

            var result = await response.Content.ReadFromJsonAsync<HelloWorldResponse>();
            return result is null ? "A API não retornou uma mensagem." : $"{result.Message} {result.MongoStatus}";
        }
        catch
        {
            return "Não foi possível consultar a API protegida.";
        }
    }

    public async Task<UserProfileResponse?> GetUserAsync(string userId, string token)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, $"api/Users/{Uri.EscapeDataString(userId)}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        using var response = await Client.SendAsync(request);
        if (!response.IsSuccessStatusCode)
            return null;

        return await response.Content.ReadFromJsonAsync<UserProfileResponse>();
    }

    public async Task<(bool Success, string Message)> RegisterUserAsync(RegisterUserRequest request)
    {
        try
        {
            using var response = await Client.PostAsJsonAsync("api/Users/register", request);
            if (response.IsSuccessStatusCode)
                return (true, "Conta criada com sucesso!");

            var detail = await response.Content.ReadAsStringAsync();
            return (false, string.IsNullOrWhiteSpace(detail)
                ? $"Não foi possível criar a conta ({(int)response.StatusCode})."
                : detail);
        }
        catch
        {
            return (false, "Não foi possível conectar à API.");
        }
    }
}