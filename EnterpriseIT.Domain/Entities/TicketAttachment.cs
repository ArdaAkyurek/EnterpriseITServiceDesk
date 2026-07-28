using EnterpriseIT.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseIT.Domain.Entities
{
    public class TicketAttachment : BaseEntity
    {
        public int AttachmentId { get; set; }

        public string FileName { get; set; } = string.Empty;

        public string FilePath { get; set; } = string.Empty;

        // Foreign Key
        public int TicketId { get; set; }
        public int UploadedByUserId { get; set; }

        // Navigation Property
        public Ticket Ticket { get; set; } = null!;

        public User UploadedByUser { get; set; } = null!;

    }
}