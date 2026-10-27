namespace EnterpriseIT.Application.DTOs.Auth;

public record ProfileDto(int UserId, string FirstName, string LastName, string Email,
    string Role, string Department, string? Phone, bool IsActive, DateTime CreatedAt);
