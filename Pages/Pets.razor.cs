using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using aRevolucaoDosPetWeb.DTOs;
using aRevolucaoDosPetWeb.Enums;
using aRevolucaoDosPetWeb.Interfaces;
using aRevolucaoDosPetWeb.Models;

namespace aRevolucaoDosPetWeb.Pages;

public partial class PetsPage : ComponentBase
{
    private const int PageSize = 20;

    [Inject] protected IApiConsumoPetService PetApi { get; set; } = default!;
    [Inject] protected ISessionService Session { get; set; } = default!;
    [Inject] protected NavigationManager Navigation { get; set; } = default!;

    protected SessionData? SessionData { get; private set; }
    protected List<PetResponseDto> Pets { get; private set; } = [];
    protected CreatePetRequest Form { get; private set; } = NewPetForm();
    protected PetResponseDto? PendingDeletePet { get; private set; }
    protected string SearchName { get; set; } = string.Empty;
    protected string AllergiesText { get; set; } = string.Empty;
    protected string ChronicConditionsText { get; set; } = string.Empty;
    protected string CoTutorIdsText { get; set; } = string.Empty;
    protected string? EditingPetId { get; private set; }
    protected string? ErrorMessage { get; private set; }
    protected string? SuccessMessage { get; private set; }
    protected string? FormError { get; private set; }
    protected string? DeleteError { get; private set; }
    protected bool Loading { get; private set; } = true;
    protected bool FormOpen { get; private set; }
    protected bool Saving { get; private set; }
    protected bool Deleting { get; private set; }
    protected int PageNumber { get; private set; } = 1;
    protected long TotalCount { get; private set; }
    protected int PageCount => TotalCount == 0 ? 1 : (int)Math.Ceiling((double)TotalCount / PageSize);
    protected bool HasNextPage => (long)PageNumber * PageSize < TotalCount;

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

        await LoadPetsAsync();
        StateHasChanged();
    }

    protected void BackToHome() => Navigation.NavigateTo("/");

    protected async Task SearchAsync()
    {
        PageNumber = 1;
        SuccessMessage = null;
        await LoadPetsAsync();
    }

    protected async Task PreviousPageAsync()
    {
        if (PageNumber <= 1)
            return;

        PageNumber--;
        await LoadPetsAsync();
    }

    protected async Task NextPageAsync()
    {
        if (!HasNextPage)
            return;

        PageNumber++;
        await LoadPetsAsync();
    }

    protected async Task LoadPetsAsync()
    {
        if (SessionData is null)
            return;

        Loading = true;
        ErrorMessage = null;
        try
        {
            var filter = new PetSearchFilter
            {
                PageNumber = PageNumber,
                PageSize = PageSize,
                OwnerId = SessionData.UserId,
                Name = string.IsNullOrWhiteSpace(SearchName) ? null : SearchName.Trim()
            };
            var result = await PetApi.SearchAsync(filter, SessionData.Token);
            if (!result.Success || result.Result is null)
            {
                Pets = [];
                TotalCount = 0;
                ErrorMessage = result.Message;
                return;
            }

            Pets = result.Result.Items?.ToList() ?? [];
            TotalCount = result.Result.TotalCount;
        }
        finally
        {
            Loading = false;
        }
    }

    protected void OpenCreateForm()
    {
        EditingPetId = null;
        Form = NewPetForm();
        AllergiesText = string.Empty;
        ChronicConditionsText = string.Empty;
        CoTutorIdsText = string.Empty;
        FormError = null;
        FormOpen = true;
    }

    protected void OpenEditForm(PetResponseDto pet)
    {
        EditingPetId = pet.Id;
        Form = new CreatePetRequest
        {
            OwnerId = pet.OwnerId,
            Name = pet.Name,
            Username = pet.Username,
            Bio = pet.Bio,
            AvatarUrl = pet.AvatarUrl,
            CoverUrl = pet.CoverUrl,
            Species = pet.Species,
            Breed = pet.Breed,
            Sex = pet.Sex,
            Color = pet.Color,
            BirthDate = pet.BirthDate,
            AdoptionDate = pet.AdoptionDate,
            WeightKg = pet.WeightKg,
            Size = pet.Size,
            Microchip = pet.Microchip,
            Pedigree = pet.Pedigree,
            IsNeutered = pet.IsNeutered,
            IsBreeder = pet.IsBreeder,
            Allergies = pet.Allergies?.ToList() ?? [],
            ChronicConditions = pet.ChronicConditions?.ToList() ?? [],
            ContinuousMedications = pet.ContinuousMedications?.ToList() ?? [],
            VetName = pet.VetName,
            VetPhone = pet.VetPhone,
            EmergencyContactName = pet.EmergencyContactName,
            EmergencyContactPhone = pet.EmergencyContactPhone,
            HealthPlan = pet.HealthPlan,
            HealthPlanNumber = pet.HealthPlanNumber,
            Status = pet.Status,
            Visibility = pet.Visibility,
            Location = pet.Location is { Coordinates: not null } location
                ? new UserLocationRequest(location.Type ?? "Point", location.Coordinates)
                : null,
            CoTutorIds = pet.CoTutorIds?.ToList()
        };
        AllergiesText = string.Join(Environment.NewLine, Form.Allergies);
        ChronicConditionsText = string.Join(Environment.NewLine, Form.ChronicConditions);
        CoTutorIdsText = string.Join(Environment.NewLine, Form.CoTutorIds ?? []);
        FormError = null;
        FormOpen = true;
    }

    protected void CloseForm()
    {
        if (Saving)
            return;

        FormOpen = false;
        FormError = null;
    }

    protected async Task SavePetAsync(EditContext _)
    {
        if (SessionData is null)
            return;

        Form.Allergies = ParseLines(AllergiesText);
        Form.ChronicConditions = ParseLines(ChronicConditionsText);
        Form.CoTutorIds = ParseLines(CoTutorIdsText);
        FormError = null;
        SuccessMessage = null;
        Saving = true;
        try
        {
            var result = EditingPetId is null
                ? await PetApi.CreateAsync(Form, SessionData.Token)
                : await PetApi.UpdateAsync(EditingPetId, Form, SessionData.Token);

            if (!result.Success)
            {
                FormError = result.Message;
                return;
            }

            FormOpen = false;
            SuccessMessage = EditingPetId is null
                ? "Pet cadastrado com sucesso."
                : "Pet atualizado com sucesso.";
            PageNumber = 1;
            await LoadPetsAsync();
        }
        finally
        {
            Saving = false;
        }
    }

    protected void RequestDelete(PetResponseDto pet)
    {
        PendingDeletePet = pet;
        DeleteError = null;
    }

    protected void CancelDelete()
    {
        if (Deleting)
            return;

        PendingDeletePet = null;
        DeleteError = null;
    }

    protected async Task DeletePetAsync()
    {
        if (SessionData is null || PendingDeletePet is null)
            return;

        Deleting = true;
        DeleteError = null;
        try
        {
            var result = await PetApi.DeleteAsync(PendingDeletePet.Id, SessionData.Token);
            if (!result.Success)
            {
                DeleteError = result.Message;
                return;
            }

            PendingDeletePet = null;
            SuccessMessage = "Pet excluído com sucesso.";
            if (Pets.Count == 1 && PageNumber > 1)
                PageNumber--;
            await LoadPetsAsync();
        }
        finally
        {
            Deleting = false;
        }
    }

    private static CreatePetRequest NewPetForm() => new()
    {
        Species = Species.DOG
    };

    private static List<string> ParseLines(string value) =>
        value.Split(
                new[] { '\r', '\n', ',' },
                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToList();
}
