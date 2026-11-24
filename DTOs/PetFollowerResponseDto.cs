namespace aRevolucaoDosPetWeb.DTOs;

public record PetFollowerResponseDto(
    string Id,
    string PetId,
    string? FollowerUserId,
    string? FollowerPetId,
    DateTime CreatedAt
);
