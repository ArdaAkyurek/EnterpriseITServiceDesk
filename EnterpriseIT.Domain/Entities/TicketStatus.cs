using EnterpriseIT.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseIT.Domain.Entities
{
    public class TicketStatus : BaseEntity
    {
        public int TicketStatusId { get; set; }

        public string StatusName { get; set; } = string.Empty;

        public string StatusDescription { get; set; }

        // Navigation Property
        public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
    }
}
