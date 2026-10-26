

using EnterpriseIT.Application.Interfaces.Repositories;
using EnterpriseIT.Domain.Entities;
using EnterpriseIT.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseIT.Persistence.Repositories
{
    public class TicketRepository
        : GenericRepository<Ticket>, ITicketRepository
    {
        public TicketRepository(EnterpriseITDbContext context)
            : base(context)
        {
        }

        public async Task<List<Ticket>> GetAllWithDetailsAsync()
        {
            return await _context.Tickets
                .Include(t => t.CreatedByUser)
                .Include(t => t.AssignedToUser)
                .Include(t => t.Category)
                .Include(t => t.Priority)
                .Include(t => t.Status)
                .Include(t => t.Asset)
                .ToListAsync();
        }
        public async Task<Ticket?> GetByIdWithDetailsAsync(int id)
        {
            return await _context.Tickets
                .Include(t => t.CreatedByUser)
                .Include(t => t.AssignedToUser)
                .Include(t => t.Category)
                .Include(t => t.Priority)
                .Include(t => t.Status)
                .Include(t => t.Asset)
                .FirstOrDefaultAsync(t => t.TicketId == id);
        }
    }
}