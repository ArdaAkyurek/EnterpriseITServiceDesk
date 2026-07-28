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
    public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
    {
        public void Configure(EntityTypeBuilder<AuditLog> builder)
        {
            // Primary Key
            builder.HasKey(x => x.AuditLogId);

            // Table Name
            builder.ToTable("AuditLogs");

            // Properties
            builder.Property(x => x.Action)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.Property(x => x.EntityName)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.Property(x => x.EntityId)
                   .IsRequired();

            // Relationship
            builder.HasOne(x => x.User)
                   .WithMany(x => x.AuditLogs)
                   .HasForeignKey(x => x.UserId);
        }
    }
}