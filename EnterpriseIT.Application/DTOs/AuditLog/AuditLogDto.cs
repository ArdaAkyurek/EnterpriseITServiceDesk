using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseIT.Application.DTOs.AuditLog
{
    public class AuditLogDto
    {
        public int AuditLogId { get; set; }

        public string Action { get; set; } = string.Empty;

        public string EntityName { get; set; } = string.Empty;

        public int EntityId { get; set; }

        public string User { get; set; } = string.Empty;
        public int UserId { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
