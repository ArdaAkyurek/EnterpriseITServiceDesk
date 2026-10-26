using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseIT.Application.DTOs.AssetAssignment
{
    public class CreateAssetAssignmentDto
    {
        public int AssetId { get; set; }

        public int UserId { get; set; }

        public DateTime AssignedDate { get; set; }
    }
}
