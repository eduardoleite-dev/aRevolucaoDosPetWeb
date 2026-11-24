using aRevolucaoDosPetWeb.Enums;

namespace aRevolucaoDosPetWeb.DTOs;

public interface IPetProfileResponse
{
    string Id { get; }
    string Name { get; }
    string? Username { get; }
    string? Bio { get; }
    string? AvatarUrl { get; }
    string? CoverUrl { get; }
    Species Species { get; }
    string? Breed { get; }
    Sex Sex { get; }
    string? Color { get; }
    PetStatus Status { get; }
    PetProfileVisibility Visibility { get; }
    int FollowersCount { get; }
    int PostsCount { get; }
    DateTime CreatedAt { get; }
}
