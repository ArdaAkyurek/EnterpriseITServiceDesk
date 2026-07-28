using EnterpriseIT.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EnterpriseIT.Persistence.Configurations
{
    public class TicketCommentConfiguration : IEntityTypeConfiguration<TicketComment>
    {
        public void Configure(EntityTypeBuilder<TicketComment> builder)
        {
            // Primary Key
            builder.HasKey(x => x.CommentId);

            // Table Name
            builder.ToTable("TicketComments");

            // Properties
            builder.Property(x => x.Comment)
                   .IsRequired()
                   .HasMaxLength(1000);

            // Relationships

            builder.HasOne(x => x.Ticket)
                   .WithMany(x => x.Comments)
                   .HasForeignKey(x => x.TicketId);

            builder.HasOne(x => x.User)
                   .WithMany(x => x.TicketComments)
                   .HasForeignKey(x => x.UserId);
        }
    }
}