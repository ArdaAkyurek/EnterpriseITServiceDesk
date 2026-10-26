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
            builder.Property(x => x.PriorityName)
                   .IsRequired()
                   .HasMaxLength(50);

            builder.Property(x => x.Description)
                   .HasMaxLength(250);

            builder.HasData(
    new TicketPriority
    {
        TicketPriorityId = 1,
        PriorityName = "Low",
        Description = "Low Priority"
    },
    new TicketPriority
    {
        TicketPriorityId = 2,
        PriorityName = "Medium",
        Description = "Medium Priority"
    },
    new TicketPriority
    {
        TicketPriorityId = 3,
        PriorityName = "High",
        Description = "High Priority"
    }
);
        }
    }
}