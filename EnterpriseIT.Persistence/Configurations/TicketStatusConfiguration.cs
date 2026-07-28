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
        }
    }
}