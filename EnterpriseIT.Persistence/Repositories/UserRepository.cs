using EnterpriseIT.Application.Interfaces.Repositories;
using EnterpriseIT.Domain.Entities;
using EnterpriseIT.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseIT.Persistence.Repositories
{
    public class UserRepository
     : GenericRepository<User>, IUserRepository
    {
        public UserRepository(EnterpriseITDbContext context)
            : base(context)
        {
          
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            email = email.Trim().ToLowerInvariant();
            return await _context.Users
                .Include(x => x.Role)
                .FirstOrDefaultAsync(x => x.Email.ToLower() == email);
        }

        public Task<List<User>> GetActiveTechniciansAsync() => _context.Users
            .Where(u => u.IsActive && u.Role.RoleName == "Technician")
            .OrderBy(u => u.FirstName).ThenBy(u => u.LastName)
            .ToListAsync();

        public Task<User?> GetProfileAsync(int userId) => _context.Users.AsNoTracking()
            .Include(u => u.Role).Include(u => u.Department)
            .FirstOrDefaultAsync(u => u.UserId == userId);

        public async Task<bool> TryUpdatePhoneAsync(int userId, string? phone)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();
            var changed = await _context.Users.Where(u => u.UserId == userId && u.IsActive)
                .ExecuteUpdateAsync(setters => setters.SetProperty(u => u.Phone, phone)
                    .SetProperty(u => u.UpdatedAt, DateTime.UtcNow));
            if (changed != 1) return false;
            await _context.RecordAuditAsync("PhoneUpdated", nameof(User), userId);
            await transaction.CommitAsync();
            return true;
        }

        public async Task<bool> TryChangePasswordAsync(int userId, string expectedHash, string newHash)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();
            var changed = await _context.Users.Where(u => u.UserId == userId && u.IsActive && u.PasswordHash == expectedHash)
                .ExecuteUpdateAsync(setters => setters.SetProperty(u => u.PasswordHash, newHash)
                    .SetProperty(u => u.UpdatedAt, DateTime.UtcNow));
            if (changed != 1) return false;
            await _context.RecordAuditAsync("PasswordChanged", nameof(User), userId);
            await transaction.CommitAsync();
            return true;
        }
    }
}
