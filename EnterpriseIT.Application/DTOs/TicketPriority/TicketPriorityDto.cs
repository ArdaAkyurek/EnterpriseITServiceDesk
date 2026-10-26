using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseIT.Application.DTOs.TicketPriority
{
    public class TicketPriorityDto
    {
        public int TicketPriorityId { get; set; }

        public string PriorityName { get; set; } = string.Empty;
    }
}
