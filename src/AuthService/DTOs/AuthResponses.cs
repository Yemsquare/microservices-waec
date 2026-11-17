namespace AuthService.DTOs;
public sealed record UserSummary(string Id,string Email,IList<string> Roles,bool Disabled);
public sealed record TokenPairResponse(string AccessToken,string RefreshToken,string TokenType,DateTime ExpiresAtUtc,string UserId,string Email,IList<string> Roles,IEnumerable<string> Permissions);