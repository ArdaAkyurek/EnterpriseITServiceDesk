using EnterpriseIT.Application.Interfaces.Repositories;
using EnterpriseIT.Domain.Entities;
using EnterpriseIT.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseIT.Persistence.Repositories
{
    public class TicketCommentRepository : GenericRepository<TicketComment>, ITicketCommentRepository
    {

        public TicketCommentRepository(EnterpriseITDbContext context)
             : base(context) { }
        
            public async Task<List<TicketComment>> GetAllWithDetailsAsync(int ticketId)
            {
             return await _context.TicketComments
                .AsNoTracking()
                .Where(x => x.TicketId == ticketId)
                .Include(x => x.User)
                .Include(x => x.Ticket)
                .OrderBy(x => x.CreatedAt).ThenBy(x => x.CommentId)
                .ToListAsync();
            }
        public async Task<TicketComment?> GetByIdWithDetailsAsync(int id, int? createdByUserId = null)
        {
            return await _context.TicketComments
                .AsNoTracking()
                .Where(x => !createdByUserId.HasValue || x.Ticket.CreatedByUserId == createdByUserId.Value)
                .Include(x => x.User)
                .Include(x => x.Ticket)
                .FirstOrDefaultAsync(x => x.CommentId == id);
        }

        public async Task<bool> TryAddToOpenTicketAsync(TicketComment comment, int? createdByUserId = null)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();
            // Durum değiştirme de aynı ticket satırını günceller; kapanış ile yorum ekleme sıralanır.
            var ticket = await _context.Tickets
                .FromSqlInterpolated($"SELECT * FROM \"Tickets\" WHERE \"TicketId\" = {comment.TicketId} FOR UPDATE")
                .AsNoTracking().SingleOrDefaultAsync();
            if (ticket == null || createdByUserId.HasValue && ticket.CreatedByUserId != createdByUserId.Value ||
                ticket.ClosedAt != null || !await _context.TicketStatuses.AnyAsync(s => s.TicketStatusId == ticket.StatusId && s.StatusName != "Closed"))
                return false;

            comment.CreatedAt = DateTime.UtcNow;
            await _context.TicketComments.AddAsync(comment);
            var recipientIds = new[] { ticket.CreatedByUserId, ticket.AssignedToUserId ?? 0 }
                .Where(id => id > 0 && id != comment.UserId).Distinct().ToArray();
            var activeRecipients = await _context.Users.Where(u => recipientIds.Contains(u.UserId) && u.IsActive)
                .Select(u => u.UserId).ToListAsync();
            foreach (var recipientId in activeRecipients)
                _context.Notifications.Add(new Notification
                {
                    UserId = recipientId, Title = "Ticket’a yeni yorum",
                    Message = $"#{ticket.TicketId} numaralı ticket’a yeni bir yorum eklendi.",
                    CreatedAt = comment.CreatedAt
                });
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            return true;
        }


    }
 }
    


