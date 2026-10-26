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
    public class TicketConfiguration : IEntityTypeConfiguration<Ticket>
    {
        public void Configure(EntityTypeBuilder<Ticket> builder)
        {
            // Primary Key
            builder.HasKey(x => x.TicketId);

            // Table Name
            builder.ToTable("Tickets");

            // Properties
            builder.Property(x => x.Title)
                   .IsRequired()
                   .HasMaxLength(150);

            builder.Property(x => x.Description)
                   .IsRequired();

            builder.Property(x => x.ClosedAt);

            // Relationships

            builder.HasOne(x => x.CreatedByUser)
                   .WithMany(x => x.CreatedTickets)
                   .HasForeignKey(x => x.CreatedByUserId);

            builder.HasOne(x => x.AssignedToUser)
                   .WithMany(x => x.AssignedTickets)
                   .HasForeignKey(x => x.AssignedToUserId);

            builder.HasOne(x => x.Category)
                   .WithMany(x => x.Tickets)
                   .HasForeignKey(x => x.CategoryId);

            builder.HasOne(x => x.Priority)
                   .WithMany(x => x.Tickets)
                   .HasForeignKey(x => x.PriorityId);

            builder.HasOne(x => x.Status)
                   .WithMany(x => x.Tickets)
                   .HasForeignKey(x => x.StatusId);

            builder.HasOne(x => x.Asset)
                   .WithMany(x => x.Tickets)
                   .HasForeignKey(x => x.AssetId);

            builder.HasOne(x => x.CreatedByUser)
                   .WithMany(x => x.CreatedTickets)
                   .HasForeignKey(x => x.CreatedByUserId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}