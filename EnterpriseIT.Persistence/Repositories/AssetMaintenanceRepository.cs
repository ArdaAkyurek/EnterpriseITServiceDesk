using EnterpriseIT.Application.Interfaces.Repositories;
using EnterpriseIT.Domain.Entities;
using EnterpriseIT.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseIT.Persistence.Repositories
{
    public class AssetMaintenanceRepository
    : GenericRepository<AssetMaintenance>, IAssetMaintenanceRepository
    {
        public AssetMaintenanceRepository(EnterpriseITDbContext context)
            : base(context)
        {
        }

        public async Task<List<AssetMaintenance>> GetAllWithDetailsAsync()
        {
            return await _context.AssetMaintenances
                .Include(x => x.Asset)
                .Include(x => x.PerformedByUser)
                .ToListAsync();
        }

        public async Task<AssetMaintenance?> GetByIdWithDetailsAsync(int id)
        {
            return await _context.AssetMaintenances
                .Include(x => x.Asset)
                .Include(x => x.PerformedByUser)
                .FirstOrDefaultAsync(x => x.MaintenanceId == id);
        }
    }
}
