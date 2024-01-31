using Blazored.LocalStorage;
using BootstrapBlazorCRD.Shared.Auth;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;

namespace BootstrapBlazorCRD.Shared.Service;

public class AuthService : IAuthService
{
    private readonly ILocalStorageService _localStorage;
    private readonly AuthenticationStateProvider _authenticationStateProvider;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    public AuthService(ILocalStorageService localStorageService, AuthenticationStateProvider authenticationStateProvider, IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _localStorage = localStorageService;
        _authenticationStateProvider = authenticationStateProvider;
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    public async Task<bool> LoginAsync(string? strEmployeeID)
    {
        bool result = false;
        string WebAPIUrl = _configuration.GetValue<string>("WebAPIUrl") + "TokenAuth";

        var request = new HttpRequestMessage(HttpMethod.Get, WebAPIUrl + "/" + strEmployeeID);
        request.Headers.Add("Accept", "application/json");

        var client = _httpClientFactory?.CreateClient();
        var response = await client.SendAsync(request);

        if (response.IsSuccessStatusCode)
        {
            string token = response.Content.ReadAsStringAsync().Result.Replace(@"""", "");
            await _localStorage.SetItemAsync<string>("token", token);
            ((CustomAuthStateProvider)_authenticationStateProvider).NotifyUserAuthentication(token);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Replace("\"", ""));
            result = true;
        }

        return result;
    }

    public async Task LogoutAsync()
    {
        if (await _localStorage.ContainKeyAsync("token"))
        {
            await _localStorage.RemoveItemAsync("token");
            ((CustomAuthStateProvider)_authenticationStateProvider).NotifyUserLogOut();
        }      
    }
}
