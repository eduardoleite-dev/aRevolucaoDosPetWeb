using Microsoft.AspNetCore.Components;
using aRevolucaoDosPetWeb.Interfaces;
using aRevolucaoDosPetWeb.Services;

namespace aRevolucaoDosPetWeb.Pages;

public partial class LoginPage : ComponentBase
{
    [Inject] protected IAuthService Auth { get; set; } = default!;
    [Inject] protected NavigationManager Navigation { get; set; } = default!;

    protected string username = "tutor@revolucaopet.com";
    protected string password = "petlover";
    protected string? errorMessage;
    protected bool loading;
    protected bool showPassword;

    protected void TogglePassword() => showPassword = !showPassword;
    protected void GoToRegister() => Navigation.NavigateTo("/cadastro");

    protected async Task HandleLoginAsync()
    {
        errorMessage = null;
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            errorMessage = "Informe usuário e senha.";
            return;
        }

        loading = true;
        try
        {
            if (await Auth.LoginAsync(username, password))
                Navigation.NavigateTo("/");
            else
                errorMessage = "Usuário ou senha inválidos.";
        }
        catch
        {
            errorMessage = "Não foi possível conectar à API.";
        }
        finally
        {
            loading = false;
        }
    }
}