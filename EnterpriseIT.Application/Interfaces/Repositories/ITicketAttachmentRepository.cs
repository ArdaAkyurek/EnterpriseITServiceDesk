using EnterpriseIT.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseIT.Application.Interfaces.Repositories
{
    public interface ITicketAttachmentRepository
    : IGenericRepository<TicketAttachment>
    {
        Task<List<TicketAttachment>> GetAllWithDetailsAsync();

        Task<TicketAttachment?> GetByIdWithDetailsAsync(int id);
    }
}
