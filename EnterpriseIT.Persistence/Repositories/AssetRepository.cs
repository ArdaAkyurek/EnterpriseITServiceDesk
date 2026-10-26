using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using EnterpriseIT.Application.Interfaces.Repositories;
using EnterpriseIT.Domain.Entities;
using EnterpriseIT.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseIT.Persistence.Repositories
{
    public class AssetRepository
        : GenericRepository<Asset>, IAssetRepository
    {
        public AssetRepository(EnterpriseITDbContext context)
            : base(context)
        {
        }

        public async Task<Asset?> GetByAssetCodeAsync(string assetCode)
        {
            return await _context.Set<Asset>()
                .FirstOrDefaultAsync(x => x.AssetCode == assetCode);
        }
    }
}