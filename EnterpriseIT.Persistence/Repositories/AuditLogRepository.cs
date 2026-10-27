using EnterpriseIT.Application.Interfaces.Repositories;
using EnterpriseIT.Domain.Entities;
using EnterpriseIT.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using EnterpriseIT.Application.DTOs.AuditLog;

namespace EnterpriseIT.Persistence.Repositories;

public class AuditLogRepository
    : GenericRepository<AuditLog>, IAuditLogRepository
{
    public AuditLogRepository(EnterpriseITDbContext context)
        : base(context)
    {
    }

    public async Task<List<AuditLog>> GetAllWithDetailsAsync(int page = 1)
    {
        return await _context.AuditLogs
            .AsNoTracking()
            .Include(x => x.User)
            .OrderByDescending(x => x.CreatedAt).ThenByDescending(x => x.AuditLogId)
            .Skip((page - 1) * AuditLogPageDto.PageSize).Take(AuditLogPageDto.PageSize + 1)
            .ToListAsync();
    }

    public async Task<AuditLog?> GetByIdWithDetailsAsync(int id)
    {
        return await _context.AuditLogs
            .AsNoTracking()
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.AuditLogId == id);
    }
}
