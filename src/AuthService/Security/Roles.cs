using System.Security.Cryptography;
using System.Text;

namespace AuthService.Security;

public static class Roles
{
    public const string Candidate = "Candidate";
    public const string Examiner = "Examiner";
    public const string Administrator = "Administrator";
    public static readonly string[] All = [Candidate, Examiner, Administrator];
    public static readonly string[] Staff = [Examiner, Administrator];
}

