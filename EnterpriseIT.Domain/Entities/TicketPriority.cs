using EnterpriseIT.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseIT.Domain.Entities
{
    public class TicketPriority : BaseEntity
    {
        public int TicketPriorityId { get; set; }

        public string PriorityName { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        // Navigation Property
        public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
    }
}
