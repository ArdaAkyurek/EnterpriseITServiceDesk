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
    public class TicketAttachmentConfiguration : IEntityTypeConfiguration<TicketAttachment>
    {
        public void Configure(EntityTypeBuilder<TicketAttachment> builder)
        {
            // Primary Key
            builder.HasKey(x => x.AttachmentId);

            // Table Name
            builder.ToTable("TicketAttachments");

            // Properties
            builder.Property(x => x.FileName)
                   .IsRequired()
                   .HasMaxLength(255);

            builder.Property(x => x.FilePath)
                   .IsRequired()
                   .HasMaxLength(500);

            // Relationships

            builder.HasOne(x => x.Ticket)
                   .WithMany(x => x.Attachments)
                   .HasForeignKey(x => x.TicketId);

            builder.HasOne(x => x.UploadedByUser)
                   .WithMany(x => x.UploadedAttachments)
                   .HasForeignKey(x => x.UploadedByUserId);
        }
    }
}