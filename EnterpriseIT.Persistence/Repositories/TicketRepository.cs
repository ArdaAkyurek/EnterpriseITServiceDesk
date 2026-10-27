

using EnterpriseIT.Application.Interfaces.Repositories;
using EnterpriseIT.Domain.Entities;
using EnterpriseIT.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using EnterpriseIT.Application.DTOs.Ticket;
using System.Data;

namespace EnterpriseIT.Persistence.Repositories
{
    public class TicketRepository
        : GenericRepository<Ticket>, ITicketRepository
    {
        public TicketRepository(EnterpriseITDbContext context)
            : base(context)
        {
        }

        public async Task<DashboardDto> GetDashboardAsync(int userId, string role)
        {
            if (role is not ("Admin" or "Technician" or "Employee")) throw new ArgumentOutOfRangeException(nameof(role));
            // All dashboard sections describe the same read snapshot without producing audit writes.
            await using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.RepeatableRead);
            var query = _context.Tickets.AsNoTracking();
            if (role == "Employee") query = query.Where(t => t.CreatedByUserId == userId);
            else if (role == "Technician") query = query.Where(t => t.AssignedToUserId == userId);

            var result = await query.GroupBy(t => 1).Select(g => new DashboardDto
            {
                Total = g.Count(),
                Open = g.Count(t => t.ClosedAt == null && t.Status.StatusName == "Open"),
                InProgress = g.Count(t => t.ClosedAt == null && t.Status.StatusName == "In Progress"),
                Closed = g.Count(t => t.ClosedAt != null || t.Status.StatusName == "Closed"),
                Other = g.Count(t => t.ClosedAt == null && t.Status.StatusName != "Open" &&
                    t.Status.StatusName != "In Progress" && t.Status.StatusName != "Closed")
            }).SingleOrDefaultAsync() ?? new DashboardDto();
            result.Scope = role == "Admin" ? "all" : role == "Technician" ? "assigned" : "created";
            if (role != "Employee")
                result.UnassignedActiveCount = await _context.Tickets.CountAsync(t =>
                    t.AssignedToUserId == null && t.ClosedAt == null && t.Status.StatusName != "Closed");
            result.ActiveByPriority = await query.Where(t => t.ClosedAt == null && t.Status.StatusName != "Closed")
                .GroupBy(t => t.Priority.PriorityName)
                .Select(g => new DashboardPriorityDto { Priority = g.Key, Count = g.Count() })
                .OrderByDescending(p => p.Count).ThenBy(p => p.Priority).ToListAsync();
            result.RecentTickets = await query.OrderByDescending(t => t.CreatedAt).ThenByDescending(t => t.TicketId)
                .Take(5).Select(t => new DashboardTicketDto
                {
                    TicketId = t.TicketId, Title = t.Title,
                    Status = t.ClosedAt != null ? "Closed" : t.Status.StatusName,
                    Priority = t.Priority.PriorityName, CreatedAt = t.CreatedAt,
                    AssignedTo = t.AssignedToUser == null ? null : t.AssignedToUser.FirstName + " " + t.AssignedToUser.LastName
                }).ToListAsync();
            result.GeneratedAt = DateTime.UtcNow;
            await transaction.CommitAsync();
            return result;
        }

        public async Task<List<Ticket>> GetAllWithDetailsAsync(int? createdByUserId = null, int? assignedToUserId = null, bool unassignedOnly = false)
        {
            return await _context.Tickets
                .AsNoTracking()
                .Where(t => !createdByUserId.HasValue || t.CreatedByUserId == createdByUserId.Value)
                .Where(t => !assignedToUserId.HasValue || t.AssignedToUserId == assignedToUserId.Value)
                .Where(t => !unassignedOnly || t.AssignedToUserId == null)
                .Include(t => t.CreatedByUser)
                .Include(t => t.AssignedToUser)
                .Include(t => t.Category)
                .Include(t => t.Priority)
                .Include(t => t.Status)
                .Include(t => t.Asset)
                .OrderByDescending(t => t.CreatedAt)
                .ThenByDescending(t => t.TicketId)
                .ToListAsync();
        }
        public async Task<Ticket?> GetByIdWithDetailsAsync(int id, int? createdByUserId = null)
        {
            return await _context.Tickets
                .AsNoTracking()
                .Where(t => !createdByUserId.HasValue || t.CreatedByUserId == createdByUserId.Value)
                .Include(t => t.CreatedByUser)
                .Include(t => t.AssignedToUser)
                .Include(t => t.Category)
                .Include(t => t.Priority)
                .Include(t => t.Status)
                .Include(t => t.Asset)
                .FirstOrDefaultAsync(t => t.TicketId == id);
        }

