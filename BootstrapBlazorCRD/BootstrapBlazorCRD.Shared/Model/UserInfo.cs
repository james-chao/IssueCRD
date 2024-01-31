using BootstrapBlazor.Components;
using System.ComponentModel.DataAnnotations;

namespace BootstrapBlazorCRD.Shared.Model;

public class UserInfo
{
    [AutoGenerateColumn(Order = 10, Readonly = true, Editable = false, Sortable = true, Filterable = true, Align = Alignment.Center)]
    [Display(Name = "User Name")]
    public string UserName { get; set; } = string.Empty;

    [AutoGenerateColumn(Order = 20, Sortable = true, Filterable = true, Searchable = true, Align = Alignment.Center)]
    [Display(Name = "Mail Address")]
    public string MailAddress { get; set; } = string.Empty;

    [AutoGenerateColumn(Order = 30, Filterable = true, Align = Alignment.Center)]
    [Display(Name = "Roles")]
    public EnumRoles? Roles { get; set; } 

    [AutoGenerateColumn(Order = 40, Sortable = true, Filterable = true, FormatString = "yyyy-MM-dd", Align = Alignment.Center)]
    [Display(Name = "Create Date")]
    public DateTime CreateDate { get; set; }

    [AutoGenerateColumn(Order = 50, Readonly = true, Editable = false, Sortable = true, Filterable = true, Align = Alignment.Center)]
    [Display(Name = "Create Name")]
    public string CreateName { get; set; } = string.Empty;

    public enum EnumRoles
    {
        [Display(Name = "ADMIN")]
        ADMIN,
        [Display(Name = "CE")]
        CE,
        [Display(Name = "DM")]
        DM,
        [Display(Name = "MFG")]
        MFG,
        [Display(Name = "PJE")]
        PJE,
        [Display(Name = "RD")]
        RD,
        [Display(Name = "SALES")]
        SALES,
        [Display(Name = "TE")]
        TE,
        [Display(Name = "USERS")]
        USERS,
        [Display(Name = "WEBAPI")]
        WEBAPI
    }
}
