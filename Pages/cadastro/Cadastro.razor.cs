using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using aRevolucaoDosPetWeb.Models;
using aRevolucaoDosPetWeb.Services;
using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace aRevolucaoDosPetWeb.Pages.cadastro;

public partial class CadastroPage : ComponentBase, IAsyncDisposable
{
    private const long MaxAvatarSizeBytes = 10 * 1024 * 1024;

    [Inject] protected ApiService Api { get; set; } = default!;
    [Inject] protected NavigationManager Navigation { get; set; } = default!;
    [Inject] protected IJSRuntime JSRuntime { get; set; } = default!;
    [Inject] protected IHttpClientFactory HttpClientFactory { get; set; } = default!;

    protected string name = string.Empty;
    protected string email = string.Empty;
    protected string password = string.Empty;
    protected string confirmPassword = string.Empty;
    protected string avatarUrl = string.Empty;
    protected string? avatarPreviewUrl;
    protected IBrowserFile? avatarFile;
    protected string? avatarFileName;
    protected string? avatarErrorMessage;
    protected string bio = string.Empty;
    protected string phone = string.Empty;
    protected string city = string.Empty;
    protected string state = string.Empty;
    protected string country = "BR";
    protected string selectedStateId = string.Empty;
    protected List<CountryOption> countries = [];
    protected List<BrazilState> states = [];
    protected List<string> cities = [];
    protected bool loadingCities;
    protected string? cityMessage;
    protected string? locationMessage;
    protected bool showPassword;
    protected bool loading;
    protected string? errorMessage;
    protected string? successMessage;

    protected void TogglePassword() => showPassword = !showPassword;
    protected void GoToLogin() => Navigation.NavigateTo("/login");

