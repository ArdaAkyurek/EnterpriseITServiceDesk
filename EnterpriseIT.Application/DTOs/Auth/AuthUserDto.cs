namespace EnterpriseIT.Application.DTOs.Auth;

public record AuthUserDto(int UserId, string FirstName, string LastName, string Email, string Role);
