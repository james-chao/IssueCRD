using Blazored.LocalStorage;
using BootstrapBlazorCRD.Shared.Model;
using Microsoft.Extensions.Configuration;
using System.DirectoryServices;
using DirectoryEntry = System.DirectoryServices.DirectoryEntry;
using BootstrapBlazorCRD.Shared.Data;
using Microsoft.AspNetCore.DataProtection;

namespace BootstrapBlazorCRD.Shared.Service;

public class UserService : IUserService
{
    private readonly ILocalStorageService _localStorage;
    private readonly IConfiguration _configuration;
    private readonly string _LDAPServer;
    private readonly IDataProtectionProvider _dataProtectionProvider;
    public UserService(ILocalStorageService localStorageService, IConfiguration configuration, IDataProtectionProvider dataProtectionProvider)
    {
        _localStorage = localStorageService;
        _configuration = configuration;
        _dataProtectionProvider = dataProtectionProvider;
        _LDAPServer = _configuration.GetValue<string>("LDAPServer");
    }

    public async Task<List<UserInfo>> GetUserAsync(string? strEmployeeID)
    {
        using (var db = new CRDContext(_configuration, _dataProtectionProvider))
        {
            List<UserInfo> users = new List<UserInfo>();

            if (strEmployeeID == "ALL")
            {
                users = db.Users.OrderByDescending(u => u.CreateDate).ToList();
            }
            else
            {
                var singleuser = db.Users.FirstOrDefault(b => b.UserName == strEmployeeID);
                if (singleuser != null)
                    users.Add(singleuser);
            }

            return users;
        } 
    }

    public async Task<List<UserInfo>> GetUserListAsync()
    {
        List<UserInfo> user = new List<UserInfo>();
        user = await GetUserAsync("ALL");
        return user;
    }

    public async Task<bool> IsAuthorized(string? strEmployeeID)
    {
        var user = await GetUserAsync(strEmployeeID);

        if (user.Count > 0)
        { return true; }

        return false;
    }

    public async Task<bool> SetUserStorageAsync(string storagename, UserInfo userInfo)
    {
        await _localStorage.SetItemAsync(storagename, userInfo);
        return true;
    }

    public Task<string> GetEmployeeID(string strMailAddress)
    {
        string strEmployeeID = string.Empty;
        string strLDAP = "LDAP://" + _LDAPServer + "/" + "DC = quanta,DC = corp";
        DirectoryEntry ldapConnection = new DirectoryEntry();
        ldapConnection.Path = strLDAP;
        ldapConnection.AuthenticationType = AuthenticationTypes.Secure;

        DirectorySearcher search = new DirectorySearcher(ldapConnection);
        search.Filter = "(mail=" + strMailAddress + ")";
        search.PropertiesToLoad.Add("mailnickname");

        SearchResult result = search.FindOne();

        if (result != null)
        {
            foreach (Object myCollection in result.Properties["mailnickname"])
                strEmployeeID = myCollection.ToString();
        }

        return Task.FromResult(strEmployeeID);
    }

    public Task<bool> AddUserAsync(UserInfo userInfo)
    {
        bool ret = false;
        userInfo.UserName = GetEmployeeID(userInfo.MailAddress).Result;

        if (!IsAuthorized(userInfo.UserName).Result && userInfo.UserName !="")
        {
            ret = UserProcess(userInfo, "Add");
        }
        return Task.FromResult(ret);
    }

    public Task<bool> UpdateUserAsync(UserInfo userInfo)
    {
        bool ret = false;
        ret = UserProcess(userInfo, "Update");
        return Task.FromResult(ret);
    }

    public Task<bool> DeleteUserAsync(UserInfo userInfo)
    {
        bool ret = false;
        ret = UserProcess(userInfo, "Delete");
        return Task.FromResult(ret);
    }

    private bool UserProcess(UserInfo userInfo, string strType)
    {
        bool ret = false;
        using (var db = new CRDContext(_configuration, _dataProtectionProvider))
        {
            switch (strType)
            {
                case "Add":
                    db.Add(userInfo);
                    break;
                case "Update":
                    var userUpdate = db.Users.Single(b => b.UserName == userInfo.UserName);
                    userUpdate!.MailAddress = userInfo.MailAddress;
                    userUpdate.Roles = userInfo.Roles;
                    userUpdate.CreateDate = userInfo.CreateDate;
                    userUpdate.CreateName = userInfo.CreateName;
                    break;
                case "Delete":
                    var userDelete = db.Users.Single(b => b.UserName == userInfo.UserName);
                    db.Users.Remove(userDelete);
                    break;
            }
            if (db.SaveChanges() > 0)
            { ret = true; }
            else { ret = false; }
        }

        return ret;
    } 
}

