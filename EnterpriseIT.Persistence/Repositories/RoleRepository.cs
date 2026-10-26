using EnterpriseIT.Application.Interfaces.Repositories;
using EnterpriseIT.Domain.Entities;

using EnterpriseIT.Persistence.Context;

namespace EnterpriseIT.Persistence.Repositories
{
    public class RoleRepository
     : GenericRepository<Role>, IRoleRepository
    {
        public RoleRepository(EnterpriseITDbContext context)
            : base(context)
        {
        }
    }
}
