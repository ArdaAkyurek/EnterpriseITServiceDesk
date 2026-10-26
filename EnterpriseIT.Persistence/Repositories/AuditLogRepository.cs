using EnterpriseIT.Application.Interfaces.Repositories;
using EnterpriseIT.Domain.Entities;
using EnterpriseIT.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseIT.Persistence.Repositories;

public class AuditLogRepository
    : GenericRepository<AuditLog>, IAuditLogRepository
{
    public AuditLogRepository(EnterpriseITDbContext context)
        : base(context)
    {
    }

    public async Task<List<AuditLog>> GetAllWithDetailsAsync()
    {
        return await _context.AuditLogs
            .Include(x => x.User)
            .ToListAsync();
    }

    public async Task<AuditLog?> GetByIdWithDetailsAsync(int id)
    {
        return await _context.AuditLogs
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.AuditLogId == id);
    }
}
