using System.Net.Http.Headers;
using System.Net.Http.Json;
using aRevolucaoDosPetWeb.Interfaces;
using aRevolucaoDosPetWeb.Models;
using aRevolucaoDosPetWeb.Util;

namespace aRevolucaoDosPetWeb.Services;

public sealed class ApiConsumoService : IApiConsumoService
{
    private readonly HttpClient client;

    public ApiConsumoService(HttpClient client) => this.client = client;

    public async Task<string> GetPublicMessageAsync()
    {
        try
        {
            var result = await client.GetFromJsonAsync<HelloWorldResponse>("api/HelloWorld/publico");
            return result?.Message ?? ServiceMessages.Api.EmptyResponse;
        }
        catch
        {
            return ServiceMessages.Api.PublicUnavailable;
        }
    }

    public async Task<string> GetPrivateMessageAsync(string token)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "api/HelloWorld/privado");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        try
        {
            using var response = await client.SendAsync(request);
            if (!response.IsSuccessStatusCode)
                return ServiceMessages.Api.RequestRejected((int)response.StatusCode);

            var result = await response.Content.ReadFromJsonAsync<HelloWorldResponse>();
            return result is null
                ? ServiceMessages.Api.EmptyResponse
                : $"{result.Message} {result.MongoStatus}";
        }
        catch
        {
            return ServiceMessages.Api.PrivateUnavailable;
        }
    }
}