using EnterpriseIT.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseIT.Application.Interfaces.Repositories
{
    public interface IAuditLogRepository : IGenericRepository<AuditLog>
    {
        Task<List<AuditLog>> GetAllWithDetailsAsync(int page = 1);

        Task<AuditLog?> GetByIdWithDetailsAsync(int id);
    }
}
