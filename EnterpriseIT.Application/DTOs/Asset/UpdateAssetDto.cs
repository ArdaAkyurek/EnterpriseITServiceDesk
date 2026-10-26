using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseIT.Application.DTOs.Asset
{
    public class UpdateAssetDto
    {
        public string AssetCode { get; set; } = string.Empty;

        public string Brand { get; set; } = string.Empty;

        public string Model { get; set; } = string.Empty;

        public string SerialNumber { get; set; } = string.Empty;

        public DateTime PurchaseDate { get; set; }

        public DateTime WarrantyEndDate { get; set; }

        public string Status { get; set; } = string.Empty;

        public int AssetTypeId { get; set; }
    }
}