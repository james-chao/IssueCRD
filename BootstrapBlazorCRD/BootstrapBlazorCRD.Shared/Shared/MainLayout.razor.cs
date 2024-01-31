using BootstrapBlazor.Components;
using BootstrapBlazorCRD.Shared.Service;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Configuration;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Security.Claims;

namespace BootstrapBlazorCRD.Shared.Shared;
/// <summary>
/// MainLayout的construct
/// </summary>
public sealed partial class MainLayout
{
    private bool UseTabSet { get; set; } = true;

    private string Theme { get; set; } = "";

    private bool IsFixedHeader { get; set; } = true;

    private bool IsFixedFooter { get; set; } = true;

    private bool IsFullSide { get; set; } = true;

    private bool ShowFooter { get; set; } = true;

    private List<MenuItem>? Menus { get; set; }

    private string? UserName { get; set; }

    private string? Roles { get; set; }

    private ClaimsPrincipal? user = null;

    private bool result = false;

    private string? AppVersion { get; set; }

    [CascadingParameter]
    [NotNull]
    private Task<AuthenticationState>? AuthenticationStateTask { get; set; }

    [Inject]
    private NavigationManager? navigationManager { get; set; }

    [Inject]
    private IAuthService? authService { get; set; }

    [Inject]
    private IConfiguration? configuration { get; set; }

    //[Inject]
    //[NotNull]
    //private IDispatchService<MessageItem>? DispatchService { get; set; }

    [Inject]
    [NotNull]
    private ToastService? Toast { get; set; }

    protected override void OnInitialized()
    {
        base.OnInitialized();
        //DispatchService.Subscribe(Dispatch);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);
        user = (await AuthenticationStateTask).User;

        if (firstRender)
        {
            result = configuration.GetValue<bool>("IsDebug") ? true : false;
            Roles = configuration.GetValue<bool>("IsDebug") ? "ADMIN" : string.Empty;
            AppVersion = "Version:" + Assembly.GetEntryAssembly()!.GetCustomAttribute<AssemblyInformationalVersionAttribute>()!.InformationalVersion;

            if (user != null && user!.Identity!.IsAuthenticated)
            {
                if (!configuration.GetValue<bool>("IsDebug") && user.Identity.Name != string.Empty)
                {
                    string? strEmploueeID = user.Identity.Name;
                    result = await authService!.LoginAsync(strEmploueeID!);
                }

                if (result)
                {
                    Menus = GetIconSideMenuItems();
                    var userinfo = (await AuthenticationStateTask).User;
                    UserName = userinfo.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)!.Value;
                    Roles = userinfo?.Claims?.FirstOrDefault(c => c.Type == ClaimTypes.Role)!.Value;
                    navigationManager?.NavigateTo("./Admin");
                }
                else
                {
                    await authService!.LogoutAsync();
                    navigationManager?.NavigateTo("./NoPermission");
                }
            }
        }
    }
    private static List<MenuItem> GetIconSideMenuItems()
    {
        var menus = new List<MenuItem>
        {
            new MenuItem() { Text = "CRD Report", Icon = "fa-solid fa-fw fa-table", Url = "/CRDReport" },
            new MenuItem() { Text = "Admin", Icon = "fas fa-user-plus", Url = "/Admin" }
        };

        return menus;
    }

    //private async Task Dispatch(DispatchEntry<MessageItem> entry)
    //{
    //    if (entry.Entry != null)
    //    {
    //        await Toast.Show(new ToastOption()
    //        {
    //            Title = "CRD2.2 Messages Notification",
    //            Content = entry.Entry.Content,
    //            Category = ToastCategory.Information,
    //            Delay = 15 * 1000,//強迫通知15秒後自動關閉
    //            ForceDelay = true
    //        });
    //    }
    //}

    //private void Dispose(bool disposing)
    //{
    //    if (disposing)
    //    {
    //        DispatchService.UnSubscribe(Dispatch);
    //    }
    //}

    //public void Dispose()
    //{
    //    Dispose(true);
    //    GC.SuppressFinalize(this);
    //}
}