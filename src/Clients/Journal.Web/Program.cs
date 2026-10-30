using Blazored.LocalStorage;
using Journal.Web.Auth;
using Journal.Web.Components;
using Microsoft.AspNetCore.Components.Authorization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// 1. Register your custom handler
builder.Services.AddTransient<AuthenticationHeaderHandler>();

// 2. Register the HttpClient and attach the interceptor
builder.Services.AddHttpClient("JournalApi", client =>
{
    client.BaseAddress = new Uri("http://localhost:5194");
})
.AddHttpMessageHandler<AuthenticationHeaderHandler>();

// 3. Make this customized client the default whenever you inject HttpClient into a page
builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("JournalApi"));

// 4. Register Auth and LocalStorage
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
