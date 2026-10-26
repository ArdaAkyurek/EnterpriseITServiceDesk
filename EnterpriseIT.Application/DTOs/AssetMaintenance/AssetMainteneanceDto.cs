using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseIT.Application.DTOs.AssetMaintenance
{
    public class AssetMaintenanceDto
    {
        public int MaintenanceId { get; set; }

        public string Description { get; set; } = string.Empty;

        public decimal Cost { get; set; }

        public DateTime MaintenanceDate { get; set; }

        public string Asset { get; set; } = string.Empty;

        public string PerformedBy { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
    }
}
