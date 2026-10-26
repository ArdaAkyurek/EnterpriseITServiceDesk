using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel.DataAnnotations;

namespace EnterpriseIT.Application.DTOs.Asset
{
    public class CreateAssetDto
    {
        [Required, StringLength(50)]
        public string AssetCode { get; set; } = string.Empty;

        [Required, StringLength(100)]
        public string Brand { get; set; } = string.Empty;

        [Required, StringLength(100)]
        public string Model { get; set; } = string.Empty;

        [Required, StringLength(100)]
        public string SerialNumber { get; set; } = string.Empty;

        public DateTime PurchaseDate { get; set; }

        public DateTime WarrantyEndDate { get; set; }

        [Required, StringLength(50)]
        public string Status { get; set; } = string.Empty;

        [Range(1, int.MaxValue)]
        public int AssetTypeId { get; set; }
    }
}
