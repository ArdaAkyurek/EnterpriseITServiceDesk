using EnterpriseIT.Application.Interfaces.Repositories;
using EnterpriseIT.Domain.Entities;
using EnterpriseIT.Persistence.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseIT.Persistence.Repositories
{
    public class TicketCategoryRepository : GenericRepository<TicketCategory>, ITicketCategoryRepository
    {
        public TicketCategoryRepository(EnterpriseITDbContext context) : base (context) { 
            }
    }
}
