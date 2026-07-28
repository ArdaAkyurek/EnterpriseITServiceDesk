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
    public class TicketPriorityConfiguration : IEntityTypeConfiguration<TicketPriority>
    {
        public void Configure(EntityTypeBuilder<TicketPriority> builder)
        {
            // Primary Key
            builder.HasKey(x => x.TicketPriorityId);

            // Table Name
            builder.ToTable("TicketPriorities");

            // Properties
            builder.Property(x => x.TicketPriorityName)
                   .IsRequired()
                   .HasMaxLength(50);

            builder.Property(x => x.Description)
                   .HasMaxLength(250);
        }
    }
}