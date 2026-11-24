using aRevolucaoDosPetWeb.Enums;

namespace aRevolucaoDosPetWeb.DTOs;

public record PetPublicProfileDto(
    string Id,
    string Name,
    string? Username,
    string? Bio,
    string? AvatarUrl,
    string? CoverUrl,
    Species Species,
    string? Breed,
    Sex Sex,
    string? Color,
    PetStatus Status,
    PetProfileVisibility Visibility,
    int FollowersCount,
    int PostsCount,
    DateTime CreatedAt
) : IPetProfileResponse;
