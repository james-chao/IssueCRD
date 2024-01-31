namespace BootstrapBlazorCRD.Shared.Service;

public interface IAuthService
{
    Task<bool> LoginAsync(string strEmployeeID);

    Task LogoutAsync();
}
