namespace AuthService.DTOs;

public sealed record TokenPairResponse(string AccessToken, string RefreshToken, string TokenType,
    DateTime ExpiresAtUtc, string UserId, string Email, IList<string> Roles, IEnumerable<string> Permissions);
public sealed record UserCreatedResponse(string Id, string? Email, string Role);
public sealed record UserSummaryResponse(string Id, string Email, IList<string> Roles, bool Disabled);
public sealed record UserStatusResponse(string Id, string? Email, bool Enabled);
public sealed record MeResponse(string? Id, string? Email, IEnumerable<string> Roles, IEnumerable<string> Permissions);