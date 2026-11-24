using System.Text.Json.Serialization;

namespace aRevolucaoDosPetWeb.Models;

public sealed record UserLocationResponse(
    [property: JsonPropertyName("type")] string? Type,
    [property: JsonPropertyName("coordinates")] double[]? Coordinates);
