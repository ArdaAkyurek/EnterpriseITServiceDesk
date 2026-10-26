using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseIT.Application.DTOs.AssetAssignmentDto
{
    public class AssetAssignmentDto
    {
        public int AssetAssignmentId { get; set; }

        public string Asset { get; set; } = string.Empty;

        public string User { get; set; } = string.Empty;

        public DateTime AssignedDate { get; set; }

        public DateTime? ReturnedDate { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
