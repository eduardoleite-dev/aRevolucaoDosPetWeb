using System.ComponentModel.DataAnnotations;
using aRevolucaoDosPetWeb.Enums;

namespace aRevolucaoDosPetWeb.DTOs;

public class PetSearchFilter
{
    [Range(1, 1_000_000)]
    public int PageNumber { get; set; } = 1;

    [Range(1, 100)]
    public int PageSize { get; set; } = 20;

    public string? Id { get; set; }
    public string? OwnerId { get; set; }
    public string? CoTutorId { get; set; }
    public string? Name { get; set; }
    public string? Username { get; set; }
    public string? Bio { get; set; }
    public string? AvatarUrl { get; set; }
    public string? CoverUrl { get; set; }
    public Species? Species { get; set; }
    public string? Breed { get; set; }
    public Sex? Sex { get; set; }
    public string? Color { get; set; }
    public DateTime? BirthDateFrom { get; set; }
    public DateTime? BirthDateTo { get; set; }
    public DateTime? AdoptionDateFrom { get; set; }
    public DateTime? AdoptionDateTo { get; set; }

    [Range(0, 1000)]
    public decimal? MinWeightKg { get; set; }

    [Range(0, 1000)]
    public decimal? MaxWeightKg { get; set; }

    public PetSize? Size { get; set; }
    public string? Microchip { get; set; }
    public string? Pedigree { get; set; }
    public bool? IsNeutered { get; set; }
    public bool? IsBreeder { get; set; }
    public string? Allergy { get; set; }
    public string? ChronicCondition { get; set; }
    public string? MedicationName { get; set; }
    public string? VetName { get; set; }
    public string? VetPhone { get; set; }
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }
    public string? HealthPlan { get; set; }
    public string? HealthPlanNumber { get; set; }
    public PetStatus? Status { get; set; }
    public PetProfileVisibility? Visibility { get; set; }
    public bool? HasLocation { get; set; }

    [Range(-180, 180)]
    public double? Longitude { get; set; }

    [Range(-90, 90)]
    public double? Latitude { get; set; }

    [Range(0.01, 20_000_000)]
    public double? RadiusMeters { get; set; }

    [Range(0, int.MaxValue)]
    public int? MinFollowersCount { get; set; }

    [Range(0, int.MaxValue)]
    public int? MaxFollowersCount { get; set; }

    [Range(0, int.MaxValue)]
    public int? MinFollowingCount { get; set; }

    [Range(0, int.MaxValue)]
    public int? MaxFollowingCount { get; set; }

    [Range(0, int.MaxValue)]
    public int? MinPostsCount { get; set; }

    [Range(0, int.MaxValue)]
    public int? MaxPostsCount { get; set; }

    public DateTime? CreatedAtFrom { get; set; }
    public DateTime? CreatedAtTo { get; set; }
    public DateTime? UpdatedAtFrom { get; set; }
    public DateTime? UpdatedAtTo { get; set; }
    public DateTime? DeletedAtFrom { get; set; }
    public DateTime? DeletedAtTo { get; set; }
    public bool IncludeDeleted { get; set; }
}
