using Microsoft.AspNetCore.Components;
using aRevolucaoDosPetWeb.Models;
using aRevolucaoDosPetWeb.Services;

namespace aRevolucaoDosPetWeb.Pages;

public partial class PerfilPage : ComponentBase
{
    [Inject] protected ApiService Api { get; set; } = default!;
    [Inject] protected SessionService Session { get; set; } = default!;
    [Inject] protected NavigationManager Navigation { get; set; } = default!;

    protected SessionData? SessionData { get; private set; }
    protected UserProfileResponse? Profile { get; private set; }
    protected bool Loading { get; private set; } = true;
    protected string? ErrorMessage;

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

    protected async Task LogoutAsync()
    {
        await Session.ClearAsync();
        Navigation.NavigateTo("/login", forceLoad: true);
    }
}
