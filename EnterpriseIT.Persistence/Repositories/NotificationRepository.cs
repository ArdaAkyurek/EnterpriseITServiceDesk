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

        public async Task<List<Notification>> GetAllWithDetailsAsync()
        {
            return await _context.Notifications
                .Include(x => x.User)
                .ToListAsync();
        }

        public async Task<Notification?> GetByIdWithDetailsAsync(int id)
        {
            return await _context.Notifications
                .Include(x => x.User)
                .FirstOrDefaultAsync(x => x.NotificationId == id);
        }
    }
}
