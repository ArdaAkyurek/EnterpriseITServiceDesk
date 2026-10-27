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

    public async Task<List<TicketAttachment>> GetAllWithDetailsAsync(int ticketId)
    {
        return await _context.TicketAttachments
            .AsNoTracking()
            .Where(x => x.TicketId == ticketId)
            .Include(x => x.Ticket)
            .Include(x => x.UploadedByUser)
            .OrderByDescending(x => x.CreatedAt).ThenByDescending(x => x.AttachmentId)
            .ToListAsync();
    }

    public async Task<TicketAttachment?> GetByIdWithDetailsAsync(int id, int? createdByUserId = null)
    {
        return await _context.TicketAttachments
            .AsNoTracking()
            .Where(x => !createdByUserId.HasValue || x.Ticket.CreatedByUserId == createdByUserId.Value)
            .Include(x => x.Ticket)
            .Include(x => x.UploadedByUser)
            .FirstOrDefaultAsync(x => x.AttachmentId == id);
    }

    public async Task<bool> TryAddToOpenTicketAsync(TicketAttachment attachment, int? createdByUserId = null)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();
        // Serialize with closure, assignment and other attachment uploads on the parent ticket.
        var ticket = await _context.Tickets
            .FromSqlInterpolated($"SELECT * FROM \"Tickets\" WHERE \"TicketId\" = {attachment.TicketId} FOR UPDATE")
            .AsNoTracking().SingleOrDefaultAsync();
        if (ticket == null || createdByUserId.HasValue && ticket.CreatedByUserId != createdByUserId.Value ||
            ticket.ClosedAt != null || !await _context.TicketStatuses.AnyAsync(s => s.TicketStatusId == ticket.StatusId && s.StatusName != "Closed"))
            return false;

        attachment.CreatedAt = DateTime.UtcNow;
        _context.TicketAttachments.Add(attachment);
        await _context.SaveChangesAsync();
        await transaction.CommitAsync();
        return true;
    }
}
