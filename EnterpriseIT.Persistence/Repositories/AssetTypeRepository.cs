using EnterpriseIT.Application.Interfaces.Repositories;
using EnterpriseIT.Domain.Entities;
using EnterpriseIT.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseIT.Persistence.Repositories
{
    public class AssetTypeRepository
        : GenericRepository<AssetType>, IAssetTypeRepository
    {
        public AssetTypeRepository(EnterpriseITDbContext context)
            : base(context)
        {
        }

        public async Task<AssetType?> GetByNameAsync(string name)
        {
            return await _context.Set<AssetType>()
                .FirstOrDefaultAsync(x => x.Name == name);
        }
    }
}