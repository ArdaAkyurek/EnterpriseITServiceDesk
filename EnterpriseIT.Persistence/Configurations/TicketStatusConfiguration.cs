using EnterpriseIT.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EnterpriseIT.Persistence.Configurations
{
    public class TicketStatusConfiguration : IEntityTypeConfiguration<TicketStatus>
    {
        public void Configure(EntityTypeBuilder<TicketStatus> builder)
        {
            // Primary Key
            builder.HasKey(x => x.TicketStatusId);

            // Table Name
            builder.ToTable("TicketStatuses");

            // Properties
            builder.Property(x => x.StatusName)
                   .IsRequired()
                   .HasMaxLength(50);

            builder.Property(x => x.StatusDescription)
                   .HasMaxLength(250);


            builder.HasData(
    new TicketStatus
    {
        TicketStatusId = 1,
        StatusName = "Open",
        StatusDescription = "Newly created ticket"
    },
    new TicketStatus
    {
        TicketStatusId = 2,
        StatusName = "In Progress",
        StatusDescription = "Work in progress"
    },
    new TicketStatus
    {
        TicketStatusId = 3,
        StatusName = "Closed",
        StatusDescription = "Issue resolved"
    }
);
        }
    }
}