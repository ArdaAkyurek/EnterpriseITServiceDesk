using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using EnterpriseIT.Domain.Common;

namespace EnterpriseIT.Domain.Entities
{
    public class User : BaseEntity
    {
        public int UserId { get; set; }

        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string PasswordHash { get; set; } = string.Empty;

        public string? Phone { get; set; }

        public bool IsActive { get; set; } = true;

        // Foreign Keys
        public int DepartmentId { get; set; }

        public int RoleId { get; set; }

        // Navigation Properties
        public Department Department { get; set; } = null!;

        public Role Role { get; set; } = null!;

        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();

        public ICollection<Ticket> CreatedTickets { get; set; } = new List<Ticket>();

        public ICollection<Ticket> AssignedTickets { get; set; } = new List<Ticket>();

        public ICollection<TicketComment> TicketComments { get; set; } = new List<TicketComment>();
        public ICollection<AssetMaintenance> PerformedMaintenances { get; set; } = new List<AssetMaintenance>();

        public ICollection<AssetAssignment> AssetAssignments { get; set; } = new List<AssetAssignment>();

        public ICollection<TicketAttachment> UploadedAttachments { get; set; } = new List<TicketAttachment>();

        public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();

        
    }
}