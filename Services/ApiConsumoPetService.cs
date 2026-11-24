using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using aRevolucaoDosPetWeb.DTOs;
using aRevolucaoDosPetWeb.Interfaces;
using aRevolucaoDosPetWeb.Models;
using aRevolucaoDosPetWeb.Util;

namespace aRevolucaoDosPetWeb.Services;

public sealed class ApiConsumoPetService : IApiConsumoPetService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly HttpClient client;

    public ApiConsumoPetService(HttpClient client) => this.client = client;

    public async Task<(bool Success, PagedResult<PetResponseDto>? Result, string Message)> SearchAsync(
        PetSearchFilter filter,
        string token,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(filter);
        if (string.IsNullOrWhiteSpace(token))
            return (false, null, ServiceMessages.Pet.AuthenticationRequired);

        using var request = new HttpRequestMessage(HttpMethod.Get, BuildSearchUri(filter));
        AddBearerToken(request, token);

        return await SendJsonAsync<PagedResult<PetResponseDto>>(
            request,
            ServiceMessages.Pet.SearchResponseMissing,
            cancellationToken);
    }

    public async Task<(bool Success, IPetProfileResponse? Profile, string Message)> GetByIdAsync(
        string id,
        string? token = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);

        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"api/Pets/{Uri.EscapeDataString(id)}");
        if (!string.IsNullOrWhiteSpace(token))
            AddBearerToken(request, token);

        try
        {
            using var response = await client.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
                return (false, null, await ReadApiErrorAsync(response, cancellationToken));

            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
            using var document = JsonDocument.Parse(responseBody);
            if (document.RootElement.ValueKind != JsonValueKind.Object)
                return (false, null, ServiceMessages.Pet.InvalidResponse);

            var isFullProfile = document.RootElement
                .EnumerateObject()
                .Any(property => string.Equals(property.Name, "ownerId", StringComparison.OrdinalIgnoreCase));

            IPetProfileResponse? profile = isFullProfile
                ? JsonSerializer.Deserialize<PetResponseDto>(responseBody, JsonOptions)
                : JsonSerializer.Deserialize<PetPublicProfileDto>(responseBody, JsonOptions);

            return profile is null
                ? (false, null, ServiceMessages.Pet.InvalidResponse)
                : (true, profile, string.Empty);
        }
        catch (HttpRequestException)
        {
            return (false, null, ServiceMessages.Pet.ConnectionUnavailable);
        }
        catch (JsonException)
        {
            return (false, null, ServiceMessages.Pet.InvalidResponse);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return (false, null, ServiceMessages.Pet.RequestTimedOut);
        }
    }

    public async Task<(bool Success, PetResponseDto? Pet, string Message)> CreateAsync(
        CreatePetRequest requestBody,
        string token,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(requestBody);
        if (string.IsNullOrWhiteSpace(token))
            return (false, null, ServiceMessages.Pet.AuthenticationRequired);

        using var request = new HttpRequestMessage(HttpMethod.Post, "api/Pets")
        {
            Content = JsonContent.Create(requestBody)
        };
        AddBearerToken(request, token);

        return await SendJsonAsync<PetResponseDto>(
            request,
            ServiceMessages.Pet.PetResponseMissing,
            cancellationToken);
    }

    public async Task<(bool Success, PetResponseDto? Pet, string Message)> UpdateAsync(
        string id,
        PetWriteRequest requestBody,
        string token,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentNullException.ThrowIfNull(requestBody);
        if (string.IsNullOrWhiteSpace(token))
            return (false, null, ServiceMessages.Pet.AuthenticationRequired);

        using var request = new HttpRequestMessage(
            HttpMethod.Put,
            $"api/Pets/{Uri.EscapeDataString(id)}")
        {
            Content = JsonContent.Create(requestBody)
        };
        AddBearerToken(request, token);

        return await SendJsonAsync<PetResponseDto>(
            request,
            ServiceMessages.Pet.PetResponseMissing,
            cancellationToken);
    }

    public async Task<(bool Success, string Message)> DeleteAsync(
        string id,
        string token,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        if (string.IsNullOrWhiteSpace(token))
            return (false, ServiceMessages.Pet.AuthenticationRequired);

        using var request = new HttpRequestMessage(
            HttpMethod.Delete,
            $"api/Pets/{Uri.EscapeDataString(id)}");
        AddBearerToken(request, token);

        try
        {
            using var response = await client.SendAsync(request, cancellationToken);
            return response.IsSuccessStatusCode
                ? (true, string.Empty)
                : (false, await ReadApiErrorAsync(response, cancellationToken));
        }
        catch (HttpRequestException)
        {
            return (false, ServiceMessages.Pet.ConnectionUnavailable);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return (false, ServiceMessages.Pet.RequestTimedOut);
        }
    }

    private async Task<(bool Success, T? Data, string Message)> SendJsonAsync<T>(
        HttpRequestMessage request,
        string missingResponseMessage,
        CancellationToken cancellationToken)
    {
        try
        {
            using var response = await client.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
                return (false, default, await ReadApiErrorAsync(response, cancellationToken));

            var data = await response.Content.ReadFromJsonAsync<T>(JsonOptions, cancellationToken);
            return data is null
                ? (false, default, missingResponseMessage)
                : (true, data, string.Empty);
        }
        catch (HttpRequestException)
        {
            return (false, default, ServiceMessages.Pet.ConnectionUnavailable);
        }
        catch (JsonException)
        {
            return (false, default, ServiceMessages.Pet.InvalidResponse);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return (false, default, ServiceMessages.Pet.RequestTimedOut);
        }
    }

    private static string BuildSearchUri(PetSearchFilter filter)
    {
        var queryParameters = typeof(PetSearchFilter)
            .GetProperties()
            .Select(property => new
            {
                property.Name,
                Value = property.GetValue(filter)
            })
            .Where(parameter => parameter.Value is not null)
            .Select(parameter => new KeyValuePair<string, string?>(
                parameter.Name,
                FormatQueryValue(parameter.Value!)));

        var query = string.Join("&", queryParameters.Select(parameter =>
            $"{Uri.EscapeDataString(parameter.Key)}={Uri.EscapeDataString(parameter.Value!)}"));
        return string.IsNullOrEmpty(query) ? "api/Pets" : $"api/Pets?{query}";
    }

    private static string FormatQueryValue(object value) =>
        value switch
        {
            DateTime dateTime => dateTime.ToString("O", CultureInfo.InvariantCulture),
            Enum enumValue => enumValue.ToString(),
            IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture),
            _ => value.ToString() ?? string.Empty
        };

    private static void AddBearerToken(HttpRequestMessage request, string token) =>
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

    private static async Task<string> ReadApiErrorAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!string.IsNullOrWhiteSpace(responseBody))
        {
            try
            {
                using var document = JsonDocument.Parse(responseBody);
                if (document.RootElement.ValueKind == JsonValueKind.Object)
                {
                    foreach (var property in document.RootElement.EnumerateObject())
                    {
                        if (property.Name.Equals("message", StringComparison.OrdinalIgnoreCase) ||
                            property.Name.Equals("detail", StringComparison.OrdinalIgnoreCase) ||
                            property.Name.Equals("title", StringComparison.OrdinalIgnoreCase))
                        {
                            if (property.Value.ValueKind == JsonValueKind.String &&
                                !string.IsNullOrWhiteSpace(property.Value.GetString()))
                                return property.Value.GetString()!;
                        }
                    }
                }
            }
            catch (JsonException)
            {
                return responseBody;
            }

            return responseBody;
        }

        return ServiceMessages.Pet.RequestRejected((int)response.StatusCode);
    }
}
