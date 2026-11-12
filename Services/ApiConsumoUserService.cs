using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Components.Forms;
using aRevolucaoDosPetWeb.Interfaces;
using aRevolucaoDosPetWeb.Models;
using aRevolucaoDosPetWeb.Util;

namespace aRevolucaoDosPetWeb.Services;

public sealed class ApiConsumoUserService : IApiConsumoUserService
{
    private const long MaxMediaSizeBytes = 10 * 1024 * 1024;
    private readonly HttpClient client;

    public ApiConsumoUserService(HttpClient client) => this.client = client;

    public async Task<LoginResponse?> LoginAsync(LoginRequest loginRequest)
    {
        try
        {
            using var response = await client.PostAsJsonAsync("api/Auth/login", loginRequest);
            return response.IsSuccessStatusCode
                ? await response.Content.ReadFromJsonAsync<LoginResponse>()
                : null;
        }
        catch
        {
            return null;
        }
    }

    public async Task<UserProfileResponse?> GetUserAsync(string userId, string token)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, $"api/Users/{Uri.EscapeDataString(userId)}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        using var response = await client.SendAsync(request);
        if (!response.IsSuccessStatusCode)
            return null;

        return await response.Content.ReadFromJsonAsync<UserProfileResponse>();
    }

    public async Task<(bool Success, string Message)> RegisterUserAsync(RegisterUserRequest registerRequest)
    {
        try
        {
            using var response = await client.PostAsJsonAsync("api/Users/register", registerRequest);
            if (response.IsSuccessStatusCode)
                return (true, ServiceMessages.User.RegistrationSuccess);

            var detail = await response.Content.ReadAsStringAsync();
            return (false, string.IsNullOrWhiteSpace(detail)
                ? ServiceMessages.User.RegistrationFailed((int)response.StatusCode)
                : detail);
        }
        catch
        {
            return (false, ServiceMessages.User.ConnectionUnavailable);
        }
    }

    public async Task<(bool Success, string? Url, string Message)> UploadMediaAsync(
        IBrowserFile file, string name = "profile-photo")
    {
        try
        {
            if (string.IsNullOrWhiteSpace(name) || name.Length > 100 ||
                name.Any(character => !char.IsAsciiLetterOrDigit(character) && character is not '_' and not '-'))
                return (false, null, ServiceMessages.User.InvalidMediaName);

            await using var stream = file.OpenReadStream(MaxMediaSizeBytes);
            using var fileContent = new StreamContent(stream);
            if (!string.IsNullOrWhiteSpace(file.ContentType))
                fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);

            using var form = new MultipartFormDataContent();
            form.Add(new StringContent(name), "Name");
            form.Add(fileContent, "File", Path.GetFileName(file.Name));

            using var request = new HttpRequestMessage(HttpMethod.Post, "api/Media/upload") { Content = form };
            request.Headers.Accept.ParseAdd("text/plain");

            using var response = await client.SendAsync(request);
            if (!response.IsSuccessStatusCode)
                return (false, null, await ReadApiErrorAsync(response));

            var responseBody = await response.Content.ReadAsStringAsync();
            using var document = JsonDocument.Parse(responseBody);
            if (!document.RootElement.TryGetProperty("url", out var urlElement) ||
                urlElement.ValueKind != JsonValueKind.String ||
                string.IsNullOrWhiteSpace(urlElement.GetString()))
                return (false, null, ServiceMessages.User.MediaUrlMissing);

            return (true, urlElement.GetString(), ServiceMessages.User.MediaUploadSuccess);
        }
        catch
        {
            return (false, null, ServiceMessages.User.MediaUploadFailed);
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

            using var response = await client.SendAsync(request);
            if (!response.IsSuccessStatusCode)
                return (false, null, await ReadApiErrorAsync(response));

            var profile = await response.Content.ReadFromJsonAsync<UserProfileResponse>();
            return profile is null
                ? (false, null, ServiceMessages.User.UpdatedProfileMissing)
                : (true, profile, ServiceMessages.User.PhotoUpdateSuccess);
        }
        catch
        {
            return (false, null, ServiceMessages.User.PhotoUpdateFailed);
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

            using var response = await client.SendAsync(request);
            if (!response.IsSuccessStatusCode)
                return (false, null, await ReadApiErrorAsync(response));

            var profile = await response.Content.ReadFromJsonAsync<UserProfileResponse>();
            return profile is null
                ? (false, null, ServiceMessages.User.UpdatedUserMissing)
                : (true, profile, ServiceMessages.User.ProfileUpdateSuccess);
        }
        catch
        {
            return (false, null, ServiceMessages.User.ProfileUpdateFailed);
        }
    }

    private static async Task<string> ReadApiErrorAsync(HttpResponseMessage response)
    {
        var detail = await response.Content.ReadAsStringAsync();
        return string.IsNullOrWhiteSpace(detail)
            ? ServiceMessages.User.RequestRejected((int)response.StatusCode)
            : detail;
    }
}