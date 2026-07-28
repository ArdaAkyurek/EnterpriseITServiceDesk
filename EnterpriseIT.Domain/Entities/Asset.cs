using EnterpriseIT.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseIT.Domain.Entities
{
    public class Asset : BaseEntity
    {
        public int AssetId { get; set; }

        public string AssetCode { get; set; } = string.Empty;

        public string Brand { get; set; } = string.Empty;

        public string Model { get; set; } = string.Empty;

        public string SerialNumber { get; set; } = string.Empty;

        public DateTime PurchaseDate { get; set; }

        public DateTime WarrantyEndDate { get; set; }

        public string Status { get; set; } = string.Empty;

        // Foreign Key
        public int AssetTypeId { get; set; }

        // Navigation Property
        public AssetType AssetType { get; set; } = null!;

        public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();

        public ICollection<AssetAssignment> Assignments { get; set; } = new List<AssetAssignment>();

        public ICollection<AssetMaintenance> Maintenances { get; set; } = new List<AssetMaintenance>();
        
    }
}
