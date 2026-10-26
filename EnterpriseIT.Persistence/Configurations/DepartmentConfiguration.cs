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
    public class DepartmentConfiguration : IEntityTypeConfiguration<Department>
    {
        public void Configure(EntityTypeBuilder<Department> builder)
        {
            builder.HasKey(x => x.DepartmentId);

            builder.ToTable("Departments");

            builder.Property(x => x.DepartmentName)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.Property(x => x.DepartmentDescription)
                   .HasMaxLength(250);

            builder.HasData(
                new Department
                {
                    DepartmentId = 1,
                    DepartmentName = "IT",
                    DepartmentDescription = "Information Technology"
                },
                new Department
                {
                    DepartmentId = 2,
                    DepartmentName = "Human Resources",
                    DepartmentDescription = "HR Department"
                },
                new Department
                {
                    DepartmentId = 3,
                    DepartmentName = "Finance",
                    DepartmentDescription = "Finance Department"
                }
            );
        }
    }
}
