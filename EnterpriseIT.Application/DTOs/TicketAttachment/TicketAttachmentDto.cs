using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseIT.Application.DTOs.TicketAttachment
{
    public class TicketAttachmentDto
    {
        public int AttachmentId { get; set; }

        public string FileName { get; set; } = string.Empty;

        public int TicketId { get; set; }

        public string TicketTitle { get; set; } = string.Empty;

        public string UploadedBy { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
    }
}
