using System.Security.Claims;
using EnterpriseIT.API.Services;
using EnterpriseIT.Application.DTOs.Auth;
using EnterpriseIT.Application.Interfaces.Repositories;
using EnterpriseIT.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace EnterpriseIT.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public class AuthController(IUserRepository users, IPasswordHasher<User> hasher, TokenService tokens) : ControllerBase
{
    private static readonly User DummyUser = new();
    private static readonly string DummyHash = new PasswordHasher<User>().HashPassword(DummyUser, Guid.NewGuid().ToString());

    [AllowAnonymous]
    [HttpPost("login")]
    [EnableRateLimiting("login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        var user = await users.GetByEmailAsync(dto.Email);
        PasswordVerificationResult result;
        try
        {
            result = hasher.VerifyHashedPassword(user ?? DummyUser,
                string.IsNullOrEmpty(user?.PasswordHash) ? DummyHash : user.PasswordHash, dto.Password);
        }
        catch (FormatException)
        {
            result = PasswordVerificationResult.Failed;
        }
        if (user == null || !user.IsActive || string.IsNullOrEmpty(user.PasswordHash) ||
            user.Role == null || user.Role.RoleName is not ("Admin" or "Technician" or "Employee") ||
            result == PasswordVerificationResult.Failed)
            return Unauthorized(new { message = "E-posta veya parola hatalı." });

        return Ok(tokens.Issue(user));
    }

    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        var user = await users.GetByIdAsync(int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!));
        // Authentication validation has already loaded and checked the role in this request's DbContext.
        return user == null ? Unauthorized() : Ok(TokenService.ToDto(user));
    }

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        tokens.Revoke(User);
        return NoContent();
    }

    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        var user = await users.GetProfileAsync(int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!));
        return user == null || !user.IsActive ? Unauthorized() : Ok(ToProfile(user));
    }

    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile(UpdateProfileDto dto)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var phone = string.IsNullOrWhiteSpace(dto.Phone) ? null : dto.Phone.Trim();
        if (!await users.TryUpdatePhoneAsync(userId, phone)) return Unauthorized();
        var user = await users.GetProfileAsync(userId);
        return user == null || !user.IsActive ? Unauthorized() : Ok(ToProfile(user));
    }

    [HttpPut("password")]
    [EnableRateLimiting("login")]
    public async Task<IActionResult> ChangePassword(ChangePasswordDto dto)
    {
        var user = await users.GetByIdAsync(int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!));
        if (user == null || !user.IsActive) return Unauthorized();
        PasswordVerificationResult result;
        try { result = hasher.VerifyHashedPassword(user, user.PasswordHash, dto.CurrentPassword); }
        catch (FormatException) { result = PasswordVerificationResult.Failed; }
        if (result == PasswordVerificationResult.Failed)
            return BadRequest(new { message = "Mevcut parolanız hatalı." });
        if (dto.CurrentPassword == dto.NewPassword)
            return BadRequest(new { message = "Yeni parola mevcut parolanızdan farklı olmalıdır." });

        var newHash = hasher.HashPassword(user, dto.NewPassword);
        if (!await users.TryChangePasswordAsync(user.UserId, user.PasswordHash, newHash))
            return Conflict(new { message = "Hesap bilgileri değişti. Sayfayı yenileyip tekrar deneyin." });
        tokens.Revoke(User);
        return NoContent();
    }

    private static ProfileDto ToProfile(User user) => new(user.UserId, user.FirstName, user.LastName,
        user.Email, user.Role.RoleName, user.Department.DepartmentName, user.Phone, user.IsActive, user.CreatedAt);
}
