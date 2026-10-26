using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseIT.Application.DTOs.AssetMaintenance
{
    public class CreateAssetMaintenanceDto
    {
        public string Description { get; set; } = string.Empty;

        public decimal Cost { get; set; }

        public DateTime MaintenanceDate { get; set; }

        public int AssetId { get; set; }

        public int PerformedByUserId { get; set; }
    }
}
