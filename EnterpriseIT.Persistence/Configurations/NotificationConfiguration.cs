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
    public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
    {
        public void Configure(EntityTypeBuilder<Notification> builder)
        {
            // Primary Key
            builder.HasKey(x => x.NotificationId);

            // Table Name
            builder.ToTable("Notifications");

            // Properties
            builder.Property(x => x.Title)
                   .IsRequired()
                   .HasMaxLength(150);

            builder.Property(x => x.Message)
                   .IsRequired()
                   .HasMaxLength(1000);

            builder.Property(x => x.IsRead)
                   .IsRequired();

            // Relationship
            builder.HasOne(x => x.User)
                   .WithMany(x => x.Notifications)
                   .HasForeignKey(x => x.UserId);
        }
    }
}