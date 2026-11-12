using Microsoft.AspNetCore.Components;
using aRevolucaoDosPetWeb.Interfaces;
using aRevolucaoDosPetWeb.Models;
using aRevolucaoDosPetWeb.Services;

namespace aRevolucaoDosPetWeb.Pages;

public partial class IndexPage : ComponentBase
{
    [Inject] protected IApiConsumoService Api { get; set; } = default!;
    [Inject] protected IApiConsumoUserService UserApi { get; set; } = default!;
    [Inject] protected ISessionService Session { get; set; } = default!;
    [Inject] protected NavigationManager Navigation { get; set; } = default!;

    protected bool publicLoading;
    protected bool privateLoading;
    protected bool profileMenuOpen;
    protected string? publicMessage;
    protected string? privateMessage;
    protected SessionData? SessionData { get; private set; }
    protected UserProfileResponse? Profile { get; private set; }
    protected bool HasSession => SessionData is not null;
    protected string DisplayName => Profile?.Name ?? SessionData?.Username ?? "Tutor";
    protected string UserInitial => string.IsNullOrWhiteSpace(DisplayName) ? "U" : DisplayName[..1].ToUpperInvariant();

    protected void ToggleProfileMenu() => profileMenuOpen = !profileMenuOpen;

    protected void OpenProfile()
    {
        profileMenuOpen = false;
        Navigation.NavigateTo("/perfil");
    }

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
            Profile = await UserApi.GetUserAsync(SessionData.UserId, SessionData.Token);
        }
        catch
        {
            Profile = null;
        }

        await LoadPublicAsync();
        StateHasChanged();
    }

    protected async Task LoadPublicAsync()
    {
        publicLoading = true;
        publicMessage = await Api.GetPublicMessageAsync();
        publicLoading = false;
    }

    protected async Task LoadPrivateAsync()
    {
        SessionData ??= await Session.GetAsync();
        if (SessionData is null)
        {
            Navigation.NavigateTo("/login", forceLoad: true);
            return;
        }

        privateLoading = true;
        privateMessage = await Api.GetPrivateMessageAsync(SessionData.Token);
        privateLoading = false;
    }

    protected async Task LogoutAsync()
    {
        await Session.ClearAsync();
        Navigation.NavigateTo("/login", forceLoad: true);
    }
}