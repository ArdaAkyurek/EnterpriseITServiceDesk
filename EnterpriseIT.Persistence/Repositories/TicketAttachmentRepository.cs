using EnterpriseIT.Application.Interfaces.Repositories;
using EnterpriseIT.Domain.Entities;
using EnterpriseIT.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseIT.Persistence.Repositories;

public class TicketAttachmentRepository
    : GenericRepository<TicketAttachment>, ITicketAttachmentRepository
{
    public TicketAttachmentRepository(EnterpriseITDbContext context)
        : base(context)
    {
    }

    public async Task<List<TicketAttachment>> GetAllWithDetailsAsync()
    {
        return await _context.TicketAttachments
            .Include(x => x.Ticket)
            .Include(x => x.UploadedByUser)
            .ToListAsync();
    }

    public async Task<TicketAttachment?> GetByIdWithDetailsAsync(int id)
    {
        return await _context.TicketAttachments
            .Include(x => x.Ticket)
            .Include(x => x.UploadedByUser)
            .FirstOrDefaultAsync(x => x.AttachmentId == id);
    }
}