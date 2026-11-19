using System.Security.Cryptography;
using AuthService.Security;


public static class Permissions
{
    public const string ClaimType = "permission";
    public const string CatalogRead = "catalog.read";
    public const string CatalogManage = "catalog.manage";
    public const string RegistrationsRead = "registrations.read";
    public const string RegistrationsCreate = "registrations.create";
    public const string ProfileRead = "profile.read";
    public const string UsersManage = "users.manage";

    public static class Policies { public const string AdminOnly = "AdminOnly"; }

    public static readonly IReadOnlyDictionary<string, string[]> ByRole = new Dictionary<string, string[]>
    {
        [Roles.Candidate] = [CatalogRead, RegistrationsCreate, ProfileRead],
        [Roles.Examiner] = [CatalogRead, CatalogManage, RegistrationsRead, RegistrationsCreate, ProfileRead],
        [Roles.Administrator] = [CatalogRead, CatalogManage, RegistrationsRead, RegistrationsCreate, ProfileRead, UsersManage]
    };
}

