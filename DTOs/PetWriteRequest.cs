using System.ComponentModel.DataAnnotations;
using aRevolucaoDosPetWeb.Enums;
using aRevolucaoDosPetWeb.Models;

namespace aRevolucaoDosPetWeb.DTOs;

public class PetWriteRequest
{
    [Required]
    [StringLength(120, MinimumLength = 1)]
    public string Name { get; set; } = string.Empty;

    [RegularExpression("^@?[a-zA-Z0-9._]{3,30}$")]
    public string? Username { get; set; }

    [StringLength(5000)]
    public string? Bio { get; set; }

    [Url]
    [StringLength(2048)]
    public string? AvatarUrl { get; set; }

    [Url]
    [StringLength(2048)]
    public string? CoverUrl { get; set; }

    [EnumDataType(typeof(Species))]
    public required Species Species { get; set; }

    [StringLength(120)]
    public string? Breed { get; set; }

    [EnumDataType(typeof(Sex))]
    public Sex Sex { get; set; } = Sex.UNKNOWN;

    [StringLength(100)]
    public string? Color { get; set; }

    public DateTime? BirthDate { get; set; }
    public DateTime? AdoptionDate { get; set; }

    [Range(0.01, 1000)]
    public decimal? WeightKg { get; set; }

    [EnumDataType(typeof(PetSize))]
    public PetSize? Size { get; set; }

    [StringLength(100)]
    public string? Microchip { get; set; }

    [StringLength(150)]
    public string? Pedigree { get; set; }

    public bool IsNeutered { get; set; }
    public bool IsBreeder { get; set; }
    public List<string> Allergies { get; set; } = [];
    public List<string> ChronicConditions { get; set; } = [];
    public List<PetMedicationDto> ContinuousMedications { get; set; } = [];

    [StringLength(150)]
    public string? VetName { get; set; }

    [StringLength(30)]
    public string? VetPhone { get; set; }

    [StringLength(150)]
    public string? EmergencyContactName { get; set; }

    [StringLength(30)]
    public string? EmergencyContactPhone { get; set; }

    [StringLength(150)]
    public string? HealthPlan { get; set; }

    [StringLength(100)]
    public string? HealthPlanNumber { get; set; }

    [EnumDataType(typeof(PetStatus))]
    public PetStatus Status { get; set; } = PetStatus.ALIVE;

    [EnumDataType(typeof(PetProfileVisibility))]
    public PetProfileVisibility Visibility { get; set; } = PetProfileVisibility.PUBLIC;

    public UserLocationRequest? Location { get; set; }

    public List<string>? CoTutorIds { get; set; }
}
