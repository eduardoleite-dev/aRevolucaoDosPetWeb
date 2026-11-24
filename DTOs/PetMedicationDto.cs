using System.ComponentModel.DataAnnotations;

namespace aRevolucaoDosPetWeb.DTOs;

public class PetMedicationDto
{
    [Required]
    [StringLength(150)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Dosage { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Frequency { get; set; } = string.Empty;

    public DateTime StartedAt { get; set; }
}
