using EnterpriseIT.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseIT.Domain.Entities
{
    public class AuditLog : BaseEntity
    {
        public int AuditLogId { get; set; }

        public string Action { get; set; } = string.Empty;

        public string EntityName { get; set; } = string.Empty;

        public int EntityId { get; set; }

        // Foreign Key
        public int UserId { get; set; }

        // Navigation Property
        public User User { get; set; } = null!;
    }
}