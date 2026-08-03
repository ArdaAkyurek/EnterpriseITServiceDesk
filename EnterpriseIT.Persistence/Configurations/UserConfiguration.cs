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
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            // Primary Key
            builder.HasKey(x => x.UserId);

            // Table Name
            builder.ToTable("Users");

            // Properties
            builder.Property(x => x.FirstName)
                   .IsRequired()
                   .HasMaxLength(50);

            builder.Property(x => x.LastName)
                   .IsRequired()
                   .HasMaxLength(50);

            builder.Property(x => x.Email)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.Property(x => x.PasswordHash)
                   .IsRequired();

            builder.Property(x => x.Phone)
                   .HasMaxLength(20);

            builder.Property(x => x.IsActive)
                   .HasDefaultValue(true);

            // Unique Index
            builder.HasIndex(x => x.Email)
                   .IsUnique();

            // Relationships
            builder.HasOne(x => x.Role)
                   .WithMany(x => x.Users)
                   .HasForeignKey(x => x.RoleId)
                   .OnDelete(DeleteBehavior.Restrict);
                   

            builder.HasOne(x => x.Department)
                   .WithMany(x => x.Users)
                   .HasForeignKey(x => x.DepartmentId);
        }
    }
}