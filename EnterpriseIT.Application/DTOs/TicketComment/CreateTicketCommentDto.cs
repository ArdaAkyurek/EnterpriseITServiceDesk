using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseIT.Application.DTOs.TicketComment
{
    public class CreateTicketCommentDto
    {
        public string Comment { get; set; } = string.Empty;

        public int TicketId { get; set; }
    }
}
