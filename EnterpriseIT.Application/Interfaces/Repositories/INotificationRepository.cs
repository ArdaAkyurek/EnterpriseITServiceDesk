using EnterpriseIT.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseIT.Application.Interfaces.Repositories
{
    public interface INotificationRepository
        : IGenericRepository<Notification>
    {
        Task<List<Notification>> GetAllWithDetailsAsync(int userId, int page = 1);

        Task<Notification?> GetByIdWithDetailsAsync(int id, int userId);

        Task<int> GetUnreadCountAsync(int userId);
    }
}
