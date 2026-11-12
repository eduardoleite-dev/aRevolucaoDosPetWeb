using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Components.Forms;
using aRevolucaoDosPetWeb.Models;

namespace aRevolucaoDosPetWeb.Services;

public sealed class ApiService
{
    private const long MaxMediaSizeBytes = 10 * 1024 * 1024;

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

    public async Task<(bool Success, string? Url, string Message)> UploadMediaAsync(IBrowserFile file, string name = "profile-photo")
    {
        try
        {
            if (string.IsNullOrWhiteSpace(name) || name.Length > 100 ||
                name.Any(character => !char.IsAsciiLetterOrDigit(character) && character is not '_' and not '-'))
                return (false, null, "O nome do arquivo enviado à API não é válido.");

            await using var stream = file.OpenReadStream(MaxMediaSizeBytes);
            using var fileContent = new StreamContent(stream);
            if (!string.IsNullOrWhiteSpace(file.ContentType))
                fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);

            using var form = new MultipartFormDataContent();
            form.Add(new StringContent(name), "Name");
            form.Add(fileContent, "File", Path.GetFileName(file.Name));

            using var request = new HttpRequestMessage(HttpMethod.Post, "api/Media/upload") { Content = form };
            request.Headers.Accept.ParseAdd("text/plain");

            using var response = await Client.SendAsync(request);
            if (!response.IsSuccessStatusCode)
                return (false, null, await ReadApiErrorAsync(response));

            var responseBody = await response.Content.ReadAsStringAsync();
            using var document = JsonDocument.Parse(responseBody);
            if (!document.RootElement.TryGetProperty("url", out var urlElement) ||
                urlElement.ValueKind != JsonValueKind.String ||
                string.IsNullOrWhiteSpace(urlElement.GetString()))
                return (false, null, "A API não retornou a URL da foto enviada.");

            return (true, urlElement.GetString(), "Foto enviada com sucesso.");
        }
        catch (Exception)
        {
            return (false, null, "Não foi possível enviar a foto. Verifique o arquivo e tente novamente.");
        }
    }

    public async Task<(bool Success, UserProfileResponse? Profile, string Message)> UpdateUserPhotoAsync(
        string userId, IBrowserFile file, string token)
    {
        try
        {
            await using var stream = file.OpenReadStream(MaxMediaSizeBytes);
            using var fileContent = new StreamContent(stream);
            if (!string.IsNullOrWhiteSpace(file.ContentType))
                fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);

            using var form = new MultipartFormDataContent();
            form.Add(fileContent, "File", Path.GetFileName(file.Name));

            using var request = new HttpRequestMessage(HttpMethod.Post,
                $"api/Users/{Uri.EscapeDataString(userId)}/photo") { Content = form };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            using var response = await Client.SendAsync(request);
            if (!response.IsSuccessStatusCode)
                return (false, null, await ReadApiErrorAsync(response));

            var profile = await response.Content.ReadFromJsonAsync<UserProfileResponse>();
            return profile is null
                ? (false, null, "A API não retornou o perfil atualizado.")
                : (true, profile, "Foto atualizada com sucesso.");
        }
        catch (Exception)
        {
            return (false, null, "Não foi possível atualizar a foto. Verifique o arquivo e tente novamente.");
        }
    }

    public async Task<(bool Success, UserProfileResponse? Profile, string Message)> UpdateUserAsync(
        string userId, UpdateUserRequest update, string token)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Put,
                $"api/Users/{Uri.EscapeDataString(userId)}") { Content = JsonContent.Create(update) };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            using var response = await Client.SendAsync(request);
            if (!response.IsSuccessStatusCode)
                return (false, null, await ReadApiErrorAsync(response));

            var profile = await response.Content.ReadFromJsonAsync<UserProfileResponse>();
            return profile is null
                ? (false, null, "A API não retornou os dados atualizados.")
                : (true, profile, "Perfil atualizado com sucesso.");
        }
        catch (Exception)
        {
            return (false, null, "Não foi possível atualizar o perfil.");
        }
    }

    private static async Task<string> ReadApiErrorAsync(HttpResponseMessage response)
    {
        var detail = await response.Content.ReadAsStringAsync();
        return string.IsNullOrWhiteSpace(detail)
            ? $"A API recusou a solicitação ({(int)response.StatusCode})."
            : detail;
    }
}