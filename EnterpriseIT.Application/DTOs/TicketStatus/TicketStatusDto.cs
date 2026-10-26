using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseIT.Application.DTOs.TicketStatus;

public class TicketStatusDto
{
    public int TicketStatusId { get; set; }

    public string StatusName { get; set; } = string.Empty;
}