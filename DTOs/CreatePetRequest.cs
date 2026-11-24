namespace aRevolucaoDosPetWeb.DTOs;

public class CreatePetRequest : PetWriteRequest
{
    public string? OwnerId { get; set; }
}