    protected async Task OnAvatarSelected(InputFileChangeEventArgs eventArgs)
    {
        await JSRuntime.InvokeVoidAsync("petAvatarPreview.clear");
        avatarPreviewUrl = null;
        avatarFile = null;
        avatarFileName = null;
        avatarUrl = string.Empty;
        avatarErrorMessage = null;

        var file = eventArgs.File;
        if (!file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
        {
            avatarErrorMessage = "Selecione um arquivo de imagem.";
            return;
        }

        if (file.Size > MaxAvatarSizeBytes)
        {
            avatarErrorMessage = "A foto deve ter no máximo 10 MB.";
            return;
        }

        avatarFile = file;
        avatarFileName = file.Name;
        avatarPreviewUrl = await JSRuntime.InvokeAsync<string?>("petAvatarPreview.create", "avatar-file");
    }

    public async ValueTask DisposeAsync()
    {
        if (avatarPreviewUrl is null)
            return;

        try
        {
            await JSRuntime.InvokeVoidAsync("petAvatarPreview.clear");
        }
        catch (JSDisconnectedException) { }
    }

    protected override void OnInitialized()
    {
        countries = CultureInfo.GetCultures(CultureTypes.SpecificCultures)
            .Select(culture => new RegionInfo(culture.Name))
            .GroupBy(region => region.TwoLetterISORegionName, StringComparer.OrdinalIgnoreCase)
            .Select(group => new CountryOption(group.Key, group.First().NativeName))
            .OrderBy(option => option.Code == "BR" ? 0 : 1)
            .ThenBy(option => option.Name)
            .ToList();

        states =
        [
            new(12, "Acre", "AC"), new(27, "Alagoas", "AL"), new(16, "Amapá", "AP"),
            new(13, "Amazonas", "AM"), new(29, "Bahia", "BA"), new(23, "Ceará", "CE"),
            new(53, "Distrito Federal", "DF"), new(32, "Espírito Santo", "ES"), new(52, "Goiás", "GO"),
            new(21, "Maranhão", "MA"), new(51, "Mato Grosso", "MT"), new(50, "Mato Grosso do Sul", "MS"),
            new(31, "Minas Gerais", "MG"), new(15, "Pará", "PA"), new(25, "Paraíba", "PB"),
            new(41, "Paraná", "PR"), new(26, "Pernambuco", "PE"), new(22, "Piauí", "PI"),
            new(33, "Rio de Janeiro", "RJ"), new(24, "Rio Grande do Norte", "RN"),
            new(43, "Rio Grande do Sul", "RS"), new(11, "Rondônia", "RO"), new(14, "Roraima", "RR"),
            new(42, "Santa Catarina", "SC"), new(35, "São Paulo", "SP"), new(28, "Sergipe", "SE"),
            new(17, "Tocantins", "TO")
        ];
    }

    protected void OnCountryChanged()
    {
        selectedStateId = string.Empty;
        state = string.Empty;
        city = string.Empty;
        cities = [];
        cityMessage = null;
    }

    protected async Task OnStateChangedAsync(ChangeEventArgs eventArgs)
    {
        selectedStateId = eventArgs.Value?.ToString() ?? string.Empty;
        state = states.FirstOrDefault(item => item.Id.ToString() == selectedStateId)?.Abbreviation ?? string.Empty;
        city = string.Empty;
        cities = [];
        cityMessage = null;

        if (string.IsNullOrEmpty(selectedStateId))
            return;

        loadingCities = true;
        try
        {
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            try
            {
                var client = HttpClientFactory.CreateClient();
                var brazilCities = await client.GetFromJsonAsync<List<BrazilCity>>(
                    $"https://servicodados.ibge.gov.br/api/v1/localidades/estados/{selectedStateId}/municipios?orderBy=nome",
                    timeout.Token);
                cities = brazilCities?.Select(item => item.Name).ToList() ?? [];
            }
            catch (Exception exception) when (exception is HttpRequestException or OperationCanceledException)
            {
                cities = await JSRuntime.InvokeAsync<List<string>>(
                    "petLocations.getCities", selectedStateId) ?? [];
            }

            if (cities.Count == 0)
                cityMessage = "Não há cidades disponíveis para este estado.";
        }
        catch (Exception exception) when (exception is HttpRequestException or OperationCanceledException or JSException)
        {
            cityMessage = "Não foi possível carregar as cidades. Verifique sua conexão e tente novamente.";
        }
        finally
        {
            loadingCities = false;
        }
    }

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
        if (!string.IsNullOrWhiteSpace(avatarErrorMessage))
        { errorMessage = avatarErrorMessage; return; }

        loading = true;
        try
        {
            if (avatarFile is not null && string.IsNullOrWhiteSpace(avatarUrl))
            {
                var upload = await Api.UploadMediaAsync(avatarFile, "profile-photo");
                if (!upload.Success)
                {
                    errorMessage = upload.Message;
                    return;
                }

                avatarUrl = upload.Url?.Trim() ?? string.Empty;
                if (!Uri.TryCreate(avatarUrl, UriKind.Absolute, out _))
                {
                    errorMessage = "A API não retornou uma URL válida para a foto.";
                    return;
                }
            }

            UserLocationRequest? location = null;
            locationMessage = null;
            try
            {
                var coordinates = await JSRuntime.InvokeAsync<double?[]>("petLocation.getCurrentCoordinates");
                if (coordinates is { Length: 2 } && coordinates[0].HasValue && coordinates[1].HasValue &&
                    double.IsFinite(coordinates[0]!.Value) && double.IsFinite(coordinates[1]!.Value))
                    location = new UserLocationRequest("Point", [coordinates[0]!.Value, coordinates[1]!.Value]);
            }
            catch (JSException) { }

            if (location is null)
                locationMessage = "Não foi possível obter a localização do dispositivo; sua conta será criada sem coordenadas.";

            var request = new RegisterUserRequest(
                name.Trim(), email.Trim(), password,
                EmptyToNull(avatarUrl), EmptyToNull(bio), EmptyToNull(phone),
                EmptyToNull(city), EmptyToNull(state), country.Trim().ToUpperInvariant(), location);

            if (avatarFile is not null && string.IsNullOrWhiteSpace(request.AvatarUrl))
            {
                errorMessage = "A URL da foto não foi incluída no cadastro. Tente selecionar a imagem novamente.";
                return;
            }

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

    protected sealed record CountryOption(string Code, string Name);

    protected sealed record BrazilState(
        [property: JsonPropertyName("id")] int Id,
        [property: JsonPropertyName("nome")] string Name,
        [property: JsonPropertyName("sigla")] string Abbreviation);

    protected sealed record BrazilCity(
        [property: JsonPropertyName("nome")] string Name);
}
