using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using EnterpriseIT.Domain.Entities;

namespace EnterpriseIT.Application.Interfaces.Repositories
{
    public interface IAssetTypeRepository : IGenericRepository<AssetType>
    {
        Task<AssetType?> GetByNameAsync(string name);
    }
}
