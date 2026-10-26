using EnterpriseIT.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseIT.Application.Interfaces.Repositories
{
    public interface IAssetMaintenanceRepository
    : IGenericRepository<AssetMaintenance>
    {
        Task<List<AssetMaintenance>> GetAllWithDetailsAsync();

        Task<AssetMaintenance?> GetByIdWithDetailsAsync(int id);
    }
}
