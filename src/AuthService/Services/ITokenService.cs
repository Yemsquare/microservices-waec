using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using AuthService.DTOs;
using AuthService.Entities;
using AuthService.Options;
using AuthService.Repositories;
using AuthService.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using AuthService.Services;

namespace AuthService.Services;
public interface ITokenService
{
    /// Issues an access token + refresh token and persists the refresh token hash.
    Task<TokenPairResponse> IssueTokenPairAsync(IdentityUser user, CancellationToken ct = default);
}

