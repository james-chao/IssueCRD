using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;

namespace BootstrapBlazorCRD.Shared.Auth;
public class CustomAuthStateProvider : AuthenticationStateProvider
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILocalStorageService _localStorage;
    private AuthenticationState? anonymous;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CustomAuthStateProvider(ILocalStorageService localStorage, IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
    {
        _httpClientFactory = httpClientFactory;
        _localStorage = localStorage;     
        _httpContextAccessor = httpContextAccessor;     
    }
    
    public override async Task<AuthenticationState> GetAuthenticationStateAsync()    
    {        
        string? token = await _localStorage.GetItemAsStringAsync("token");
        var client = _httpClientFactory.CreateClient();

        if (string.IsNullOrEmpty(token?.Replace("\"", "")))
        {   
            var Name = _httpContextAccessor?.HttpContext?.User.Identity?.Name;

            if (string.IsNullOrEmpty(Name))
            {
                anonymous = new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, String.Empty) }, "anonymous")));
            }
            else
            {
                anonymous = new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, Name.Split('\\')[1].ToString()) }, "Windows")));
            }
            return anonymous;
        }

        //將token取出轉為claim
        var claims = ParseClaimsFromJwt(token);
        //在每次request的header中帶入bearer token
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Replace("\"", ""));
        //回傳帶有user claim的AuthenticationState物件
        return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity(claims, "jwt")));
    }

    public void NotifyUserAuthentication(string token)
    {
        var claims = ParseClaimsFromJwt(token);
        var authenticatedUser = new ClaimsPrincipal(new ClaimsIdentity(claims, "jwt"));
        var authState = Task.FromResult(new AuthenticationState(authenticatedUser));
        NotifyAuthenticationStateChanged(authState);
    }

    public void NotifyUserLogOut()
    {
        var authState = Task.FromResult(anonymous);
        NotifyAuthenticationStateChanged(authState!);
    }
    public static IEnumerable<Claim> ParseClaimsFromJwt(string token)
    {
        var claims = new List<Claim>();
        var payload = token.Split('.')[1];
        var jsonBytes = ParseBase64WithoutPadding(payload);
        var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);

        keyValuePairs!.TryGetValue("roles", out object? roles);
        keyValuePairs.TryGetValue("sub", out object? users);
        keyValuePairs.TryGetValue("nameid", out object? email);

        if (roles != null)
        {
            if (roles.ToString()!.Trim().StartsWith("["))
            {
                var parsedRoles = JsonSerializer.Deserialize<string[]>(roles.ToString()!);

                foreach (var parsedRole in parsedRoles!)
                {
                    claims.Add(new Claim(ClaimTypes.Role, parsedRole));
                }
            }
            else
            {
                claims.Add(new Claim(ClaimTypes.Role, roles.ToString()!));
            }

            keyValuePairs.Remove("roles");
        }

        if (users != null)
        {
            claims.Add(new Claim(ClaimTypes.Name, users.ToString()!));

            keyValuePairs.Remove("sub");
        }

        if (email != null)
        {
            claims.Add(new Claim(ClaimTypes.Name, email.ToString()!));

            keyValuePairs.Remove("nameid");
        }

        claims.AddRange(keyValuePairs.Select(kvp => new Claim(kvp.Key, kvp.Value.ToString()!)));

        return claims;
    }

    private static byte[] ParseBase64WithoutPadding(string base64)
    {
        switch (base64.Length % 4)
        {
            case 2: base64 += "=="; break;
            case 3: base64 += "="; break;
        }
        return Convert.FromBase64String(base64);
    }
}
