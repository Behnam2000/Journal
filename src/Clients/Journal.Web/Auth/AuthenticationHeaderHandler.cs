using System.Net.Http.Headers;
using Blazored.LocalStorage;

namespace Journal.Web.Auth;

public class AuthenticationHeaderHandler : DelegatingHandler
{
    private readonly ILocalStorageService _localStorage;

    public AuthenticationHeaderHandler(ILocalStorageService localStorage)
    {
        _localStorage = localStorage;

    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        try
        {
            // Retrieve the JWT from the browser;s local storage
            var token = await _localStorage.GetItemAsync<string>("authToken");

            // If the token exists, attach it to the Authorization header
            if (!string.IsNullOrWhiteSpace(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }
        catch (InvalidOperationException)
        {
            // JavaScript interop is not available during static prerendering.
            // Silently catch the error so the HTTP request can proceed without a token.
        }

        // Continue sending the HTTP request to the API
        return await base.SendAsync(request, cancellationToken);

    }
}