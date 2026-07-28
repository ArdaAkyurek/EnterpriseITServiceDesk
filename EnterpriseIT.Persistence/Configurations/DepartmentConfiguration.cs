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
            builder.HasKey(x=>x.DepartmentId);



            builder.ToTable("Departments");



            builder.Property(x => x.DepartmentName)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(x => x.DepartmentDescription)
                .HasMaxLength(250);

        }
    }
}
