using EnterpriseIT.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseIT.Domain.Entities
{
    public class TicketCategory : BaseEntity
    {
        public int TicketCategoryId { get; set; }

        public string TicketCategoryName { get; set; } = string.Empty;

        public string? Description { get; set; }

        // Navigation Property
        public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
    }
}