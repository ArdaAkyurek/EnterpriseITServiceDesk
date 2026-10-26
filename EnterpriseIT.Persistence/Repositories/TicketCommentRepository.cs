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
        
            public async Task<List<TicketComment>> GetAllWithDetailsAsync()
            {
             return await _context.TicketComments
                .Include(x => x.User)
                .Include(x => x.Ticket)
                .ToListAsync();
            }
        public async Task<TicketComment?> GetByIdWithDetailsAsync(int id)
        {
            return await _context.TicketComments
                .Include(x => x.User)
                .Include(x => x.Ticket)
                .FirstOrDefaultAsync(x => x.CommentId == id);
        }


    }
 }
    


