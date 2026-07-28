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
    public class AssetMaintenanceConfiguration : IEntityTypeConfiguration<AssetMaintenance>
    {
        public void Configure(EntityTypeBuilder<AssetMaintenance> builder)
        {
            // Primary Key
            builder.HasKey(x => x.MaintenanceId);

            // Table Name
            builder.ToTable("AssetMaintenances");

            // Properties
            builder.Property(x => x.Description)
                   .IsRequired()
                   .HasMaxLength(500);

            builder.Property(x => x.Cost)
                   .HasColumnType("decimal(18,2)");

            builder.Property(x => x.MaintenanceDate)
                   .IsRequired();

            // Relationships
            builder.HasOne(x => x.Asset)
             .WithMany(x => x.Maintenances)
             .HasForeignKey(x => x.AssetId);

            builder.HasOne(x => x.PerformedByUser)
                   .WithMany(x => x.PerformedMaintenances)
                   .HasForeignKey(x => x.PerformedByUserId);
        }
    }
}