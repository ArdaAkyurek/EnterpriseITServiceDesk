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
    public class TicketPriorityRepository
    : GenericRepository<TicketPriority>,
      ITicketPriorityRepository
    {
        public TicketPriorityRepository(EnterpriseITDbContext context)
            : base(context)
        {
        }
    }
}
