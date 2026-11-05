using Microsoft.AspNetCore.Components;
using aRevolucaoDosPetWeb.Models;
using aRevolucaoDosPetWeb.Services;

namespace aRevolucaoDosPetWeb.Pages.cadastro;

public partial class CadastroPage : ComponentBase
{
    [Inject] protected ApiService Api { get; set; } = default!;
    [Inject] protected NavigationManager Navigation { get; set; } = default!;

    protected string name = string.Empty;
    protected string email = string.Empty;
    protected string password = string.Empty;
    protected string confirmPassword = string.Empty;
    protected string avatarUrl = string.Empty;
    protected string bio = string.Empty;
    protected string phone = string.Empty;
    protected string city = string.Empty;
    protected string state = string.Empty;
    protected string country = "BR";
    protected string longitude = string.Empty;
    protected string latitude = string.Empty;
    protected bool showPassword;
    protected bool loading;
    protected string? errorMessage;
    protected string? successMessage;

    protected void TogglePassword() => showPassword = !showPassword;
    protected void GoToLogin() => Navigation.NavigateTo("/login");

    protected async Task HandleRegisterAsync()
    {
        errorMessage = null;
        successMessage = null;

        if (string.IsNullOrWhiteSpace(name) || name.Trim().Length < 2)
        { errorMessage = "Informe um nome com pelo menos 2 caracteres."; return; }
        if (string.IsNullOrWhiteSpace(email))
        { errorMessage = "Informe um e-mail válido."; return; }
        if (password.Length < 8)
        { errorMessage = "A senha deve ter pelo menos 8 caracteres."; return; }
        if (password != confirmPassword)
        { errorMessage = "As senhas não conferem."; return; }
        if (country.Length != 2)
        { errorMessage = "O país deve ter duas letras, por exemplo BR."; return; }

        UserLocationRequest? location = null;
        if (!string.IsNullOrWhiteSpace(longitude) || !string.IsNullOrWhiteSpace(latitude))
        {
            if (!double.TryParse(longitude, System.Globalization.CultureInfo.InvariantCulture, out var lon) ||
                !double.TryParse(latitude, System.Globalization.CultureInfo.InvariantCulture, out var lat))
            { errorMessage = "Longitude e latitude devem ser números válidos."; return; }
            location = new UserLocationRequest("Point", [lon, lat]);
        }

        var request = new RegisterUserRequest(
            name.Trim(), email.Trim(), password,
            EmptyToNull(avatarUrl), EmptyToNull(bio), EmptyToNull(phone),
            EmptyToNull(city), EmptyToNull(state), country.Trim().ToUpperInvariant(), location);

        loading = true;
        try
        {
            var result = await Api.RegisterUserAsync(request);
            if (result.Success)
            {
                successMessage = result.Message;
                await Task.Delay(900);
                Navigation.NavigateTo("/login");
            }
            else errorMessage = result.Message;
        }
        finally
        {
            loading = false;
        }
    }

    private static string? EmptyToNull(string value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
