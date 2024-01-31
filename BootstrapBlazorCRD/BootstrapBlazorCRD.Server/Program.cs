using Blazored.LocalStorage;
using System.Text;
using BootstrapBlazorCRD.Shared.Service;
using Microsoft.AspNetCore.Components.Authorization;
using BootstrapBlazorCRD.Shared.Auth;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

// 增加 Bootstrap Blazor 组件
builder.Services.AddBootstrapBlazor();

// 增加 BlazoredLocalStorage 组件
builder.Services.AddBlazoredLocalStorage();

// 增加 authentication authorization 组件
builder.Services.AddHttpContextAccessor();
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthenticationStateProvider,CustomAuthStateProvider>();
builder.Services.AddScoped<IAuthService,AuthService>();

// 增加 CustomSpinner class(SpinnerService.cs)
builder.Services.AddScoped<SpinnerService>();

// 增加 UserService class(UserService.cs)
builder.Services.AddScoped<IUserService,UserService>();

var app = builder.Build();

app.UsePathBase("/CRD");

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error");
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();