using System.ComponentModel.DataAnnotations;

namespace aRevolucaoDosPetWeb.DTOs;

public class PetFollowerSearchFilter
{
    [Range(1, 1_000_000)]
    public int PageNumber { get; set; } = 1;

    [Range(1, 100)]
    public int PageSize { get; set; } = 20;

    public string? FollowerUserId { get; set; }
    public string? FollowerPetId { get; set; }
}
