using System.Text.Json.Serialization;

namespace aRevolucaoDosPetWeb.Models;

public sealed record HelloWorldResponse(
    [property: JsonPropertyName("message")] string Message,
    [property: JsonPropertyName("mongoStatus")] string? MongoStatus = null);
