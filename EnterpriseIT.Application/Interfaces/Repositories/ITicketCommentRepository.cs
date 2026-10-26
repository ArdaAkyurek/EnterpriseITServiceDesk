using EnterpriseIT.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseIT.Application.Interfaces.Repositories
{
    public interface ITicketCommentRepository
    : IGenericRepository<TicketComment>
    {
        Task<List<TicketComment>> GetAllWithDetailsAsync();

        Task<TicketComment?> GetByIdWithDetailsAsync(int id);
    }
}
