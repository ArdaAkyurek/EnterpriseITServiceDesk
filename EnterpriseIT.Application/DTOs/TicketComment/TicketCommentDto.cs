using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseIT.Application.DTOs.TicketComment
{
    public class TicketCommentDto
    {
        public int CommentId { get; set; }

        public string Comment { get; set; } = string.Empty;

        public int TicketId { get; set; }

        public string User { get; set; } = string.Empty;

        public string TicketTitle { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
    }
}
