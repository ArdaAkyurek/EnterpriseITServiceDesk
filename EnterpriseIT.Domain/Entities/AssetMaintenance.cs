using EnterpriseIT.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseIT.Domain.Entities
{
    public class AssetMaintenance : BaseEntity
    {
        public int MaintenanceId { get; set; }

        public string Description { get; set; } = string.Empty;

        public decimal Cost { get; set; }

        public DateTime MaintenanceDate { get; set; }

        // Foreign Keys
        public int AssetId { get; set; }

        public int PerformedByUserId { get; set; }

        // Navigation Properties
        public Asset Asset { get; set; } = null!;

        public User PerformedByUser { get; set; } = null!;
    }
}
