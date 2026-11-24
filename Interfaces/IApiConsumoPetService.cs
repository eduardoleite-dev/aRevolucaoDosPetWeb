using aRevolucaoDosPetWeb.DTOs;
using aRevolucaoDosPetWeb.Models;

namespace aRevolucaoDosPetWeb.Interfaces;

public interface IApiConsumoPetService
{
    Task<(bool Success, PagedResult<PetResponseDto>? Result, string Message)> SearchAsync(
        PetSearchFilter filter,
        string token,
        CancellationToken cancellationToken = default);

    Task<(bool Success, IPetProfileResponse? Profile, string Message)> GetByIdAsync(
        string id,
        string? token = null,
        CancellationToken cancellationToken = default);

    Task<(bool Success, PetResponseDto? Pet, string Message)> CreateAsync(
        CreatePetRequest request,
        string token,
        CancellationToken cancellationToken = default);

    Task<(bool Success, PetResponseDto? Pet, string Message)> UpdateAsync(
        string id,
        PetWriteRequest request,
        string token,
        CancellationToken cancellationToken = default);

    Task<(bool Success, string Message)> DeleteAsync(
        string id,
        string token,
        CancellationToken cancellationToken = default);
}
