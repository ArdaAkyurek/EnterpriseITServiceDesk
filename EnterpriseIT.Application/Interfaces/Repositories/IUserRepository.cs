using EnterpriseIT.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseIT.Application.Interfaces.Repositories
{
    public interface IUserRepository : IGenericRepository<User>
    {
        Task<User?> GetByEmailAsync(string email);
        Task<List<User>> GetActiveTechniciansAsync();
        Task<User?> GetProfileAsync(int userId);
        Task<bool> TryUpdatePhoneAsync(int userId, string? phone);
        Task<bool> TryChangePasswordAsync(int userId, string expectedHash, string newHash);
    }
}
