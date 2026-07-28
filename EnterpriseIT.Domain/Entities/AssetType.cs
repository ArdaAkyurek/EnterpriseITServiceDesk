using EnterpriseIT.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseIT.Domain.Entities
{
    public class AssetType : BaseEntity
    {
        public int AssetTypeId { get; set; }

        public string Name { get; set; } = string.Empty;

        // Navigation Property
        public ICollection<Asset> Assets { get; set; } = new List<Asset>();
    }
}