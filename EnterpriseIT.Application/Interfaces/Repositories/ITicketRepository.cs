using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using EnterpriseIT.Domain.Entities;

namespace EnterpriseIT.Application.Interfaces.Repositories
{
    public interface ITicketRepository : IGenericRepository<Ticket>
    {
        Task<List<Ticket>> GetAllWithDetailsAsync();

        Task<Ticket?> GetByIdWithDetailsAsync(int id);
    }
}