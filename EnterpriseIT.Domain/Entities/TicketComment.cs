using EnterpriseIT.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseIT.Domain.Entities
{
    public class TicketComment : BaseEntity
    {
        
        public int CommentId { get; set; }

        public string Comment { get; set; } = string.Empty;

        // Foreign Keys
        public int TicketId { get; set; }

        public int UserId { get; set; }

        // Navigation Properties
        public Ticket Ticket { get; set; } = null!;

        public User User { get; set; } = null!;
    }
}