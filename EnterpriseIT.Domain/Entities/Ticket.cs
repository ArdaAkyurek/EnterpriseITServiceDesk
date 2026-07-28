using EnterpriseIT.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseIT.Domain.Entities
{
    public class Ticket : BaseEntity
    {
        public int TicketId { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public DateTime? ClosedAt { get; set; }

        // Foreign Keys
        public int CreatedByUserId { get; set; }

        public int AssignedToUserId { get; set; }

        public int CategoryId { get; set; }

        public int PriorityId { get; set; }

        public int StatusId { get; set; }

        public int AssetId { get; set; }

        // Navigation Properties
        public User CreatedByUser { get; set; } = null!;

        public User AssignedToUser { get; set; } = null!;

        public TicketCategory Category { get; set; } = null!;

        public TicketPriority Priority { get; set; } = null!;

        public TicketStatus Status { get; set; } = null!;

        public Asset Asset { get; set; } = null!;

        public ICollection<TicketComment> Comments { get; set; } = new List<TicketComment>();

        public ICollection<TicketAttachment> Attachments { get; set; } = new List<TicketAttachment>();
    }
}