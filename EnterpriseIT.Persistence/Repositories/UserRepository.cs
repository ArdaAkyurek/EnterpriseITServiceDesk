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
    }
}
