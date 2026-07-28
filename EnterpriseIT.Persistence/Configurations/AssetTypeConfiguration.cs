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
        }
    }
}
