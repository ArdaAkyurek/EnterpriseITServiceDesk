namespace EnterpriseIT.Application.DTOs.AuditLog;

public record AuditLogPageDto(List<AuditLogDto> Items, int Page, bool HasMore)
{
    public const int PageSize = 50;
}
