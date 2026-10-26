using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseIT.Application.DTOs.Ticket
{
    namespace EnterpriseIT.Application.DTOs.Ticket
    {
        public class CreateTicketDto
        {
            public string Title { get; set; } = string.Empty;

            public string Description { get; set; } = string.Empty;

            public int CategoryId { get; set; }

            public int PriorityId { get; set; }

            public int? AssetId { get; set; }
        }
    }
}
