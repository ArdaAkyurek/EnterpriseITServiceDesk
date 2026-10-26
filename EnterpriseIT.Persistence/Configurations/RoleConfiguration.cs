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
    public class RoleConfiguration : IEntityTypeConfiguration<Role>
    {

        public void Configure(EntityTypeBuilder<Role> builder)
        {

            // Primary Key
            builder.HasKey(x=>x.RoleId);
            // Table Name
            builder.ToTable("Roles");

            //proporties

            builder.Property(x=>x.RoleName)
                .IsRequired()
                .HasMaxLength(50);
            builder.Property(x => x.Description)
                .HasMaxLength(250);

            builder.HasData(
    new Role
    {
        RoleId = 1,
        RoleName = "Admin"
    },
    new Role
    {
        RoleId = 2,
        RoleName = "Technician"
    },
    new Role
    {
        RoleId = 3,
        RoleName = "Employee"
    }
);

        }
    }
}
