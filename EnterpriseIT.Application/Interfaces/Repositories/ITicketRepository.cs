using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using EnterpriseIT.Domain.Entities;

namespace EnterpriseIT.Application.Interfaces.Repositories
{
    public interface ITicketRepository : IGenericRepository<Ticket>
    {
        Task<List<Ticket>> GetAllWithDetailsAsync(int? createdByUserId = null, int? assignedToUserId = null, bool unassignedOnly = false);

        Task<Ticket?> GetByIdWithDetailsAsync(int id, int? createdByUserId = null);
        Task<EnterpriseIT.Application.DTOs.Ticket.DashboardDto> GetDashboardAsync(int userId, string role);
        Task<bool> TryAssignAsync(int id, int userId, int? expectedAssigneeId, DateTime? expectedUpdatedAt);
        Task<bool> TryUpdateDetailsAsync(Ticket ticket, int? expectedAssigneeId, DateTime? expectedUpdatedAt);
        Task<bool> TryChangeStatusAsync(int id, int statusId, string expectedStatus, int? expectedAssigneeId, DateTime? expectedUpdatedAt, int? technicianUserId);
    }
}
