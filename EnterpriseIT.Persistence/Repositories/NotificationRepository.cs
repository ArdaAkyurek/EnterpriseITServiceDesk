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
    public class NotificationRepository
      : GenericRepository<Notification>, INotificationRepository
    {
        public NotificationRepository(EnterpriseITDbContext context)
            : base(context)
        {
        }

        public async Task<List<Notification>> GetAllWithDetailsAsync(int userId, int page = 1)
        {
            return await _context.Notifications
                .AsNoTracking()
                .Where(x => x.UserId == userId)
                .Include(x => x.User)
                .OrderByDescending(x => x.CreatedAt).ThenByDescending(x => x.NotificationId)
                .Skip((page - 1) * 20).Take(21)
                .ToListAsync();
        }

        public async Task<Notification?> GetByIdWithDetailsAsync(int id, int userId)
        {
            return await _context.Notifications
                .Include(x => x.User)
                .FirstOrDefaultAsync(x => x.NotificationId == id && x.UserId == userId);
        }

        public Task<int> GetUnreadCountAsync(int userId) =>
            _context.Notifications.CountAsync(x => x.UserId == userId && !x.IsRead);
    }
}
