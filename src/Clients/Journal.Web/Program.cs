using Blazored.LocalStorage;
using Journal.Web.Auth;
using Journal.Web.Components;
using Microsoft.AspNetCore.Components.Authorization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// 1. Register the handler strictly to the user's Blazor circuit
builder.Services.AddScoped<AuthenticationHeaderHandler>();

// 2. Manually construct the HttpClient to ensure it stays in the user's scope
builder.Services.AddScoped(sp =>
{
    var handler = sp.GetRequiredService<AuthenticationHeaderHandler>();
    // Attach the base network handler
    handler.InnerHandler = new HttpClientHandler();

    return new HttpClient(handler)
    {
        BaseAddress = new Uri("http://localhost:5194")
    };
});

// 3. Register Auth and LocalStorage
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthenticationStateProvider, JwtAuthStateProvider>();

// Make the AuthService available to your pages
builder.Services.AddScoped<Journal.Web.Services.AuthService>();

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