using Blazored.LocalStorage;
using Journal.Web.Auth;
using Journal.Web.Components;
using Microsoft.AspNetCore.Components.Authorization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Configure HttpClient to point to your backend API
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("http://localhost:5194") });

// Register Auth and LocalStorage
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddAuthenticationCore();
builder.Services.AddScoped<AuthenticationStateProvider, JwtAuthStateProvider>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
