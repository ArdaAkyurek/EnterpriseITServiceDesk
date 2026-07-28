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
    public class AssetAssignmentConfiguration : IEntityTypeConfiguration<AssetAssignment>
    {
        public void Configure(EntityTypeBuilder<AssetAssignment> builder)
        {
            // Primary Key
            builder.HasKey(x => x.AssetAssignmentId);

            // Table Name
            builder.ToTable("AssetAssignments");

            // Properties
            builder.Property(x => x.AssignedDate)
                   .IsRequired();

            builder.Property(x => x.ReturnedDate);

            // Relationships
            builder.HasOne(x => x.Asset)
                   .WithMany(x => x.Assignments)
                   .HasForeignKey(x => x.AssetId);

            builder.HasOne(x => x.User)
                   .WithMany(x => x.AssetAssignments)
                   .HasForeignKey(x => x.UserId);
        }
    }
}