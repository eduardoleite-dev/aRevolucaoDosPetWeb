using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using aRevolucaoDosPetWeb.Models;
using aRevolucaoDosPetWeb.Services;

namespace aRevolucaoDosPetWeb.Pages;

public partial class PerfilPage : ComponentBase, IAsyncDisposable
{
    [Inject] protected ApiService Api { get; set; } = default!;
    [Inject] protected SessionService Session { get; set; } = default!;
    [Inject] protected NavigationManager Navigation { get; set; } = default!;
    [Inject] protected IJSRuntime JSRuntime { get; set; } = default!;

    protected SessionData? SessionData { get; private set; }
    protected UserProfileResponse? Profile { get; private set; }
    protected bool Loading { get; private set; } = true;
    protected string? ErrorMessage;
    protected bool Editing { get; private set; }
    protected bool SavingProfile { get; private set; }
    protected bool SavingPhoto { get; private set; }
    protected string EditName { get; set; } = string.Empty;
    protected string EditEmail { get; set; } = string.Empty;
    protected string EditPhone { get; set; } = string.Empty;
    protected string EditCity { get; set; } = string.Empty;
    protected string EditState { get; set; } = string.Empty;
    protected string EditCountry { get; set; } = string.Empty;
    protected string EditBio { get; set; } = string.Empty;
    protected IBrowserFile? SelectedPhoto { get; private set; }
    protected string? PhotoPreviewUrl { get; private set; }
    protected string? PhotoMessage { get; private set; }
    protected bool PhotoSucceeded { get; private set; }
    protected string? ProfileMessage { get; private set; }
    protected bool ProfileSucceeded { get; private set; }

    protected string DisplayName => Profile?.Name ?? SessionData?.Username ?? "Usuário";
    protected string UserInitial => string.IsNullOrWhiteSpace(DisplayName) ? "U" : DisplayName[..1].ToUpperInvariant();
    protected string AccessLevelName => Profile?.AccessLevel switch { 0 => "Tutor", 1 => "Administrador", _ => $"Nível {Profile?.AccessLevel}" };

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender)
            return;

        SessionData = await Session.GetAsync();
        if (SessionData is null)
        {
            Navigation.NavigateTo("/login", forceLoad: true);
            return;
        }

        try
        {
            Profile = await Api.GetUserAsync(SessionData.UserId, SessionData.Token);
            if (Profile is null)
                ErrorMessage = "Não foi possível carregar os dados deste usuário.";
        }
        catch
        {
            ErrorMessage = "Erro ao consultar o perfil na API.";
        }
        finally
        {
            Loading = false;
            StateHasChanged();
        }
    }

    protected void BackToHome() => Navigation.NavigateTo("/");

    protected void OpenEditForm()
    {
        if (Profile is null)
            return;

        EditName = Profile.Name ?? string.Empty;
        EditEmail = Profile.Email ?? string.Empty;
        EditPhone = Profile.Phone ?? string.Empty;
        EditCity = Profile.City ?? string.Empty;
        EditState = Profile.State ?? string.Empty;
        EditCountry = Profile.Country ?? string.Empty;
        EditBio = Profile.Bio ?? string.Empty;
        ProfileMessage = null;
        Editing = true;
    }

    protected void CloseEditForm() => Editing = false;

    protected async Task SaveProfileAsync()
    {
        if (Profile is null || SessionData is null)
            return;

        ProfileMessage = null;
        SavingProfile = true;
        try
        {
            var update = new UpdateUserRequest(
                EditName.Trim(), EditEmail.Trim(), Profile.AvatarUrl,
                EmptyToNull(EditBio), EmptyToNull(EditPhone), EmptyToNull(EditCity),
                EmptyToNull(EditState), EmptyToNull(EditCountry)?.ToUpperInvariant(),
                Profile.Location is { Coordinates.Length: 2 } point
                    ? new UserLocationRequest(point.Type ?? "Point", point.Coordinates!)
                    : null);

            var result = await Api.UpdateUserAsync(SessionData.UserId, update, SessionData.Token);
            ProfileSucceeded = result.Success;
            ProfileMessage = result.Message;
            if (result.Success && result.Profile is not null)
            {
                Profile = result.Profile;
                Editing = false;
            }
        }
        finally
        {
            SavingProfile = false;
        }
    }

    protected async Task OnPhotoSelectedAsync(InputFileChangeEventArgs eventArgs)
    {
        await ClearPhotoPreviewAsync();
        SelectedPhoto = null;
        PhotoMessage = null;
        PhotoSucceeded = false;

        var file = eventArgs.File;
        if (!file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
        {
            PhotoMessage = "Selecione um arquivo de imagem.";
            return;
        }
        if (file.Size > 10 * 1024 * 1024)
        {
            PhotoMessage = "A foto deve ter no máximo 10 MB.";
            return;
        }

        SelectedPhoto = file;
        PhotoPreviewUrl = await JSRuntime.InvokeAsync<string?>("petAvatarPreview.create", "profile-photo-file");
    }

    protected async Task UpdatePhotoAsync()
    {
        if (SelectedPhoto is null || SessionData is null)
            return;

        SavingPhoto = true;
        PhotoMessage = null;
        try
        {
            var result = await Api.UpdateUserPhotoAsync(SessionData.UserId, SelectedPhoto, SessionData.Token);
            PhotoSucceeded = result.Success;
            PhotoMessage = result.Message;
            if (result.Success && result.Profile is not null)
            {
                Profile = result.Profile;
                SelectedPhoto = null;
                await ClearPhotoPreviewAsync();
            }
        }
        finally
        {
            SavingPhoto = false;
        }
    }

    protected async Task CancelPhotoSelection()
    {
        SelectedPhoto = null;
        PhotoMessage = null;
        await ClearPhotoPreviewAsync();
    }

    private async Task ClearPhotoPreviewAsync()
    {
        PhotoPreviewUrl = null;
        try { await JSRuntime.InvokeVoidAsync("petAvatarPreview.clear"); }
        catch (JSDisconnectedException) { }
    }

    public async ValueTask DisposeAsync() => await ClearPhotoPreviewAsync();

    private static string? EmptyToNull(string value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    protected async Task LogoutAsync()
    {
        await Session.ClearAsync();
        Navigation.NavigateTo("/login", forceLoad: true);
    }
}
