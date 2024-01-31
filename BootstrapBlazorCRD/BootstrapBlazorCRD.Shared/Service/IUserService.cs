using BootstrapBlazorCRD.Shared.Model;

namespace BootstrapBlazorCRD.Shared.Service;
public interface IUserService
{
    public Task<List<UserInfo>> GetUserAsync(string strEmployeeID);

    public Task<bool> IsAuthorized(string strEmployeeID);

    public Task<bool> SetUserStorageAsync(string storagename,UserInfo userInfo);

    public Task<List<UserInfo>> GetUserListAsync();

    public Task<bool> AddUserAsync(UserInfo userInfo);

    public Task<bool> UpdateUserAsync(UserInfo userInfo);

    public Task<bool> DeleteUserAsync(UserInfo userInfo);

    public Task<string> GetEmployeeID(string strMailAddress);
}
