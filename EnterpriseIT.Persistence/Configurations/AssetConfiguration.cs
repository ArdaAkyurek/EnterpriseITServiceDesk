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
    public class AssetConfiguration : IEntityTypeConfiguration<Asset>
    {
        public void Configure(EntityTypeBuilder<Asset> builder)
        {
            // Primary Key
            builder.HasKey(x => x.AssetId);

            // Table Name
            builder.ToTable("Assets");

            // Properties
            builder.Property(x => x.AssetCode)
                   .IsRequired()
                   .HasMaxLength(50);

            builder.Property(x => x.Brand)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.Property(x => x.Model)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.Property(x => x.SerialNumber)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.Property(x => x.PurchaseDate)
                   .IsRequired();

            builder.Property(x => x.WarrantyEndDate)
                   .IsRequired();

            builder.Property(x => x.Status)
                   .IsRequired()
                   .HasMaxLength(50);

            // Relationships

            builder.HasOne(x => x.AssetType)
                   .WithMany(x => x.Assets)
                   .HasForeignKey(x => x.AssetTypeId);
        }
    }
}