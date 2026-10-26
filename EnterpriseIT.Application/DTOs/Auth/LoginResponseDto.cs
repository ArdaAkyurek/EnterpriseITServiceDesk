namespace EnterpriseIT.Application.DTOs.Auth;

public record LoginResponseDto(string AccessToken, DateTime ExpiresAt, AuthUserDto User);
