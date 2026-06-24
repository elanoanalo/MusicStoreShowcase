using MudBlazor.Services;
using MusicStoreShowcase.Components;
using MusicStoreShowcase.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMudServices();

builder.Services.AddSingleton<LocaleService>(_ =>
{
    string localesPath = Path.Combine(builder.Environment.WebRootPath, "locales");
    return new LocaleService(localesPath);
});

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapGet("/cover/{seed}/{index}", (ulong seed, int index, string title, string artist, string lang) =>
{
    byte[] png = MusicStoreShowcase.Models.CoverGenerator.GenerateCoverPng(seed, index, title ?? "", artist ?? "", lang ?? "");
    return Results.File(png, "image/png");
});

app.MapGet("/music/{seed}/{index}", (ulong seed, int index) =>
{
    byte[] midi = MusicStoreShowcase.Models.MusicGenerator.GenerateMidi(seed, index);
    return Results.File(midi, "audio/midi");
});

app.Run();