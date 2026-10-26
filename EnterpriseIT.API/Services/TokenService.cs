using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using EnterpriseIT.Application.DTOs.Auth;
using EnterpriseIT.Domain.Entities;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;

namespace EnterpriseIT.API.Services;

// Sessions belong to this API process. Restarting it invalidates all issued tokens.
public sealed class TokenService
{
    private readonly IMemoryCache _revokedTokens;
    private readonly string _instanceId = Guid.NewGuid().ToString("N");
    private readonly string _issuer;
    private readonly string _audience;
    public SymmetricSecurityKey SigningKey { get; }

    public TokenService(IConfiguration configuration, IWebHostEnvironment environment, IMemoryCache revokedTokens)
    {
        _revokedTokens = revokedTokens;
        _issuer = configuration["Jwt:Issuer"] ?? "EnterpriseITServiceDesk";
        _audience = configuration["Jwt:Audience"] ?? "EnterpriseITClient";
        var key = configuration["Jwt:SigningKey"];
        if (string.IsNullOrWhiteSpace(key) && !environment.IsDevelopment())
            throw new InvalidOperationException("Jwt:SigningKey must be configured outside Development.");
        var bytes = string.IsNullOrWhiteSpace(key) ? RandomNumberGenerator.GetBytes(64) : Encoding.UTF8.GetBytes(key);
        if (bytes.Length < 32) throw new InvalidOperationException("Jwt:SigningKey must contain at least 32 bytes.");
        SigningKey = new SymmetricSecurityKey(bytes);
    }

    public TokenValidationParameters ValidationParameters => new()
    {
        ValidateIssuer = true, ValidIssuer = _issuer,
        ValidateAudience = true, ValidAudience = _audience,
        ValidateIssuerSigningKey = true, IssuerSigningKey = SigningKey,
        ValidateLifetime = true, RequireExpirationTime = true,
        ValidAlgorithms = [SecurityAlgorithms.HmacSha256],
        ClockSkew = TimeSpan.Zero,
        NameClaimType = ClaimTypes.Name, RoleClaimType = ClaimTypes.Role
    };

    public LoginResponseDto Issue(User user)
    {
        var expiresAt = DateTime.UtcNow.AddMinutes(30);
        var token = new JwtSecurityToken(_issuer, _audience,
        [
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
            new Claim(ClaimTypes.Role, user.Role.RoleName),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
            new Claim("instance", _instanceId),
            new Claim("credential", CredentialStamp(user.PasswordHash))
        ], notBefore: DateTime.UtcNow, expires: expiresAt,
        signingCredentials: new SigningCredentials(SigningKey, SecurityAlgorithms.HmacSha256));
        return new(new JwtSecurityTokenHandler().WriteToken(token), expiresAt, ToDto(user));
    }

    public bool IsCurrent(ClaimsPrincipal principal, User user) =>
        user.IsActive && user.Role != null &&
        principal.FindFirstValue("instance") == _instanceId &&
        principal.FindFirstValue("credential") == CredentialStamp(user.PasswordHash) &&
        principal.FindFirstValue(ClaimTypes.Role) == user.Role.RoleName &&
        principal.FindFirstValue(JwtRegisteredClaimNames.Jti) is { } id &&
        !_revokedTokens.TryGetValue($"revoked:{id}", out _);

    public void Revoke(ClaimsPrincipal principal)
    {
        var id = principal.FindFirstValue(JwtRegisteredClaimNames.Jti)!;
        var expiresAt = DateTimeOffset.FromUnixTimeSeconds(long.Parse(principal.FindFirstValue("exp")!));
        if (expiresAt > DateTimeOffset.UtcNow)
            _revokedTokens.Set($"revoked:{id}", true, expiresAt);
    }

    public static AuthUserDto ToDto(User user) =>
        new(user.UserId, user.FirstName, user.LastName, user.Email, user.Role.RoleName);

    private static string CredentialStamp(string hash) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(hash)));
}
