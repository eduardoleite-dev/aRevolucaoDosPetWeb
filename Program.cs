using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using aRevolucaoDosPetWeb.Interfaces;
using aRevolucaoDosPetWeb.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
void ConfigureApiClient(HttpClient client)
{
    client.BaseAddress = new Uri(builder.Configuration["ApiBaseUrl"]
        ?? "https://arevolucaodospet-dkf9hxc5d4dbgve2.chilecentral-01.azurewebsites.net/");
}

builder.Services.AddHttpClient<IApiConsumoService, ApiConsumoService>(ConfigureApiClient);
builder.Services.AddHttpClient<IApiConsumoUserService, ApiConsumoUserService>(ConfigureApiClient);
builder.Services.AddHttpClient<IApiConsumoPetService, ApiConsumoPetService>(ConfigureApiClient);
builder.Services.AddScoped<ISessionService, SessionService>();
builder.Services.AddScoped<IAuthService, AuthService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");
app.Run();