        public async Task<bool> TryAssignAsync(int id, int userId, int? expectedAssigneeId, DateTime? expectedUpdatedAt)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();
            var changed = await _context.Tickets
                .Where(t => t.TicketId == id && t.AssignedToUserId == expectedAssigneeId &&
                    t.UpdatedAt == expectedUpdatedAt && t.ClosedAt == null && t.Status.StatusName != "Closed" &&
                    _context.Users.Any(u => u.UserId == userId && u.IsActive && u.Role.RoleName == "Technician"))
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(t => t.AssignedToUserId, userId)
                    .SetProperty(t => t.UpdatedAt, DateTime.UtcNow));
            if (changed != 1) return false;
            await _context.RecordAuditAsync(_context.AuditActorId == userId ? "Claim" : "Assign", nameof(Ticket), id);
            if (userId != expectedAssigneeId)
                await NotifyAsync(userId, "Ticket atandı", $"#{id} numaralı ticket size atandı.");
            await transaction.CommitAsync();
            return true;
        }

        public async Task<bool> TryUpdateDetailsAsync(Ticket ticket, int? expectedAssigneeId, DateTime? expectedUpdatedAt)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();
            var changed = await _context.Tickets
                .Where(t => t.TicketId == ticket.TicketId && t.AssignedToUserId == expectedAssigneeId && t.UpdatedAt == expectedUpdatedAt)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(t => t.Title, ticket.Title)
                    .SetProperty(t => t.Description, ticket.Description)
                    .SetProperty(t => t.CategoryId, ticket.CategoryId)
                    .SetProperty(t => t.PriorityId, ticket.PriorityId)
                    .SetProperty(t => t.StatusId, ticket.StatusId)
                    .SetProperty(t => t.AssetId, ticket.AssetId)
                    .SetProperty(t => t.ClosedAt, ticket.ClosedAt)
                    .SetProperty(t => t.UpdatedAt, DateTime.UtcNow));
            if (changed != 1) return false;
            await _context.RecordAuditAsync("Update", nameof(Ticket), ticket.TicketId);
            await transaction.CommitAsync();
            return true;
        }

        public async Task<bool> TryChangeStatusAsync(int id, int statusId, string expectedStatus, int? expectedAssigneeId, DateTime? expectedUpdatedAt, int? technicianUserId)
        {
            if (expectedStatus is not ("Open" or "In Progress")) return false;
            await using var transaction = await _context.Database.BeginTransactionAsync();
            var closing = expectedStatus == "In Progress";
            var now = DateTime.UtcNow;
            var changed = await _context.Tickets
                .Where(t => t.TicketId == id && t.AssignedToUserId == expectedAssigneeId &&
                    t.UpdatedAt == expectedUpdatedAt && t.Status.StatusName == expectedStatus && t.ClosedAt == null &&
                    t.AssignedToUserId != null && (!technicianUserId.HasValue || t.AssignedToUserId == technicianUserId.Value) &&
                    (closing || _context.Users.Any(u => u.UserId == t.AssignedToUserId && u.IsActive && u.Role.RoleName == "Technician")))
                .ExecuteUpdateAsync(setters => setters.SetProperty(t => t.StatusId, statusId)
                    .SetProperty(t => t.ClosedAt, closing ? (DateTime?)now : null)
                    .SetProperty(t => t.UpdatedAt, now));
            if (changed != 1) return false;
            await _context.RecordAuditAsync(closing ? "Close" : "Start", nameof(Ticket), id);
            var ownerId = await _context.Tickets.Where(t => t.TicketId == id).Select(t => t.CreatedByUserId).SingleAsync();
            await NotifyAsync(ownerId, closing ? "Ticket kapatıldı" : "Ticket işleme alındı",
                $"#{id} numaralı talebiniz {(closing ? "kapatıldı" : "işleme alındı")}.");
            await transaction.CommitAsync();
            return true;
        }
        private async Task NotifyAsync(int recipientId, string title, string message)
        {
            if (recipientId == _context.AuditActorId || !await _context.Users.AnyAsync(u => u.UserId == recipientId && u.IsActive)) return;
            _context.Notifications.Add(new Notification
            {
                UserId = recipientId, Title = title, Message = message, CreatedAt = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();
        }
    }
}
