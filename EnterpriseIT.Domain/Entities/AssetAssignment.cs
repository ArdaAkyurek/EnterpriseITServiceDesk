using EnterpriseIT.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseIT.Domain.Entities
{
    public class AssetAssignment : BaseEntity
    {
        public int AssetAssignmentId { get; set; }

        public DateTime AssignedDate { get; set; }

        public DateTime? ReturnedDate { get; set; }

        // Foreign Keys
        public int AssetId { get; set; }

        public int UserId { get; set; }

        // Navigation Properties
        public Asset Asset { get; set; } = null!;

        public User User { get; set; } = null!;
    }
}