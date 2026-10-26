using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using EnterpriseIT.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EnterpriseIT.Persistence.Configurations
{
    public class AssetTypeConfiguration : IEntityTypeConfiguration<AssetType>
    {
        public void Configure(EntityTypeBuilder<AssetType> builder)
        {
            // Primary Key
            builder.HasKey(x => x.AssetTypeId);

            // Table Name
            builder.ToTable("AssetTypes");

            // Properties
            builder.Property(x => x.Name)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.HasData(
    new AssetType
    {
        AssetTypeId = 1,
        Name = "Laptop"
    },
    new AssetType
    {
        AssetTypeId = 2,
        Name = "Desktop"
    },
    new AssetType
    {
        AssetTypeId = 3,
        Name = "Monitor"
    },
    new AssetType
    {
        AssetTypeId = 4,
        Name = "Printer"
    },
    new AssetType
    {
        AssetTypeId = 5,
        Name = "Server"
    }
);
        }
    }
}
