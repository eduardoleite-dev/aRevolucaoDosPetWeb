using Microsoft.AspNetCore.Components.Forms;
using aRevolucaoDosPetWeb.Models;

namespace aRevolucaoDosPetWeb.Interfaces;

public interface IApiConsumoUserService
{
    Task<LoginResponse?> LoginAsync(LoginRequest loginRequest);
    Task<UserProfileResponse?> GetUserAsync(string userId, string token);
    Task<(bool Success, string Message)> RegisterUserAsync(RegisterUserRequest registerRequest);
    Task<(bool Success, string? Url, string Message)> UploadMediaAsync(
        IBrowserFile file, string name = "profile-photo");
    Task<(bool Success, UserProfileResponse? Profile, string Message)> UpdateUserPhotoAsync(
        string userId, IBrowserFile file, string token);
    Task<(bool Success, UserProfileResponse? Profile, string Message)> UpdateUserAsync(
        string userId, UpdateUserRequest update, string token);
}