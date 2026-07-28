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
    public class TicketCategoryConfiguration : IEntityTypeConfiguration<TicketCategory>
    {
        public void Configure(EntityTypeBuilder<TicketCategory> builder)
        {
            // Primary Key
            builder.HasKey(x => x.TicketCategoryId);

            // Table Name
            builder.ToTable("TicketCategories");

            // Properties
            builder.Property(x => x.TicketCategoryName)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.Property(x => x.Description)
                   .HasMaxLength(250);
        }
    }
}
