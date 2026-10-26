using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using EnterpriseIT.Domain.Entities;

namespace EnterpriseIT.Application.Interfaces.Repositories
{
    public interface IAssetRepository : IGenericRepository<Asset>
    {
        Task<Asset?> GetByAssetCodeAsync(string assetCode);
    }
}
